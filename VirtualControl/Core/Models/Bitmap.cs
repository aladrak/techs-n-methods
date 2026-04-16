namespace VirtualControl.Core.Models;

public class Bitmap
{
    private readonly byte[] _bits;  // 16 байт

    public Bitmap() => _bits = new byte[Constants.BitmapSizeBytes];

    public Bitmap(byte[] source)
    {
        if (source.Length != Constants.BitmapSizeBytes)
            throw new ArgumentException($"Bitmap must be {Constants.BitmapSizeBytes} bytes");
        _bits = (byte[])source.Clone();
    }

    public bool IsSet(int index)
    {
        if (index < 0 || index >= Constants.ElementsPerPageFixed)
            throw new ArgumentOutOfRangeException(nameof(index));
        int byteIndex = index / 8;
        int bitIndex = index % 8;
        return (_bits[byteIndex] & (1 << bitIndex)) != 0;
    }

    public void Set(int index, bool value)
    {
        if (index < 0 || index >= Constants.ElementsPerPageFixed)
            throw new ArgumentOutOfRangeException(nameof(index));
        int byteIndex = index / 8;
        int bitIndex = index % 8;
        if (value)
            _bits[byteIndex] |= (byte)(1 << bitIndex);
        else
            _bits[byteIndex] &= (byte)~(1 << bitIndex);
    }

    public byte[] ToBytes() => (byte[])_bits.Clone();

    public void Clear() => Array.Clear(_bits, 0, _bits.Length);
}

public static class BitmapExtensions
{
    extension(Bitmap bitmap)
    {
        public void MarkWritten(int elementOffset, bool written)
            => bitmap.Set(elementOffset, written);

        public bool IsWritten(int elementOffset)
            => bitmap.IsSet(elementOffset);
    }
}