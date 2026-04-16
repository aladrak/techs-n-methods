namespace VirtualControl.Core;

public static class Constants
{
    public const string FileSignature = "VM";
    public const int PageSize = 512;
    public const int BitmapSizeBytes = 16; // 128 бит
    public const int ElementsPerPageFixed = 128;
    public const int MinBufferSize = 3;
    public const int AddressSize = sizeof(int);
    public const int HeaderSize = 512;
    public const int PageTotalSize = PageSize + BitmapSizeBytes;
}