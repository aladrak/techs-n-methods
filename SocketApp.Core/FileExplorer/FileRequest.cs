namespace SocketApp.Core.FileExplorer;

public enum FileRequestType
{
    DrivesRequest,
    PathRequest,
    Disconnect
}

public sealed class FileRequest
{
    public FileRequestType Type { get; set; }

    public string? Path { get; set; }

    public static FileRequest Drives()
    {
        return new FileRequest
        {
            Type = FileRequestType.DrivesRequest
        };
    }

    public static FileRequest ForPath(string path)
    {
        return new FileRequest
        {
            Type = FileRequestType.PathRequest,
            Path = path
        };
    }

    public static FileRequest Disconnect()
    {
        return new FileRequest
        {
            Type = FileRequestType.Disconnect
        };
    }
}
