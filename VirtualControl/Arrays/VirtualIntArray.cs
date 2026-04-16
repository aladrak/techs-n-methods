using VirtualControl.Core;

namespace VirtualControl.Arrays;

public class VirtualIntArray : VirtualArrayBase
{
public VirtualIntArray(string filePath, long elementCount, bool createNew = false)
    : base(filePath, elementCount, ArrayType.Int, 0, createNew) { }

    public new bool TryRead(long index, out int value)
    {
        value = 0;
        if (base.TryRead(index, out var obj) && obj is int i) { value = i; return true; }
        return false;
    }

    public new bool Write(long index, int value) => base.Write(index, value);

    public new int this[long index]
    {
        get => TryRead(index, out var v) ? v : 0;
        set => base.Write(index, value);
    }

    protected override byte[] SerializeValue(object value) => BitConverter.GetBytes(value is int i ? i : throw new InvalidCastException());
    protected override object DeserializeValue(byte[] data) => BitConverter.ToInt32(data, 0);
    protected override int GetElementSize() => sizeof(int);
}