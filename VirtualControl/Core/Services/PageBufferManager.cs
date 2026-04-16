using VirtualControl.Core.Models;

namespace VirtualControl.Core.Services;

public class PageBufferManager
{
    private readonly PageHeader[] _buffer;
    private readonly int _bufferSize;
    private readonly FileIOService _fileIO;
    private readonly AddressCalculator _addressCalc;

    public PageBufferManager(int bufferSize, FileIOService fileIO, 
                             AddressCalculator addressCalc, ArrayDescriptor descriptor)
    {
        if (bufferSize < Constants.MinBufferSize)
            throw new Exception($"Buffer size must be at least {Constants.MinBufferSize}");
        
        _bufferSize = bufferSize;
        _buffer = new PageHeader[bufferSize];
        for (int i = 0; i < bufferSize; i++)
            _buffer[i] = new PageHeader();
        
        _fileIO = fileIO;
        _addressCalc = addressCalc;
    }

    public int? GetBufferIndexForElement(long elementIndex)
    {
        var (absolutePage, _) = _addressCalc.GetPageAndOffset(elementIndex);
        int bufferIndex = FindPageInBuffer(absolutePage);
        if (bufferIndex != -1)
        {
            _buffer[bufferIndex].Touch();
            return bufferIndex;
        }
        
        int victimIndex = SelectVictimPage();
        if (_buffer[victimIndex].IsLoaded && _buffer[victimIndex].IsDirty)
        {
            _fileIO.WritePage(_buffer[victimIndex].AbsolutePageNumber, _buffer[victimIndex]);
        }
        
        _buffer[victimIndex].AbsolutePageNumber = absolutePage;
        _fileIO.ReadPage(absolutePage, _buffer[victimIndex]);
        _buffer[victimIndex].Touch();
        _buffer[victimIndex].MarkClean();
        
        return victimIndex;
    }

    private int FindPageInBuffer(long absolutePageNumber)
    {
        for (int i = 0; i < _bufferSize; i++)
        {
            if (_buffer[i].AbsolutePageNumber == absolutePageNumber)
                return i;
        }
        return -1;
    }

    private int SelectVictimPage()
    {
        for (int i = 0; i < _bufferSize; i++)
            if (_buffer[i].AbsolutePageNumber == -1) return i;

        int victim = 0;
        DateTime oldest = _buffer[0].LastAccessTime;
        for (int i = 1; i < _bufferSize; i++)
        {
            if (_buffer[i].LastAccessTime < oldest)
            {
                oldest = _buffer[i].LastAccessTime;
                victim = i;
            }
        }
        return victim;
    }

    public PageHeader GetPage(int bufferIndex) => _buffer[bufferIndex];

    public void FlushAll()
    {
        for (int i = 0; i < _bufferSize; i++)
        {
            if (_buffer[i].IsDirty && _buffer[i].IsLoaded)
            {
                _fileIO.WritePage(_buffer[i].AbsolutePageNumber, _buffer[i]);
                _buffer[i].MarkClean();
            }
        }
        _fileIO.Flush();
    }
}