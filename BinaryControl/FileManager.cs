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

    public bool IsOpen { get; private set; } = false;

    public void Dispose() => CloseFiles();

    public void CreateDatabase(string productName, string? specName = null, short nameLength = Sizes.DefaultNameLength)
    {
        CloseFiles();

        _productFilePath = productName.EndsWith(".prd") ? productName : productName + ".prd";
        _specFilePath = specName ?? _productFilePath.Replace(".prd", ".prs");

        if (!_specFilePath.EndsWith(".prs"))
            _specFilePath = _specFilePath.Replace(Path.GetExtension(_specFilePath), ".prs");

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

        _productFilePath = productName.EndsWith(".prd") ? productName : productName + ".prd";
        _specFilePath = _productFilePath.Replace(".prd", ".prs");

        if (!File.Exists(_productFilePath))
            throw new FileNotFoundException($"Файл {_productFilePath} не найден");

        _productStream = new FileStream(_productFilePath, FileMode.Open, FileAccess.ReadWrite);

        ReadProductHeader();

        if (_productHeader.Signature[0] != 'P' || _productHeader.Signature[1] != 'S')
            throw new InvalidOperationException("Неверная сигнатура файла");

        byte[] specFileNameBytes = _productHeader.SpecFileName;
        string specFileName = Encoding.ASCII.GetString(specFileNameBytes).Trim();

        _specFilePath = Path.Combine(Path.GetDirectoryName(_productFilePath) ?? "", specFileName);

        if (!File.Exists(_specFilePath))
            throw new FileNotFoundException($"Файл спецификаций {_specFilePath} не найден");

        _specStream = new FileStream(_specFilePath, FileMode.Open, FileAccess.ReadWrite);
        ReadSpecHeader();

        IsOpen = true;
    }

    public int AddProduct(string name, ComponentType type)
    {
        if (!IsOpen) throw new InvalidOperationException("База данных не открыта");

        var recordSize = Marshal.SizeOf<ProductRecord>() + _productHeader.DataLength;
        int position;

        if (_productHeader.FreeAreaPtr == -1 || _productHeader.FreeAreaPtr >= _productStream!.Length)
        {
            position = (int)_productStream!.Length;
            _productHeader.FreeAreaPtr = position + recordSize;
        }
        else
        {
            position = _productHeader.FreeAreaPtr;
        }

        var record = new ProductRecord
        {
            IsDeleted = 0,
            SpecFilePtr = (type == ComponentType.Detail) ? -1 : CreateSpecList(),
            NextRecordPtr = _productHeader.FirstRecordPtr
        };

        _productHeader.FirstRecordPtr = position;

        _productStream.Position = position;
        var recordBytes = StructureToByteArray(record);
        _productStream.Write(recordBytes, 0, recordBytes.Length);

        var nameBytes = Encoding.ASCII.GetBytes(name.PadRight(_productHeader.DataLength).Substring(0, _productHeader.DataLength));
        _productStream.Write(nameBytes, 0, nameBytes.Length);

        WriteProductHeader();
        return position;
    }

    public void AddToSpecification(int productOffset, int componentOffset, short multiplicity)
    {
        if (!IsOpen) throw new InvalidOperationException("База данных не открыта");

        var product = ReadProduct(productOffset);
        if (product.SpecFilePtr == -1)
            throw new InvalidOperationException("Нельзя добавить в спецификацию детали");

        int specHeaderOffset = product.SpecFilePtr;
        
        if (IsComponentInSpecification(specHeaderOffset, componentOffset))
            throw new InvalidOperationException($"Компонент уже есть в спецификации");

        // Читаем заголовок спецификации
        _specStream!.Position = specHeaderOffset;
        byte[] headerBytes = new byte[Marshal.SizeOf<SpecFileHeader>()];
        _specStream.ReadExactly(headerBytes);
        var specHeader = ByteArrayToStructure<SpecFileHeader>(headerBytes);

        int recordSize = Marshal.SizeOf<SpecRecord>();
        int position;

        if (specHeader.FreeAreaPtr == -1 || specHeader.FreeAreaPtr >= _specStream.Length)
        {
            position = (int)_specStream.Length;
            specHeader.FreeAreaPtr = position + recordSize;
        }
        else
            position = specHeader.FreeAreaPtr;

        var record = new SpecRecord
        {
            IsDeleted = 0,
            ProductFilePtr = componentOffset,
            Multiplicity = multiplicity,
            NextRecordPtr = specHeader.FirstRecordPtr
        };

        specHeader.FirstRecordPtr = position;

        // Записываем обновленный заголовок
        _specStream.Position = specHeaderOffset;
        byte[] newHeaderBytes = StructureToByteArray(specHeader);
        _specStream.Write(newHeaderBytes, 0, newHeaderBytes.Length);

        // Записываем запись
        _specStream.Position = position;
        byte[] recordBytes = StructureToByteArray(record);
        _specStream.Write(recordBytes, 0, recordBytes.Length);
    }

    private bool IsComponentInSpecification(int specHeaderOffset, int componentOffset)
    {
        _specStream!.Position = specHeaderOffset;
        byte[] headerBytes = new byte[Marshal.SizeOf<SpecFileHeader>()];
        _specStream.ReadExactly(headerBytes);
        var specHeader = ByteArrayToStructure<SpecFileHeader>(headerBytes);

        int ptr = specHeader.FirstRecordPtr;
        int count = 0;
        while (ptr != -1 && ptr < _specStream.Length && count < 10000)
        {
            _specStream.Position = ptr;
            byte[] recordBytes = new byte[Marshal.SizeOf<SpecRecord>()];
            _specStream.ReadExactly(recordBytes);
            var record = ByteArrayToStructure<SpecRecord>(recordBytes);

            if (record.IsDeleted == 0 && record.ProductFilePtr == componentOffset)
                return true;

            ptr = record.NextRecordPtr;
            count++;
        }

        return false;
    }

    public ProductInfo? FindProductByName(string name)
    {
        var products = GetAllProducts();
        return products.FirstOrDefault(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase) && !p.IsDeleted);
    }

    public void LogicalDeleteProduct(string name)
    {
        if (!IsOpen) throw new InvalidOperationException("База данных не открыта");

        var product = FindProductByName(name);
        if (product == null) throw new InvalidOperationException($"Компонент '{name}' не найден");

        // Проверка ссылок
        if (HasReferences(product.FileOffset))
            throw new InvalidOperationException($"На компонент '{name}' есть ссылки в спецификациях");

        SetProductDeleted(product.FileOffset, true);
    }

    public void RestoreProduct(string name)
    {
        if (!IsOpen) throw new InvalidOperationException("База данных не открыта");

        var products = GetAllProducts();
        foreach (var product in products)
        {
            if (name == "*" || product.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
            {
                SetProductDeleted(product.FileOffset, false);
            }
        }

        if (name != "*")
        {
            SortProducts();
        }
    }

    public void RestoreAll() => RestoreProduct("*");

    public void Truncate()
    {
        if (!IsOpen) throw new InvalidOperationException("База данных не открыта");

        // Сохраняем активные продукты и спецификации
        var products = GetAllProducts().Where(p => !p.IsDeleted).OrderBy(p => p.Name).ToList();
        var specs = GetAllSpecifications().Where(s => !s.spec.IsDeleted).ToList();

        CloseFiles();

        CreateDatabase(_productFilePath, _specFilePath);

        // Словарь для маппинга старых offset продуктов на новые
        var offsetMap = new Dictionary<int, int>();
        // Словарь для быстрого поиска нового продукта по имени
        var nameToNewProduct = new Dictionary<string, ProductInfo>(StringComparer.OrdinalIgnoreCase);

        // Записываем продукты
        foreach (var product in products)
        {
            int newOffset = AddProduct(product.Name, product.Type);
            offsetMap[product.FileOffset] = newOffset;
            // После добавления продукт уже можно прочитать (но мы его только что создали)
            var newProduct = ReadProduct(newOffset);
            nameToNewProduct[product.Name] = newProduct;
        }

        // Восстанавливаем спецификации
        foreach (var (ownerOffset, spec) in specs)
        {
            // Ищем владельца по старому offset
            var oldOwner = products.FirstOrDefault(p => p.FileOffset == ownerOffset);
            if (oldOwner == null) continue;

            // Находим нового владельца по имени
            if (!nameToNewProduct.TryGetValue(oldOwner.Name, out var newOwner))
                continue;

            // Маппим компонент спецификации
            if (!offsetMap.TryGetValue(spec.ProductFilePtr, out int newComponentOffset))
                continue;

            // Добавляем запись в спецификацию нового владельца
            AddToSpecification(newOwner.SpecFilePtr, newComponentOffset, spec.Multiplicity);
        }
    }
    
    public void PrintSpecification(string name, int indent = 0)
    {
        if (!IsOpen) throw new InvalidOperationException("База данных не открыта");

        var product = FindProductByName(name);
        if (product == null) throw new InvalidOperationException($"Компонент '{name}' не найден");

        if (product.Type == ComponentType.Detail)
            throw new InvalidOperationException("Нельзя вывести спецификацию детали");

        var visitedProducts = new HashSet<int>();
        PrintSpecRecursive(product, indent, visitedProducts);
    }

    public void PrintAllProducts()
    {
        if (!IsOpen) throw new InvalidOperationException("База данных не открыта");

        var products = GetAllProducts().Where(p => !p.IsDeleted).OrderBy(p => p.Name);
        foreach (var product in products)
            Console.WriteLine($"{product.Name,-15} {product.Type}");
    }

    private ProductInfo ReadProduct(int offset)
    {
        if (!IsOpen) throw new InvalidOperationException("База данных не открыта");

        _productStream!.Position = offset;
        byte[] recordBytes = new byte[Marshal.SizeOf<ProductRecord>()];
        _productStream.ReadExactly(recordBytes);
        var record = ByteArrayToStructure<ProductRecord>(recordBytes);

        byte[] nameBytes = new byte[_productHeader.DataLength];
        _productStream.ReadExactly(nameBytes);
        string name = Encoding.ASCII.GetString(nameBytes).TrimEnd();

        return new ProductInfo
        {
            FileOffset = offset,
            Name = name,
            SpecFilePtr = record.SpecFilePtr,
            IsDeleted = record.IsDeleted == -1,
            Type = record.SpecFilePtr == -1 ? ComponentType.Detail : ComponentType.Node
        };
    }

    private List<ProductInfo> GetAllProducts()
    {
        var products = new List<ProductInfo>();
        if (!IsOpen) return products;

        var visitedOffsets = new HashSet<int>();
        int ptr = _productHeader.FirstRecordPtr;
        while (ptr != -1 && ptr < _productStream!.Length && !visitedOffsets.Contains(ptr))
        {
            visitedOffsets.Add(ptr);
            var product = ReadProduct(ptr);
            products.Add(product);
            ptr = GetNextProductPtr(ptr);

            if (products.Count > 10000) break;
        }
        return products;
    }

    private void CloseFiles()
    {
        if (IsOpen)
        {
            WriteProductHeader();
            WriteSpecHeader();
        }

        _productStream?.Flush();
        _productStream?.Close();
        _productStream = null;

        _specStream?.Flush();
        _specStream?.Close();
        _specStream = null;

        IsOpen = false;
    }

    private int GetNextProductPtr(int offset)
    {
        _productStream!.Position = offset + 1 + 4;
        byte[] ptrBytes = new byte[4];
        _productStream.ReadExactly(ptrBytes, 0, 4);
        return BitConverter.ToInt32(ptrBytes, 0);
    }

    private bool HasReferences(int productOffset)
    {
        _specStream!.Position = 0;
        byte[] headerBytes = new byte[Marshal.SizeOf<SpecFileHeader>()];
        _specStream.ReadExactly(headerBytes);
        var specHeader = ByteArrayToStructure<SpecFileHeader>(headerBytes);

        int ptr = specHeader.FirstRecordPtr;
        while (ptr != -1 && ptr < _specStream.Length)
        {
            _specStream.Position = ptr;
            byte[] recordBytes = new byte[Marshal.SizeOf<SpecRecord>()];
            _specStream.ReadExactly(recordBytes);
            var record = ByteArrayToStructure<SpecRecord>(recordBytes);


            if (record.IsDeleted == 0 && record.ProductFilePtr == productOffset)
                return true;

            ptr = record.NextRecordPtr;
        }

        return false;
    }

    private void SetProductDeleted(int offset, bool deleted)
    {
        _productStream!.Position = offset;
        byte[] recordBytes = new byte[Marshal.SizeOf<ProductRecord>()];
        _productStream.ReadExactly(recordBytes);
        var record = ByteArrayToStructure<ProductRecord>(recordBytes);
        record.IsDeleted = (sbyte)(deleted ? -1 : 0);

        _productStream.Position = offset;
        byte[] newRecordBytes = StructureToByteArray(record);
        _productStream.Write(newRecordBytes, 0, newRecordBytes.Length);
    }

    private void SortProducts()
    {
        var products = GetAllProducts();
        var activeProducts = products.Where(p => !p.IsDeleted).OrderBy(p => p.Name).ToList();
        
        Truncate();
    }

    private List<(int ownerOffset, SpecInfo spec)> GetAllSpecifications()
    {
        var result = new List<(int, SpecInfo)>();
        if (!IsOpen) return result;

        var products = GetAllProducts();
        foreach (var product in products)
        {
            if (product.SpecFilePtr == -1) continue;

            _specStream!.Position = product.SpecFilePtr;
            byte[] headerBytes = new byte[Marshal.SizeOf<SpecFileHeader>()];
            _specStream.ReadExactly(headerBytes);
            var header = ByteArrayToStructure<SpecFileHeader>(headerBytes);

            int ptr = header.FirstRecordPtr;
            int count = 0;
            while (ptr != -1 && ptr < _specStream.Length && count < 10000)
            {
                _specStream.Position = ptr;
                byte[] recordBytes = new byte[Marshal.SizeOf<SpecRecord>()];
                _specStream.ReadExactly(recordBytes);
                var record = ByteArrayToStructure<SpecRecord>(recordBytes);

                if (record.IsDeleted == 0)
                {
                    result.Add((product.FileOffset, new SpecInfo
                    {
                        FileOffset = ptr,
                        ProductFilePtr = record.ProductFilePtr,
                        Multiplicity = record.Multiplicity,
                        NextRecordPtr = record.NextRecordPtr,
                        IsDeleted = record.IsDeleted == -1
                    }));
                }

                ptr = record.NextRecordPtr;
                count++;
            }
        }
        return result;
    }

    private void PrintSpecRecursive(ProductInfo product, int indent, HashSet<int> visitedProducts)
    {
        // Защита от слишком глубокой рекурсии (дополнительно к visited)
        if (indent > 100) 
        {
            Console.WriteLine($"{new string(' ', indent * 2)}... (слишком глубокая вложенность)");
            return;
        }

        if (visitedProducts.Contains(product.FileOffset))
        {
            string prefix = indent > 0 ? new string('|', indent - 1) + " " : "";
            Console.WriteLine($"{prefix}{product.Name} (циклическая ссылка)");
            return;
        }
        visitedProducts.Add(product.FileOffset);

        string indentStr = indent > 0 ? new string('|', indent - 1) + " " : "";
        Console.WriteLine($"{indentStr}{product.Name} ({product.Type})");

        if (product.SpecFilePtr == -1 || product.SpecFilePtr >= _specStream!.Length) 
        {
            visitedProducts.Remove(product.FileOffset); // всегда удаляем при выходе
            return;
        }

        _specStream.Position = product.SpecFilePtr;
        byte[] headerBytes = new byte[Marshal.SizeOf<SpecFileHeader>()];
        _specStream.ReadExactly(headerBytes);
        var specHeader = ByteArrayToStructure<SpecFileHeader>(headerBytes);

        int ptr = specHeader.FirstRecordPtr;
        int count = 0;
        // Защита от зацикливания списка спецификации
        var visitedSpecOffsets = new HashSet<int>(); 

        while (ptr != -1 && ptr < _specStream.Length && count < 1000)
        {
            // Если уже проходили этот offset – зацикливание списка
            if (visitedSpecOffsets.Contains(ptr))
            {
                Console.WriteLine($"{new string(' ', (indent+1)*2)}... (зацикливание списка спецификации)");
                break;
            }
            visitedSpecOffsets.Add(ptr);

            _specStream.Position = ptr;
            byte[] recordBytes = new byte[Marshal.SizeOf<SpecRecord>()];
            _specStream.ReadExactly(recordBytes);
            var record = ByteArrayToStructure<SpecRecord>(recordBytes);

            if (record.IsDeleted == 0)
            {
                var component = ReadProduct(record.ProductFilePtr);
                if (!component.IsDeleted)
                {
                    // Выводим компонент с учётом кратности (один раз, с указанием xN)
                    string mult = record.Multiplicity > 1 ? $" x{record.Multiplicity}" : "";
                    // Для красоты можно вывести дополнительную вертикальную черту
                    string childIndent = new string('|', indent) + " ";
                    Console.WriteLine($"{childIndent}{component.Name} ({component.Type}){mult}");
                    
                    // Если компонент не деталь, рекурсивно выводим его спецификацию
                    if (component.SpecFilePtr != -1)
                    {
                        // Рекурсивный вызов с тем же visitedProducts
                        PrintSpecRecursive(component, indent + 1, visitedProducts);
                    }
                }
            }

            ptr = record.NextRecordPtr;
            count++;
        }

        visitedProducts.Remove(product.FileOffset);
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

    private int CreateSpecList()
    {
        int position = (int)_specStream!.Length;
        var header = new SpecFileHeader
        {
            FirstRecordPtr = -1,
            FreeAreaPtr = position + Marshal.SizeOf<SpecFileHeader>()
        };

        _specStream.Position = position;
        byte[] buffer = StructureToByteArray(header);
        _specStream.Write(buffer, 0, buffer.Length);

        return position;
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

    private byte[] StructureToByteArray<T>(T structure) where T : struct
    {
        int size = Marshal.SizeOf<T>();
        byte[] bytes = new byte[size];
        GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
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
}