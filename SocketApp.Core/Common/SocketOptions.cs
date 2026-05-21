namespace SocketApp.Core.Common;

public sealed class SocketOptions
{
    public string Host { get; set; } = AppConstants.Localhost;

    public int Port { get; set; }

    public int MaxTextFileBytes { get; set; } = 1024 * 1024;

    public int MaxDirectoryDepth { get; set; } = 3;

    public static SocketOptions ForFileBrowser()
    {
        return new SocketOptions
        {
            Port = AppConstants.FileBrowserPort
        };
    }

    public static SocketOptions ForProcessDispatcher()
    {
        return new SocketOptions
        {
            Port = AppConstants.ProcessPort
        };
    }

    public static SocketOptions ForPlantDispatcher()
    {
        return new SocketOptions
        {
            Port = AppConstants.PlantPort
        };
    }
}
