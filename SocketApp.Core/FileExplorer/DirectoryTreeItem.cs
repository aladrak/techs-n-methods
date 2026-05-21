namespace SocketApp.Core.FileExplorer;

public enum DirectoryTreeItemType
{
    Directory,
    File
}

public sealed class DirectoryTreeItem
{
    public string Name { get; set; } = string.Empty;

    public string FullPath { get; set; } = string.Empty;

    public DirectoryTreeItemType Type { get; set; }

    public List<DirectoryTreeItem> Children { get; set; } = [];
}
