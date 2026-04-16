using System.Text;
using VirtualControl.Core;

namespace VirtualControl.Arrays;

public class VirtualCharArray : VirtualArrayBase
{
    private readonly int _fixedLength;
    private readonly Encoding _encoding = Encoding.UTF8;

    public VirtualCharArray(string filePath, long elementCount, int stringLength, bool createNew = false)
        : base(filePath, elementCount, ArrayType.Char, stringLength, createNew)
    {
        _fixedLength = stringLength;
    }

    public new bool TryRead(long index, out string value)
    {
        value = string.Empty;
        if (base.TryRead(index, out var obj) && obj is string s) { value = s; return true; }
        return false;
    }
    
    public bool Write(long index, string value) => base.Write(index, value);

    public new string this[long index]
    {
        get => TryRead(index, out var v) ? v : string.Empty;
        set => Write(index, value);
    }

    protected override byte[] SerializeValue(object value)
    {
        if (value is not string str) throw new InvalidCastException();

        var bytes = _encoding.GetBytes(str);
        if (bytes.Length > _fixedLength)
            Array.Resize(ref bytes, _fixedLength);
        else if (bytes.Length < _fixedLength)
        {
            var result = new byte[_fixedLength];
            Buffer.BlockCopy(bytes, 0, result, 0, bytes.Length);
            return result;
        }
        return bytes;
    }

    protected override object DeserializeValue(byte[] data)
    {
        int len = data.Length;
        while (len > 0 && data[len - 1] == 0) len--;
        return _encoding.GetString(data, 0, len);
    }

    protected override int GetElementSize() => _fixedLength;
}