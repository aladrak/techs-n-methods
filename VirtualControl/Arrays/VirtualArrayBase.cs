using VirtualControl.Core;
using VirtualControl.Core.Models;
using VirtualControl.Core.Services;

namespace VirtualControl.Arrays;

public abstract class VirtualArrayBase : IDisposable
{
    protected readonly string _filePath;
    protected FileHeader _header;
    protected readonly FileIOService _fileIO;
    protected readonly AddressCalculator _addressCalc;
    protected readonly PageBufferManager _pageBuffer;

    protected VirtualArrayBase(string filePath, long elementCount, ArrayType type, int stringLength = 0, bool createNew = false)
    {
        _filePath = filePath;
        _fileIO = new FileIOService(_filePath);

        if (createNew)
        {
            _header = new FileHeader
            {
                Descriptor = new ArrayDescriptor
                {
                    ElementCount = elementCount,
                    ElementType = type,
                    StringLength = stringLength
                }
            };
            _header.TotalPages = _header.Descriptor.GetTotalPages();
            _fileIO.CreateFiles(_header);
        }
        else
        {
            _fileIO.OpenFiles();
            _header = _fileIO.ReadHeader();
        }

        _addressCalc = new AddressCalculator(_header.Descriptor);
        _pageBuffer = new PageBufferManager(Constants.MinBufferSize, _fileIO, _addressCalc, _header.Descriptor);

        int pagesToLoad = Math.Min(Constants.MinBufferSize, (int)_header.TotalPages);
        for (int i = 0; i < pagesToLoad; i++)
        {
            long startElementIndex = (long)i * _header.Descriptor.ElementsPerPage;
            _pageBuffer.GetBufferIndexForElement(startElementIndex);
        }
    }

    public long Length => _header.Descriptor.ElementCount;
    public ArrayType Type => _header.Descriptor.ElementType;

    public bool TryRead(long index, out object? value)
    {
        value = null;
        if (index < 0 || index >= _header.Descriptor.ElementCount) return false;

        var bufferIdx = _pageBuffer.GetBufferIndexForElement(index);
        if (bufferIdx == null) return false;

        var page = _pageBuffer.GetPage(bufferIdx.Value);
        var (_, elemOffset) = _addressCalc.GetPageAndOffset(index);
        var dataOffset = _addressCalc.GetElementOffsetInPageData(elemOffset);

        if (!page.Bitmap.IsSet(elemOffset)) return false;

        var bytes = ReadElementBytes(page.Data, (int)dataOffset);
        value = DeserializeValue(bytes);
        return true;
    }

    public bool Write(long index, object value)
    {
        if (index < 0 || index >= _header.Descriptor.ElementCount) return false;

        var bufferIdx = _pageBuffer.GetBufferIndexForElement(index);
        if (bufferIdx == null) return false;

        var page = _pageBuffer.GetPage(bufferIdx.Value);
        var (_, elemOffset) = _addressCalc.GetPageAndOffset(index);
        var dataOffset = _addressCalc.GetElementOffsetInPageData(elemOffset);

        var bytes = SerializeValue(value);
        WriteElementBytes(page.Data, (int)dataOffset, bytes);

        page.Bitmap.Set(elemOffset, true);
        page.MarkDirty();
        return true;
    }

    public object? this[long index]
    {
        get => TryRead(index, out var val) ? val : default;
        set
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            if (!Write(index, value))
                throw new Exception($"Failed to write at index {index}");
        }
    }

    protected abstract byte[] SerializeValue(object value);
    protected abstract object DeserializeValue(byte[] data);
    protected abstract int GetElementSize();

    protected virtual byte[] ReadElementBytes(byte[] pageData, int offset)
    {
        var size = GetElementSize();
        var res = new byte[size];
        Buffer.BlockCopy(pageData, offset, res, 0, size);
        return res;
    }

    protected virtual void WriteElementBytes(byte[] pageData, int offset, byte[] value)
        => Buffer.BlockCopy(value, 0, pageData, offset, value.Length);

    public void Flush() => _pageBuffer.FlushAll();

    public virtual void Dispose()
    {
        Flush();
        _fileIO.Dispose();
    }
}
