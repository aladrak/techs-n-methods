using System.Net.Sockets;
using System.Text;
using SocketApp.Core.Common;
using SocketApp.Core.Networking;

namespace SocketApp.Core.ProcessParameters;

public sealed class ProcessDispatcherClient : IDisposable
{
    private readonly IMessageSerializer _serializer;
    private readonly TcpMessageServer _server = new();

    public ProcessDispatcherClient(IMessageSerializer? serializer = null)
    {
        _serializer = serializer ?? new JsonMessageSerializer();
    }

    public Task StartAsync(
        SocketOptions options,
        Action<ProcessSample> onSampleReceived,
        Action<UiLogMessage>? log = null,
        CancellationToken cancellationToken = default)
    {
        log?.Invoke(new UiLogMessage($"Пульт ожидает контроллер: {options.Host}:{options.Port}."));

        return _server.StartAsync(
            options,
            (client, token) => HandleControllerAsync(client, onSampleReceived, log, token),
            cancellationToken);
    }

    public void Stop()
    {
        _server.Stop();
    }

    private async Task HandleControllerAsync(
        TcpClient client,
        Action<ProcessSample> onSampleReceived,
        Action<UiLogMessage>? log,
        CancellationToken cancellationToken)
    {
        string controllerName = client.Client.RemoteEndPoint?.ToString() ?? "контроллер";
        log?.Invoke(new UiLogMessage($"Подключён контроллер: {controllerName}."));

        NetworkStream stream = client.GetStream();
        using StreamReader reader = new(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, bufferSize: 1024, leaveOpen: true);

        while (!cancellationToken.IsCancellationRequested)
        {
            string? text = await reader.ReadLineAsync(cancellationToken);
            if (text is null)
                break;

            ProcessSample? sample = _serializer.Deserialize<ProcessSample>(text);
            if (sample is null)
            {
                log?.Invoke(new UiLogMessage("Получено некорректное измерение.", isError: true));
                continue;
            }

            onSampleReceived(sample);
        }

        log?.Invoke(new UiLogMessage($"Контроллер отключён: {controllerName}."));
    }

    public void Dispose()
    {
        _server.Dispose();
    }
}
