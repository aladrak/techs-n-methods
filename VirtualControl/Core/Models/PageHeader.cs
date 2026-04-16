namespace VirtualControl.Core.Models;

public class PageHeader
{
    public long AbsolutePageNumber { get; set; } = -1;
    public byte Status { get; set; } 
    public DateTime LastAccessTime { get; set; }
    public DateTime LastWriteTime { get; set; }
    public Bitmap Bitmap { get; set; }
    public byte[] Data { get; set; }

    public PageHeader()
    {
        Bitmap = new Bitmap();
        Data = new byte[Constants.PageSize - Constants.BitmapSizeBytes];
    }

    public bool IsDirty => Status == 1;
    public bool IsLoaded => AbsolutePageNumber >= 0;

    public void MarkDirty()
    {
        Status = 1;
        LastWriteTime = DateTime.UtcNow;
    }

    public void MarkClean()
    {
        Status = 0;
    }

    public void Touch() => LastAccessTime = DateTime.UtcNow;
}