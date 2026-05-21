using System.Net;
using System.Net.Sockets;
using SocketApp.Core.Common;

namespace SocketApp.Core.Networking;

public sealed class TcpMessageServer : IDisposable
{
    private TcpListener? _listener;

    public bool IsRunning { get; private set; }

    public async Task StartAsync(
        SocketOptions options,
        Func<TcpClient, CancellationToken, Task> clientHandler,
        CancellationToken cancellationToken = default)
    {
        if (IsRunning)
            throw new HttpRequestException("TCP-сервер уже запущен.");

        IPAddress address = IPAddress.Parse(options.Host);
        _listener = new TcpListener(address, options.Port);
        _listener.Start();
        IsRunning = true;

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                TcpClient client = await _listener.AcceptTcpClientAsync(cancellationToken);

                _ = Task.Run(async () =>
                {
                    using (client)
                    {
                        await clientHandler(client, cancellationToken);
                    }
                }, cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
        }
        catch (ObjectDisposedException)
        {
        }
        finally
        {
            Stop();
        }
    }

    public void Stop()
    {
        IsRunning = false;
        _listener?.Stop();
        _listener = null;
    }

    public void Dispose()
    {
        Stop();
    }
}
