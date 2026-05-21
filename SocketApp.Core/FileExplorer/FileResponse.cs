namespace SocketApp.Core.FileExplorer;

public enum FileResponseType
{
    Drives,
    Directory,
    TextFile,
    Error,
    Disconnected
}

public sealed class FileResponse
{
    public FileResponseType Type { get; set; }

    public string? Message { get; set; }

    public string? Path { get; set; }

    public List<string> Drives { get; set; } = [];

    public List<DirectoryTreeItem> Items { get; set; } = [];

    public string? TextContent { get; set; }

    public static FileResponse Error(string message)
    {
        return new FileResponse
        {
            Type = FileResponseType.Error,
            Message = message
        };
    }
}
