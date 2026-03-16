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

	public void Dispose() => CloseFiles();

	public void CreateDatabase(string productName, short nameLength = Sizes.DefaultNameLength)
	{
		CloseFiles();
		_products.Clear();
		_specs.Clear();
		_nextTempId = -1;

		_productFilePath = productName + ".prd";
		_specFilePath = productName + ".prs";

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

		_productFilePath = productName + ".prd";
		_specFilePath = productName + ".prs";

		if (!File.Exists(_productFilePath)) throw new Exception($"Файл {_productFilePath} не найден");
		if (!File.Exists(_specFilePath)) throw new Exception($"Файл спецификаций {_specFilePath} не найден");

		_productStream = new FileStream(_productFilePath, FileMode.Open, FileAccess.ReadWrite);
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

			_products.Add(new ProductInfo
			{
				FileOffset = ptr,
				Name = name,
				SpecFilePtr = record.SpecFilePtr,
				IsDeleted = record.IsDeleted == -1,
				Type = (ComponentType)record.Type
			});
			ptr = record.NextRecordPtr;
		}

		_specStream!.Position = Marshal.SizeOf<SpecFileHeader>();
		while (_specStream.Position < _specStream.Length)
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

		var activeProducts = _products.Where(p => !p.IsDeleted).OrderBy(p => p.Name).ToList();
		var activeSpecs = _specs.Where(s => !s.IsDeleted).ToList();

		_productStream?.Close();
		_specStream?.Close();

		_productStream = new FileStream(_productFilePath, FileMode.Create, FileAccess.ReadWrite);
		_specStream = new FileStream(_specFilePath, FileMode.Create, FileAccess.ReadWrite);

		_productHeader = ProductFileHeader.Create(Path.GetFileName(_specFilePath), _productHeader.DataLength);
		_specHeader = SpecFileHeader.Create();
		WriteProductHeader();
		WriteSpecHeader();

		var productNameToNewOffset = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
		var productOldToNew = new Dictionary<int, int>();

		foreach (var prod in activeProducts)
		{
			int recordOffset = (int)_productStream!.Position;
			productNameToNewOffset[prod.Name] = recordOffset;
			productOldToNew[prod.FileOffset] = recordOffset;

			var record = new ProductRecord
			{
				IsDeleted = 0,
				SpecFilePtr = -1,
				NextRecordPtr = -1,
				Type = (sbyte)prod.Type
			};
			byte[] recBytes = StructureToByteArray(record);
			_productStream.Write(recBytes, 0, recBytes.Length);

			byte[] nameBytes = Encoding.ASCII.GetBytes(prod.Name.PadRight(_productHeader.DataLength)[.._productHeader.DataLength]);
			_productStream.Write(nameBytes, 0, nameBytes.Length);
		}

		var productOffsets = activeProducts.Select(p => productNameToNewOffset[p.Name]).ToList();
		for (int i = 0; i < productOffsets.Count; i++)
		{
			int nextPtr = (i < productOffsets.Count - 1) ? productOffsets[i + 1] : -1;
			_productStream!.Position = productOffsets[i] + 1 + 4;
			byte[] nextBytes = BitConverter.GetBytes(nextPtr);
			_productStream.Write(nextBytes, 0, 4);
		}

		_productHeader.FirstRecordPtr = productOffsets.Count > 0 ? productOffsets[0] : -1;
		_productHeader.FreeAreaPtr = (int)_productStream!.Position;

		var specsByOwner = activeSpecs
			.Where(s => productOldToNew.ContainsKey(s.OwnerOffset) && productOldToNew.ContainsKey(s.ProductFilePtr))
			.GroupBy(s => s.OwnerOffset)
			.ToDictionary(g => g.Key, g => g.ToList());

		var ownerFirstSpecOffset = new Dictionary<int, int>();

		foreach (var prod in activeProducts.Where(p => p.Type != ComponentType.Detail))
		{
			if (!specsByOwner.TryGetValue(prod.FileOffset, out var specList))
				continue;

			var recordOffsets = new List<int>();

			foreach (var spec in specList)
			{
				int recOffset = (int)_specStream!.Position;
				recordOffsets.Add(recOffset);

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

			for (int i = 0; i < recordOffsets.Count; i++)
			{
				int nextPtr = (i < recordOffsets.Count - 1) ? recordOffsets[i + 1] : -1;
				_specStream!.Position = recordOffsets[i] + 1 + 4 + 2;
				byte[] nextBytes = BitConverter.GetBytes(nextPtr);
				_specStream.Write(nextBytes, 0, 4);
			}

			if (recordOffsets.Count > 0)
			{
				ownerFirstSpecOffset[prod.FileOffset] = recordOffsets[0];
			}
		}

		foreach (var prod in activeProducts.Where(p => p.Type != ComponentType.Detail))
		{
			if (ownerFirstSpecOffset.TryGetValue(prod.FileOffset, out int firstSpecOffset))
			{
				int newProdOffset = productOldToNew[prod.FileOffset];
				_productStream!.Position = newProdOffset + 1;
				byte[] ptrBytes = BitConverter.GetBytes(firstSpecOffset);
				_productStream.Write(ptrBytes, 0, 4);
			}
		}

		_specHeader.FirstRecordPtr = _specStream!.Length > Marshal.SizeOf<SpecFileHeader>()
			? Marshal.SizeOf<SpecFileHeader>()
			: -1;
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

		int tempOffset = _nextTempId--;
		var product = new ProductInfo
		{
			FileOffset = tempOffset,
			Name = name,
			Type = type,
			SpecFilePtr = -1,
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

		var component = _products.FirstOrDefault(p => p.FileOffset == componentOffset && !p.IsDeleted)
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
		PrintSpecRecursive(prod, 0, new HashSet<int>());
	}

	private void PrintSpecRecursive(ProductInfo product, int indent, HashSet<int> visited)
	{
		if (indent > 100) { Console.WriteLine($"{new string(' ', indent * 2)}... (глубокая вложенность)"); return; }
		if (visited.Contains(product.FileOffset))
		{
			Console.WriteLine($"{new string('|', indent)} {product.Name} (циклическая ссылка)");
			return;
		}
		visited.Add(product.FileOffset);

		string prefix = indent > 0 ? new string('|', indent - 1) + " " : "";
		Console.WriteLine($"{prefix}{product.Name} ({product.Type})");

		var specs = _specs.Where(s => !s.IsDeleted && s.OwnerOffset == product.FileOffset).ToList();
		foreach (var spec in specs)
		{
			var comp = _products.FirstOrDefault(p => p.FileOffset == spec.ProductFilePtr && !p.IsDeleted);
			if (comp == null) continue;

			string mult = spec.Multiplicity > 1 ? $" x{spec.Multiplicity}" : "";
			string childPrefix = new string('|', indent) + " ";
			Console.WriteLine($"{childPrefix}{comp.Name} ({comp.Type}){mult}");

			if (comp.SpecFilePtr != -1)
				PrintSpecRecursive(comp, indent + 1, visited);
		}

		visited.Remove(product.FileOffset);
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

	private ProductInfo ReadProduct(int offset)
	{
		if (!IsOpen) throw new Exception("База данных не открыта");

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
			Type = (ComponentType)record.Type
		};
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