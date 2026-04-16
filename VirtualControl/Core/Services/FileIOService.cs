using VirtualControl.Core.Models;

namespace VirtualControl.Core.Services;

public class FileIOService : IDisposable
{
    private FileStream? _mainStream;
    private readonly string _mainPath;

    public FileIOService(string mainFilePath)
    {
        _mainPath = mainFilePath;
    }

    public void CreateFiles(FileHeader header)
    {
        _mainStream = new FileStream(_mainPath, FileMode.Create, FileAccess.ReadWrite, FileShare.None);
        
        var headerBytes = header.ToBytes();
        _mainStream.Write(headerBytes, 0, headerBytes.Length);
        
        var emptyPage = new byte[Constants.PageSize];
        for (long i = 0; i < header.TotalPages; i++)
        {
            _mainStream.Write(emptyPage, 0, emptyPage.Length);
        }
        _mainStream.Flush();
    }

    public void OpenFiles()
    {
        if (!File.Exists(_mainPath))
            throw new Exception($"Main file not found: {_mainPath}");

        _mainStream = new FileStream(_mainPath, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
        
        var signature = new byte[2];
        _mainStream.ReadExactly(signature, 0, 2);
        if (signature[0] != 'V' || signature[1] != 'M')
            throw new Exception($"Invalid signature {_mainPath}");
    }

    public FileHeader ReadHeader()
    {
        if (_mainStream == null) throw new Exception("File not opened");
        
        _mainStream.Seek(0, SeekOrigin.Begin);
        var buffer = new byte[Constants.HeaderSize];
        _mainStream.ReadExactly(buffer);
        return FileHeader.FromBytes(buffer);
    }

    public void WritePage(long pageNumber, PageHeader page)
    {
        if (_mainStream == null) throw new Exception("File not opened");
        
        long offset = Constants.HeaderSize + pageNumber * Constants.PageSize;
        _mainStream.Seek(offset, SeekOrigin.Begin);
        
        var bitmapBytes = page.Bitmap.ToBytes();
        _mainStream.Write(bitmapBytes, 0, bitmapBytes.Length);
        _mainStream.Write(page.Data, 0, page.Data.Length);
        _mainStream.Flush();
    }

    public void ReadPage(long pageNumber, PageHeader page)
    {
        if (_mainStream == null) throw new InvalidOperationException("File not opened");
        
        long offset = Constants.HeaderSize + pageNumber * Constants.PageSize;
        _mainStream.Seek(offset, SeekOrigin.Begin);
        
        var bitmapBytes = new byte[Constants.BitmapSizeBytes];
        _mainStream.ReadExactly(bitmapBytes);
        page.Bitmap = new Bitmap(bitmapBytes);
        _mainStream.ReadExactly(page.Data, 0, page.Data.Length);
    }

    public void Flush() => _mainStream?.Flush();
    

    public void Close()
    {
        Flush();
        _mainStream?.Close();
        _mainStream = null;
    }

    public void Dispose() => Close();
}