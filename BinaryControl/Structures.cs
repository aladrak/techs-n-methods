using System.Runtime.InteropServices;
using System.Text;

namespace BinaryControl;

public static class Sizes
{
	public const int SignatureSize = 2;
	public const int LengthSize = 2;
	public const int PointerSize = 4;
	public const int DeleteBitSize = 1;
	public const int MultiplicitySize = 2;
	public const int FileNameSize = 16;
	public const int DefaultNameLength = 32;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct ProductFileHeader
{
	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
	public byte[] Signature;
	public short DataLength;
	public int FirstRecordPtr;
	public int FreeAreaPtr;
	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
	public byte[] SpecFileName;

	public static ProductFileHeader Create(string specFileName, short dataLength = Sizes.DefaultNameLength)
	{
		var header = new ProductFileHeader
		{
			Signature = new byte[] { (byte)'P', (byte)'S' },
			DataLength = dataLength,
			FirstRecordPtr = -1,
			FreeAreaPtr = Marshal.SizeOf<ProductFileHeader>(),
			SpecFileName = Encoding.ASCII.GetBytes(specFileName.PadRight(Sizes.FileNameSize).Substring(0, Sizes.FileNameSize))
		};
		return header;
	}
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct ProductRecord
{
	public sbyte IsDeleted;
	public int SpecFilePtr;
	public int NextRecordPtr;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct SpecFileHeader
{
	public int FirstRecordPtr;
	public int FreeAreaPtr;

	public static SpecFileHeader Create()
	{
		return new SpecFileHeader
		{
			FirstRecordPtr = -1,
			FreeAreaPtr = Marshal.SizeOf<SpecFileHeader>()
		};
	}
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct SpecRecord
{
	public sbyte IsDeleted;
	public int ProductFilePtr;
	public short Multiplicity;
	public int NextRecordPtr;
}

public enum ComponentType
{
	Product,
	Node,
	Detail
}

public class ProductInfo
{
	public int FileOffset { get; set; }
	public string Name { get; set; } = "";
	public ComponentType Type { get; set; }
	public int SpecFilePtr { get; set; }
	public bool IsDeleted { get; set; }
}

public class SpecInfo
{
	public int FileOffset { get; set; }
	public int ProductFilePtr { get; set; }
	public short Multiplicity { get; set; }
	public int NextRecordPtr { get; set; }
	public bool IsDeleted { get; set; }
}