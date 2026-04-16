namespace VirtualControl.Core.Models;

public class FileHeader
{
    public ArrayDescriptor Descriptor { get; set; } = new();
    public long TotalPages { get; set; }

    public byte[] ToBytes()
    {
        var buffer = new byte[Constants.HeaderSize];
        int offset = 0;

        // Signature "VM"
        buffer[offset++] = (byte)'V';
        buffer[offset++] = (byte)'M';

        // ElementCount (long)
        Buffer.BlockCopy(BitConverter.GetBytes(Descriptor.ElementCount), 0, buffer, offset, 8);
        offset += 8;

        // ElementType (byte)
        buffer[offset++] = (byte)Descriptor.ElementType;

        // StringLength (int)
        Buffer.BlockCopy(BitConverter.GetBytes(Descriptor.StringLength), 0, buffer, offset, 4);
        offset += 4;

        // TotalPages (long)
        Buffer.BlockCopy(BitConverter.GetBytes(TotalPages), 0, buffer, offset, 8);

        // Остальное заполнено нулями по умолчанию

        return buffer;
    }

    public static FileHeader FromBytes(byte[] data)
    {
        if (data.Length < Constants.HeaderSize)
            throw new Exception("Header insufficient size");

        int offset = 0;

        // Check signature
        if (data[offset++] != 'V' || data[offset++] != 'M')
            throw new Exception("Header invalid signature");

        var header = new FileHeader();
        
        header.Descriptor.ElementCount = BitConverter.ToInt64(data, offset);
        offset += 8;

        header.Descriptor.ElementType = (ArrayType)data[offset++];

        header.Descriptor.StringLength = BitConverter.ToInt32(data, offset);
        offset += 4;

        header.TotalPages = BitConverter.ToInt64(data, offset);

        return header;
    }
}