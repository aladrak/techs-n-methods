using System.Net.Sockets;
using System.Text;
using SocketApp.Core.Common;
using SocketApp.Core.Networking;

namespace SocketApp.Core.PlantStates;

public sealed class PlantDispatcherClient : IDisposable
{
    private readonly IMessageSerializer _serializer;
    private readonly TcpMessageServer _server = new();

    public PlantDispatcherClient(IMessageSerializer? serializer = null)
    {
        _serializer = serializer ?? new JsonMessageSerializer();
    }

    public Task StartAsync(
        SocketOptions options,
        Action<int> onPlantCountReceived,
        Action<IReadOnlyList<PlantStateItem>> onStatesReceived,
        Action<UiLogMessage>? log = null,
        CancellationToken cancellationToken = default)
    {
        log?.Invoke(new UiLogMessage($"Пульт установок ожидает контроллер: {options.Host}:{options.Port}."));

        return _server.StartAsync(
            options,
            (client, token) => HandleControllerAsync(client, onPlantCountReceived, onStatesReceived, log, token),
            cancellationToken);
    }

    public void Stop()
    {
        _server.Stop();
    }

    private async Task HandleControllerAsync(
        TcpClient client,
        Action<int> onPlantCountReceived,
        Action<IReadOnlyList<PlantStateItem>> onStatesReceived,
        Action<UiLogMessage>? log,
        CancellationToken cancellationToken)
    {
        string controllerName = client.Client.RemoteEndPoint?.ToString() ?? "контроллер";
        log?.Invoke(new UiLogMessage($"Подключён контроллер установок: {controllerName}."));

        NetworkStream stream = client.GetStream();
        using StreamReader reader = new(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, bufferSize: 1024, leaveOpen: true);
        await using StreamWriter writer = new(stream, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false), bufferSize: 1024, leaveOpen: true)
        {
            AutoFlush = true
        };

        while (!cancellationToken.IsCancellationRequested)
        {
            string? text = await reader.ReadLineAsync(cancellationToken);
            if (text is null)
                break;

            PlantStateMessage? message = _serializer.Deserialize<PlantStateMessage>(text);
            if (message is null)
            {
                log?.Invoke(new UiLogMessage("Получено некорректное сообщение установок.", isError: true));
                continue;
            }

            if (message.Type == PlantMessageType.PlantCount)
            {
                onPlantCountReceived(message.PlantCount);
                await SendAsync(writer, PlantStateMessage.Ack(), cancellationToken);
                continue;
            }

            if (message.Type == PlantMessageType.States)
            {
                onStatesReceived(message.States);
                continue;
            }

            if (message.Type == PlantMessageType.Disconnect)
                break;
        }

        log?.Invoke(new UiLogMessage($"Контроллер установок отключён: {controllerName}."));
    }

    private async Task SendAsync<T>(StreamWriter writer, T message, CancellationToken cancellationToken)
    {
        string text = _serializer.Serialize(message);
        await writer.WriteLineAsync(text.AsMemory(), cancellationToken);
        await writer.FlushAsync(cancellationToken);
    }

    public void Dispose()
    {
        _server.Dispose();
    }
}
