using System.Runtime.InteropServices;
using System.Text;

namespace BinaryControl;
public class FileManager : IDisposable
{
    private FileStream? _productStream;
    private FileStream? _specStream;
    private ProductFileHeader _productHeader;
    private SpecFileHeader _specHeader;
    private string _productFilePath = "";
    private string _specFilePath = "";

    private readonly List<ProductInfo> _products = [];
    private readonly List<SpecInfo> _specs = [];

    private int _nextTempId = -1;

    public bool IsOpen { get; private set; }
    public IReadOnlyList<ProductInfo> Products => _products.AsReadOnly();
    public IReadOnlyList<SpecInfo> Specs => _specs.AsReadOnly();
    
    public IEnumerable<SpecInfo> GetSpecsForProduct(int productOffset) =>
        _specs.Where(s => s.OwnerOffset == productOffset && !s.IsDeleted);
    
    public void Reload()
    {
        if (!IsOpen) throw new InvalidOperationException("База не открыта");
        _products.Clear();
        _specs.Clear();
        LoadFromFiles();
    }

    public void UpdateProduct(ProductInfo updatedProduct)
    {
        var prod = _products.FirstOrDefault(p => p.FileOffset == updatedProduct.FileOffset);
        if (prod != null)
        {
            prod.Name = updatedProduct.Name;
            prod.Type = updatedProduct.Type;
            prod.SpecFilePtr = updatedProduct.Type == ComponentType.Detail ? -1 : 0;
        }
    } 

    public void UpdateSpec(SpecInfo updatedSpec)
    {
        var spec = _specs.FirstOrDefault(s => s.FileOffset == updatedSpec.FileOffset);
        if (spec != null)
        {
            spec.Multiplicity = updatedSpec.Multiplicity;
        }
    }

    public void Dispose() => CloseFiles();

    public void CreateDatabase(string productName, short nameLength = Sizes.DefaultNameLength)
    {
        CloseFiles();
        _products.Clear();
        _specs.Clear();
        _nextTempId = -1;

        _productFilePath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\Binfile\\" + productName + ".prd";
        _specFilePath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\Binfile\\" + productName + ".prs";

        _productHeader = ProductFileHeader.Create(Path.GetFileName(_specFilePath), nameLength);
        _specHeader = SpecFileHeader.Create();

        _productStream = new FileStream(_productFilePath, FileMode.Create, FileAccess.ReadWrite);
        _specStream = new FileStream(_specFilePath, FileMode.Create, FileAccess.ReadWrite);

        WriteProductHeader();
        WriteSpecHeader();

        IsOpen = true;
    }

    public void OpenDatabase(string productName)
    {
        CloseFiles();
        _products.Clear();
        _specs.Clear();
        _nextTempId = -1;

        _productFilePath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\Binfile\\" + productName + ".prd";
        _specFilePath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\Binfile\\" + productName + ".prs";

        if (!File.Exists(_productFilePath)) throw new Exception($"Файл {_productFilePath} не найден");
        if (!File.Exists(_specFilePath)) throw new Exception($"Файл спецификаций {_specFilePath} не найден");

        _productStream = new FileStream( _productFilePath, FileMode.Open, FileAccess.ReadWrite);
        _specStream = new FileStream(_specFilePath, FileMode.Open, FileAccess.ReadWrite);

        ReadProductHeader();
        ReadSpecHeader();

        if (_productHeader.Signature[0] != 'P' || _productHeader.Signature[1] != 'S')
            throw new Exception("Неверная сигнатура файла");

        LoadFromFiles();
        IsOpen = true;
    }

    private void LoadFromFiles()
    {
        int ptr = _productHeader.FirstRecordPtr;
        var visited = new HashSet<int>();
        int index = 0;
        while (ptr != -1 && ptr < _productStream!.Length && !visited.Contains(ptr))
        {
            visited.Add(ptr);
            _productStream.Position = ptr;
            byte[] recordBytes = new byte[Marshal.SizeOf<ProductRecord>()];
            _productStream.ReadExactly(recordBytes);
            var record = ByteArrayToStructure<ProductRecord>(recordBytes);

            byte[] nameBytes = new byte[_productHeader.DataLength];
            _productStream.ReadExactly(nameBytes);
            string name = Encoding.ASCII.GetString(nameBytes).TrimEnd();

            ComponentType type;
            if (index == 0)
                type = ComponentType.Product;
            else
                type = record.SpecFilePtr == -1 ? ComponentType.Detail : ComponentType.Node;

            _products.Add(new ProductInfo
            {
                FileOffset = ptr,
                Name = name,
                SpecFilePtr = record.SpecFilePtr,
                IsDeleted = record.IsDeleted == -1,
                Type = type
            });
            ptr = record.NextRecordPtr;
            index++;
        }

        _specStream!.Position = Marshal.SizeOf<SpecFileHeader>();
        while (_specStream.Position + Marshal.SizeOf<SpecRecord>() <= _specStream.Length)
        {
            long startPos = _specStream.Position;
            byte[] recBytes = new byte[Marshal.SizeOf<SpecRecord>()];
            _specStream.ReadExactly(recBytes);
            var rec = ByteArrayToStructure<SpecRecord>(recBytes);

            _specs.Add(new SpecInfo
            {
                FileOffset = (int)startPos,
                ProductFilePtr = rec.ProductFilePtr,
                Multiplicity = rec.Multiplicity,
                NextRecordPtr = rec.NextRecordPtr,
                IsDeleted = rec.IsDeleted == -1,
                OwnerOffset = -1
            });
        }

        foreach (var prod in _products)
        {
            if (prod.SpecFilePtr == -1) continue;
            int specPtr = prod.SpecFilePtr;
            int count = 0;
            while (specPtr != -1 && count < 10000)
            {
                var spec = _specs.FirstOrDefault(s => s.FileOffset == specPtr);
                if (spec == null) break;
                spec.OwnerOffset = prod.FileOffset;
                specPtr = spec.NextRecordPtr;
                count++;
            }
        }
    }

    private void SaveChanges()
    {
        if (!IsOpen) return;

        var activeProducts = _products.Where(p => !p.IsDeleted).ToList();
        var activeSpecs = _specs.Where(s => !s.IsDeleted).ToList();

        _productStream?.Close();
        _specStream?.Close();

        _productStream = new FileStream(_productFilePath, FileMode.Create, FileAccess.ReadWrite);
        _specStream = new FileStream(_specFilePath, FileMode.Create, FileAccess.ReadWrite);

        _productHeader = ProductFileHeader.Create(Path.GetFileName(_specFilePath), _productHeader.DataLength);
        _specHeader = SpecFileHeader.Create();
        WriteProductHeader();
        WriteSpecHeader();
        
        var productEntry = activeProducts.FirstOrDefault(p => p.Type == ComponentType.Product);
        var nodesAndDetails = activeProducts.Where(p => p.Type != ComponentType.Product)
                                            .OrderBy(p => p.Name).ToList();
        var orderedProducts = new List<ProductInfo>();
        if (productEntry != null)
            orderedProducts.Add(productEntry);
        orderedProducts.AddRange(nodesAndDetails);

        var productNameToNewOffset = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var productOldToNew = new Dictionary<int, int>();
        
        foreach (var prod in orderedProducts)
        {
            int recordOffset = (int)_productStream!.Position;
            productNameToNewOffset[prod.Name] = recordOffset;
            productOldToNew[prod.FileOffset] = recordOffset;

            var record = new ProductRecord
            {
                IsDeleted = 0,
                SpecFilePtr = -1,
                NextRecordPtr = -1
            };
            byte[] recBytes = StructureToByteArray(record);
            _productStream.Write(recBytes, 0, recBytes.Length);

            byte[] nameBytes = Encoding.ASCII.GetBytes(prod.Name.PadRight(_productHeader.DataLength)[.._productHeader.DataLength]);
            _productStream.Write(nameBytes, 0, nameBytes.Length);
        }
        
        var productOffsets = orderedProducts.Select(p => productNameToNewOffset[p.Name]).ToList();
        for (int i = 0; i < productOffsets.Count; i++)
        {
            int nextPtr = (i < productOffsets.Count - 1) ? productOffsets[i + 1] : -1;
            _productStream!.Position = productOffsets[i] + 1 + 4; // IsDeleted + SpecFilePtr
            byte[] nextBytes = BitConverter.GetBytes(nextPtr);
            _productStream.Write(nextBytes, 0, 4);
        }

        _productHeader.FirstRecordPtr = productOffsets.Count > 0 ? productOffsets[0] : -1;
        _productHeader.FreeAreaPtr = (int)_productStream!.Position;
        
        var specsByOwner = activeSpecs
            .Where(s => productOldToNew.ContainsKey(s.OwnerOffset) && productOldToNew.ContainsKey(s.ProductFilePtr))
            .GroupBy(s => s.OwnerOffset)
            .ToDictionary(g => g.Key, g => g.ToList());
        
        var ownerRecordOffsets = new Dictionary<int, List<int>>();
        foreach (var ownerOldOffset in specsByOwner.Keys)
        {
            var specList = specsByOwner[ownerOldOffset];
            var offsets = new List<int>();

            foreach (var spec in specList)
            {
                int recOffset = (int)_specStream!.Position;
                offsets.Add(recOffset);

                int newCompOffset = productOldToNew[spec.ProductFilePtr];

                var specRecord = new SpecRecord
                {
                    IsDeleted = 0,
                    ProductFilePtr = newCompOffset,
                    Multiplicity = spec.Multiplicity,
                    NextRecordPtr = -1
                };
                byte[] recBytes = StructureToByteArray(specRecord);
                _specStream.Write(recBytes, 0, recBytes.Length);
            }

            ownerRecordOffsets[ownerOldOffset] = offsets;
        }
        
        foreach (var kv in ownerRecordOffsets)
        {
            var offsets = kv.Value;
            for (int i = 0; i < offsets.Count; i++)
            {
                int nextPtr = (i < offsets.Count - 1) ? offsets[i + 1] : -1;
                _specStream!.Position = offsets[i] + 1 + 4 + 2; // IsDeleted + ProductFilePtr + Multiplicity
                byte[] nextBytes = BitConverter.GetBytes(nextPtr);
                _specStream.Write(nextBytes, 0, 4);
            }
        }

        foreach (var prod in orderedProducts.Where(p => p.Type != ComponentType.Detail))
        {
            int newProdOffset = productOldToNew[prod.FileOffset];
            int firstSpecOffset = -1;

            if (ownerRecordOffsets.TryGetValue(prod.FileOffset, out var offsets) && offsets.Count > 0)
                firstSpecOffset = offsets[0];

            _productStream!.Position = newProdOffset + 1; // после IsDeleted
            byte[] ptrBytes = BitConverter.GetBytes(firstSpecOffset);
            _productStream.Write(ptrBytes, 0, 4);
        }

        if (_specStream.Length > Marshal.SizeOf<SpecFileHeader>())
            _specHeader.FirstRecordPtr = Marshal.SizeOf<SpecFileHeader>();
        else
            _specHeader.FirstRecordPtr = -1;
        _specHeader.FreeAreaPtr = (int)_specStream.Length;
        
        WriteSpecHeader();
        WriteProductHeader();

        _products.Clear();
        _specs.Clear();
        _nextTempId = -1;
        LoadFromFiles();
    }

    public int AddProduct(string name, ComponentType type)
    {
        if (!IsOpen) throw new Exception("База данных не открыта");
        if (_products.Any(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase) && !p.IsDeleted))
            throw new Exception("Компонент с таким именем уже существует");

        if (type == ComponentType.Product && _products.Any(p => !p.IsDeleted && p.Type == ComponentType.Product))
            throw new Exception("Изделие может быть только одно");

        int tempOffset = _nextTempId--;
        var product = new ProductInfo
        {
            FileOffset = tempOffset,
            Name = name,
            Type = type,
            SpecFilePtr = type == ComponentType.Detail ? -1 : 0,
            IsDeleted = false
        };
        _products.Add(product);
        return tempOffset;
    }

    public void AddToSpecification(int productOffset, int componentOffset, short multiplicity)
    {
        if (!IsOpen) throw new Exception("База данных не открыта");

        var product = _products.FirstOrDefault(p => p.FileOffset == productOffset && !p.IsDeleted)
            ?? throw new Exception("Продукт не найден");
        if (product.Type == ComponentType.Detail)
            throw new Exception("Нельзя добавить в спецификацию детали");

        var _ = _products.FirstOrDefault(p => p.FileOffset == componentOffset && !p.IsDeleted)
            ?? throw new Exception("Комплектующее не найдено");

        if (_specs.Any(s => !s.IsDeleted && s.OwnerOffset == productOffset && s.ProductFilePtr == componentOffset))
            throw new Exception("Компонент уже есть в спецификации");

        _specs.Add(new SpecInfo
        {
            FileOffset = _nextTempId--,
            ProductFilePtr = componentOffset,
            Multiplicity = multiplicity,
            NextRecordPtr = -1,
            IsDeleted = false,
            OwnerOffset = productOffset
        });
    }

    public ProductInfo? FindProductByName(string name) =>
        _products.FirstOrDefault(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase) && !p.IsDeleted);

    public void LogicalDeleteProduct(string name)
    {
        var prod = FindProductByName(name) ?? throw new InvalidOperationException($"Компонент '{name}' не найден");
        if (HasReferences(prod.FileOffset))
            throw new Exception($"На компонент '{name}' есть ссылки в спецификациях");
        prod.IsDeleted = true;
    }

    public void RestoreProduct(string name)
    {
        var prods = name == "*" ? _products : _products.Where(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        foreach (var p in prods) p.IsDeleted = false;
    }

    public void RestoreAll() => RestoreProduct("*");

    public void Truncate() => SaveChanges();

    public void PrintAllProducts()
    {
        var toPrint = _products.Where(p => !p.IsDeleted).OrderBy(p => p.Name);
        foreach (var p in toPrint)
        {
            Console.WriteLine($"{p.Name} --- {p.Type}");
        }
    }

    public void PrintSpecification(string name)
    {
        var prod = FindProductByName(name) ?? throw new InvalidOperationException($"Компонент '{name}' не найден");
        if (prod.Type == ComponentType.Detail) throw new InvalidOperationException("Нельзя вывести спецификацию детали");
        Console.WriteLine($"{prod.Name} ({prod.Type})");
        PrintSpecRecursive(prod, 0);
    }

    private void PrintSpecRecursive(ProductInfo product, int indent)
    {
        if (indent > 100) { Console.WriteLine($"{new string(' ', indent * 2)}... (глубокая вложенность)"); return; }
        // if (!visited.Add(product.FileOffset))
        // {
        //     Console.WriteLine($"{new string('|', indent)} {product.Name} (циклическая ссылка)");
        //     return;
        // }

        // string prefix = indent > 0 ? new string('|', indent - 1) + " " : "";
        // Console.WriteLine($"{prefix}{product.Name} ({product.Type})");

        var specs = _specs.Where(s => !s.IsDeleted && s.OwnerOffset == product.FileOffset).ToList();
        foreach (var spec in specs)
        {
            var comp = _products.FirstOrDefault(p => p.FileOffset == spec.ProductFilePtr);
            if (comp == null) continue;

            string mult = spec.Multiplicity > 1 ? $" x{spec.Multiplicity}" : "";
            string childPrefix = new string('|', indent) + " ";
            Console.WriteLine($"{childPrefix}{comp.Name} ({comp.Type}){mult}");

            if (comp.SpecFilePtr != -1)
                PrintSpecRecursive(comp, indent + 1);
        }
    }

    private bool HasReferences(int productOffset) =>
        _specs.Any(s => !s.IsDeleted && s.ProductFilePtr == productOffset);

    private void CloseFiles()
    {
        if (IsOpen) SaveChanges();
        _productStream?.Flush(); _productStream?.Close(); _productStream = null;
        _specStream?.Flush(); _specStream?.Close(); _specStream = null;
        IsOpen = false;
    }

    private void ReadProductHeader()
    {
        _productStream!.Position = 0;
        byte[] buffer = new byte[Marshal.SizeOf<ProductFileHeader>()];
        _productStream.ReadExactly(buffer);
        _productHeader = ByteArrayToStructure<ProductFileHeader>(buffer);
    }

    private void WriteProductHeader()
    {
        if (_productStream == null) return;
        _productStream.Position = 0;
        byte[] buffer = StructureToByteArray(_productHeader);
        _productStream.Write(buffer, 0, buffer.Length);
    }

    private void ReadSpecHeader()
    {
        _specStream!.Position = 0;
        byte[] buffer = new byte[Marshal.SizeOf<SpecFileHeader>()];
        _specStream.ReadExactly(buffer);
        _specHeader = ByteArrayToStructure<SpecFileHeader>(buffer);
    }

    private void WriteSpecHeader()
    {
        if (_specStream == null) return;
        _specStream.Position = 0;
        byte[] buffer = StructureToByteArray(_specHeader);
        _specStream.Write(buffer, 0, buffer.Length);
    }

    private T ByteArrayToStructure<T>(byte[] bytes) where T : struct
    {
        GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
        try
        {
            return Marshal.PtrToStructure<T>(handle.AddrOfPinnedObject());
        }
        finally
        {
            handle.Free();
        }
    }

    private static byte[] StructureToByteArray<T>(T structure) where T : struct
    {
        var size = Marshal.SizeOf<T>();
        var bytes = new byte[size];
        var handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
        try
        {
            Marshal.StructureToPtr(structure, handle.AddrOfPinnedObject(), false);
        }
        finally
        {
            handle.Free();
        }
        return bytes;
    }

    // private bool IsComponentInSpecification(int specHeaderOffset, int componentOffset)
    // {
    //     _specStream!.Position = specHeaderOffset;
    //     byte[] headerBytes = new byte[Marshal.SizeOf<SpecFileHeader>()];
    //     _specStream.ReadExactly(headerBytes);
    //     var specHeader = ByteArrayToStructure<SpecFileHeader>(headerBytes);
    //
    //     int ptr = specHeader.FirstRecordPtr;
    //     int count = 0;
    //     while (ptr != -1 && ptr < _specStream.Length && count < 10000)
    //     {
    //         _specStream.Position = ptr;
    //         byte[] recordBytes = new byte[Marshal.SizeOf<SpecRecord>()];
    //         _specStream.ReadExactly(recordBytes);
    //         var record = ByteArrayToStructure<SpecRecord>(recordBytes);
    //
    //         if (record.IsDeleted == 0 && record.ProductFilePtr == componentOffset)
    //             return true;
    //
    //         ptr = record.NextRecordPtr;
    //         count++;
    //     }
    //
    //     return false;
    // }

    // private List<ProductInfo> GetAllProducts()
    // {
    //     var products = new List<ProductInfo>();
    //     if (!IsOpen) return products;
    //
    //     var visitedOffsets = new HashSet<int>();
    //     int ptr = _productHeader.FirstRecordPtr;
    //     while (ptr != -1 && ptr < _productStream!.Length && !visitedOffsets.Contains(ptr))
    //     {
    //         visitedOffsets.Add(ptr);
    //         var product = ReadProduct(ptr);
    //         products.Add(product);
    //         ptr = GetNextProductPtr(ptr);
    //
    //         if (products.Count > 10000) break;
    //     }
    //     return products;
    // }

    // private int GetNextProductPtr(int offset)
    // {
    //     _productStream!.Position = offset + 1 + 4;
    //     byte[] ptrBytes = new byte[4];
    //     _productStream.ReadExactly(ptrBytes, 0, 4);
    //     return BitConverter.ToInt32(ptrBytes, 0);
    // }

    // private void SetProductDeleted(int offset, bool deleted)
    // {
    //     _productStream!.Position = offset;
    //     byte[] recordBytes = new byte[Marshal.SizeOf<ProductRecord>()];
    //     _productStream.ReadExactly(recordBytes);
    //     var record = ByteArrayToStructure<ProductRecord>(recordBytes);
    //     record.IsDeleted = (sbyte)(deleted ? -1 : 0);
    //
    //     _productStream.Position = offset;
    //     byte[] newRecordBytes = StructureToByteArray(record);
    //     _productStream.Write(newRecordBytes, 0, newRecordBytes.Length);
    // }

    // private List<(int ownerOffset, SpecInfo spec)> GetAllSpecifications()
    // {
    //     var result = new List<(int, SpecInfo)>();
    //     if (!IsOpen) return result;
    //
    //     var products = GetAllProducts();
    //     foreach (var product in products)
    //     {
    //         if (product.SpecFilePtr == -1) continue;
    //
    //         _specStream!.Position = product.SpecFilePtr;
    //         byte[] headerBytes = new byte[Marshal.SizeOf<SpecFileHeader>()];
    //         _specStream.ReadExactly(headerBytes);
    //         var header = ByteArrayToStructure<SpecFileHeader>(headerBytes);
    //
    //         int ptr = header.FirstRecordPtr;
    //         int count = 0;
    //         while (ptr != -1 && ptr < _specStream.Length && count < 10000)
    //         {
    //             _specStream.Position = ptr;
    //             byte[] recordBytes = new byte[Marshal.SizeOf<SpecRecord>()];
    //             _specStream.ReadExactly(recordBytes);
    //             var record = ByteArrayToStructure<SpecRecord>(recordBytes);
    //
    //             if (record.IsDeleted == 0)
    //             {
    //                 result.Add((product.FileOffset, new SpecInfo
    //                 {
    //                     FileOffset = ptr,
    //                     ProductFilePtr = record.ProductFilePtr,
    //                     Multiplicity = record.Multiplicity,
    //                     NextRecordPtr = record.NextRecordPtr,
    //                     IsDeleted = record.IsDeleted == -1
    //                 }));
    //             }
    //
    //             ptr = record.NextRecordPtr;
    //             count++;
    //         }
    //     }
    //     return result;
    // }
}