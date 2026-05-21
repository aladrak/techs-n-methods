using System.Net;
using System.Net.Sockets;
using System.Text;

namespace SocketApp.Core.Networking;

public sealed class TcpMessageClient : IAsyncDisposable
{
    private readonly IMessageSerializer _serializer;
    private TcpClient? _client;
    private StreamReader? _reader;
    private StreamWriter? _writer;

    public TcpMessageClient(IMessageSerializer? serializer = null)
    {
        _serializer = serializer ?? new JsonMessageSerializer();
    }

    public bool IsConnected => _client?.Connected == true;

    public async Task ConnectAsync(string host, int port, CancellationToken cancellationToken = default)
    {
        await DisposeAsync();

        _client = new TcpClient();
        await _client.ConnectAsync(host, port, cancellationToken);

        NetworkStream stream = _client.GetStream();
        _reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, bufferSize: 1024, leaveOpen: true);
        _writer = new StreamWriter(stream, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false), bufferSize: 1024, leaveOpen: true)
        {
            AutoFlush = true
        };
    }

    public async Task SendAsync<T>(T message, CancellationToken cancellationToken = default)
    {
        EnsureConnected();

        string text = _serializer.Serialize(message);
        await _writer!.WriteLineAsync(text.AsMemory(), cancellationToken);
        await _writer.FlushAsync(cancellationToken);
    }

    public async Task<T> ReceiveAsync<T>(CancellationToken cancellationToken = default)
    {
        EnsureConnected();

        string? text = await _reader!.ReadLineAsync(cancellationToken);
        if (text is null)
            throw new HttpRequestException("Соединение было закрыто удалённой стороной.");

        T? message = _serializer.Deserialize<T>(text);
        if (message is null)
            throw new HttpRequestException("Не удалось прочитать сообщение из TCP-потока.");

        return message;
    }

    private void EnsureConnected()
    {
        if (_client is null || _reader is null || _writer is null)
            throw new HttpRequestException("TCP-клиент не подключён.");
    }

    public async ValueTask DisposeAsync()
    {
        if (_writer is not null)
            await _writer.DisposeAsync();

        _reader?.Dispose();
        _client?.Dispose();

        _writer = null;
        _reader = null;
        _client = null;
    }
}
