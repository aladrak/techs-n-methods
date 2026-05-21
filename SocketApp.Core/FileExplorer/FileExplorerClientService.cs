using SocketApp.Core.Common;
using SocketApp.Core.Networking;

namespace SocketApp.Core.FileExplorer;

public sealed class FileExplorerClientService : IAsyncDisposable
{
    private readonly TcpMessageClient _client;

    public FileExplorerClientService(IMessageSerializer? serializer = null)
    {
        _client = new TcpMessageClient(serializer);
    }

    public bool IsConnected => _client.IsConnected;

    public async Task<FileResponse> ConnectAsync(
        SocketOptions options,
        CancellationToken cancellationToken = default)
    {
        await _client.ConnectAsync(options.Host, options.Port, cancellationToken);

        return await _client.ReceiveAsync<FileResponse>(cancellationToken);
    }

    public async Task<FileResponse> RequestDrivesAsync(CancellationToken cancellationToken = default)
    {
        await _client.SendAsync(FileRequest.Drives(), cancellationToken);
        return await _client.ReceiveAsync<FileResponse>(cancellationToken);
    }

    public async Task<FileResponse> RequestPathAsync(string path, CancellationToken cancellationToken = default)
    {
        await _client.SendAsync(FileRequest.ForPath(path), cancellationToken);
        return await _client.ReceiveAsync<FileResponse>(cancellationToken);
    }

    public async Task DisconnectAsync(CancellationToken cancellationToken = default)
    {
        if (!IsConnected)
            return;

        await _client.SendAsync(FileRequest.Disconnect(), cancellationToken);
    }

    public ValueTask DisposeAsync()
    {
        return _client.DisposeAsync();
    }
}
