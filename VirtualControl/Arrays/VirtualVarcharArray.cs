using System.Text;
using VirtualControl.Core;

namespace VirtualControl.Arrays;

public class VirtualVarcharArray : IDisposable
{
    private const int headerSize = 15;
    private readonly FileStream _swapFile;
    private readonly FileStream _dataFile;
    private readonly long _capacity;
    private readonly int _maxStringLength;

    public VirtualVarcharArray(long capacity, int maxStringLength, string path, bool isNew)
    {
        if (capacity <= 0) throw new Exception(nameof(capacity));
        if (maxStringLength <= 0) throw new Exception(nameof(maxStringLength));

        _capacity = capacity;
        _maxStringLength = maxStringLength;

        if (isNew)
        {
            _swapFile = new FileStream(path, FileMode.Create, FileAccess.ReadWrite, FileShare.None);
            _dataFile = new FileStream(path + ".data", FileMode.Create, FileAccess.ReadWrite, FileShare.None);
            InitializeFiles();
        }
        else
        {
            _swapFile = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            _dataFile = new FileStream(path + ".data", FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            ValidateExistingFiles();
        }
    }

    private void InitializeFiles()
    {
        _swapFile.Position = 0;
        _swapFile.WriteByte((byte)'V');
        _swapFile.WriteByte((byte)'M');
        WriteInt64(_swapFile, _capacity);
        _swapFile.WriteByte((byte)'V');
        WriteInt32(_swapFile, _maxStringLength);

        long numPages = (_capacity + Constants.ElementsPerPageFixed - 1) / Constants.ElementsPerPageFixed;
        byte[] emptyBitmap = new byte[Constants.BitmapSizeBytes];
        byte[] emptyPage = new byte[Constants.PageSize];

        for (long p = 0; p < numPages; p++)
        {
            _swapFile.Write(emptyBitmap, 0, Constants.BitmapSizeBytes);
            _swapFile.Write(emptyPage, 0, Constants.PageSize);
        }
        _swapFile.Flush();
    }

    private void ValidateExistingFiles()
    {
        if (_swapFile.Length < headerSize)
            throw new Exception("The page file is corrupted or incorrect.");

        _swapFile.Position = 0;
        byte b1 = (byte)_swapFile.ReadByte();
        byte b2 = (byte)_swapFile.ReadByte();
        if (b1 != 'V' || b2 != 'M')
            throw new Exception("Invalid paging file signature.");
    }

    public void Set(long index, string value)
    {
        if (index < 0 || index >= _capacity) throw new Exception($"Index {index} is out of range [0; {_capacity - 1}].");
        if (value == null) value = string.Empty;
        if (value.Length > _maxStringLength)
            throw new Exception($"The string length ({value.Length}) exceeds the limit ({_maxStringLength}).");

        byte[] strBytes = Encoding.UTF8.GetBytes(value);
        long pageIdx = index / Constants.ElementsPerPageFixed;
        int localIdx = (int)(index % Constants.ElementsPerPageFixed);
        long pageOffset = headerSize + pageIdx * Constants.PageTotalSize;

        byte[] bitmap = new byte[Constants.BitmapSizeBytes];
        int[] addresses = new int[Constants.ElementsPerPageFixed];

        _swapFile.Position = pageOffset;
        ReadExactly(_swapFile, bitmap, 0, Constants.BitmapSizeBytes);
        for (int i = 0; i < Constants.ElementsPerPageFixed; i++)
            addresses[i] = ReadInt32(_swapFile);

        int byteIdx = localIdx / 8;
        int bitIdx = localIdx % 8;
        bitmap[byteIdx] |= (byte)(1 << bitIdx);

        _dataFile.Position = _dataFile.Length;
        long dataOffset = _dataFile.Position;
        WriteInt32(_dataFile, strBytes.Length);
        _dataFile.Write(strBytes, 0, strBytes.Length);

        addresses[localIdx] = (int)dataOffset;

        _swapFile.Position = pageOffset;
        _swapFile.Write(bitmap, 0, Constants.BitmapSizeBytes);
        for (int i = 0; i < Constants.ElementsPerPageFixed; i++)
            WriteInt32(_swapFile, addresses[i]);

        _swapFile.Flush();
    }

    public string? Get(long index)
    {
        if (index < 0 || index >= _capacity) throw new Exception($"Index {index} is out of range [0; {_capacity - 1}].");

        long pageIdx = index / Constants.ElementsPerPageFixed;
        int localIdx = (int)(index % Constants.ElementsPerPageFixed);
        long pageOffset = headerSize + pageIdx * Constants.PageTotalSize;

        byte[] bitmap = new byte[Constants.BitmapSizeBytes];
        int[] addresses = new int[Constants.ElementsPerPageFixed];

        _swapFile.Position = pageOffset;
        ReadExactly(_swapFile, bitmap, 0, Constants.BitmapSizeBytes);
        for (int i = 0; i < Constants.ElementsPerPageFixed; i++)
            addresses[i] = ReadInt32(_swapFile);

        int byteIdx = localIdx / 8;
        int bitIdx = localIdx % 8;
        bool isWritten = (bitmap[byteIdx] & (1 << bitIdx)) != 0 && addresses[localIdx] != 0;

        if (!isWritten) return null;

        int addr = addresses[localIdx];
        _dataFile.Position = addr;
        int len = ReadInt32(_dataFile);
        byte[] data = new byte[len];
        ReadExactly(_dataFile, data, 0, len);

        return Encoding.UTF8.GetString(data);
    }

    private static void WriteInt32(Stream s, int val) => s.Write(BitConverter.GetBytes(val), 0, 4);
    private static int ReadInt32(Stream s)
    {
        byte[] buf = new byte[4];
        ReadExactly(s, buf, 0, 4);
        return BitConverter.ToInt32(buf, 0);
    }

    private static void WriteInt64(Stream s, long val) => s.Write(BitConverter.GetBytes(val), 0, 8);

    private static void ReadExactly(Stream stream, byte[] buffer, int offset, int count)
    {
        int totalRead = 0;
        while (totalRead < count)
        {
            int read = stream.Read(buffer, offset + totalRead, count - totalRead);
            if (read == 0) throw new Exception("Unexpected end of file.");
            totalRead += read;
        }
    }

    public void Dispose()
    {
        _swapFile?.Dispose();
        _dataFile?.Dispose();
    }
    
}