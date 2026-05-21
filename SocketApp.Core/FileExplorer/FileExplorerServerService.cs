using System.Net.Sockets;
using System.Text;
using SocketApp.Core.Common;
using SocketApp.Core.Networking;

namespace SocketApp.Core.FileExplorer;

public sealed class FileExplorerServerService : IDisposable
{
    private readonly IMessageSerializer _serializer;
    private readonly TcpMessageServer _server = new();

    public FileExplorerServerService(IMessageSerializer? serializer = null)
    {
        _serializer = serializer ?? new JsonMessageSerializer();
    }

    public Task StartAsync(
        SocketOptions options,
        Action<UiLogMessage>? log = null,
        CancellationToken cancellationToken = default)
    {
        log?.Invoke(new UiLogMessage($"Файловый сервер запущен: {options.Host}:{options.Port}."));

        return _server.StartAsync(
            options,
            (client, token) => HandleClientAsync(client, options, log, token),
            cancellationToken);
    }

    public void Stop()
    {
        _server.Stop();
    }

    private async Task HandleClientAsync(
        TcpClient client,
        SocketOptions options,
        Action<UiLogMessage>? log,
        CancellationToken cancellationToken)
    {
        string clientName = client.Client.RemoteEndPoint?.ToString() ?? "клиент";
        log?.Invoke(new UiLogMessage($"Подключён клиент: {clientName}."));

        NetworkStream stream = client.GetStream();
        using StreamReader reader = new(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, bufferSize: 1024, leaveOpen: true);
        await using StreamWriter writer = new(stream, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false), bufferSize: 1024, leaveOpen: true)
        {
            AutoFlush = true
        };

        await SendAsync(writer, CreateDrivesResponse(), cancellationToken);

        while (!cancellationToken.IsCancellationRequested)
        {
            string? requestText = await reader.ReadLineAsync(cancellationToken);
            if (requestText is null)
                break;

            FileRequest? request = _serializer.Deserialize<FileRequest>(requestText);
            if (request is null)
            {
                await SendAsync(writer, FileResponse.Error("Некорректный запрос клиента."), cancellationToken);
                continue;
            }

            if (request.Type == FileRequestType.Disconnect)
            {
                await SendAsync(writer, new FileResponse { Type = FileResponseType.Disconnected }, cancellationToken);
                break;
            }

            FileResponse response = request.Type switch
            {
                FileRequestType.DrivesRequest => CreateDrivesResponse(),
                FileRequestType.PathRequest => CreatePathResponse(request.Path, options),
                _ => FileResponse.Error("Неизвестный тип запроса.")
            };

            await SendAsync(writer, response, cancellationToken);
        }

        log?.Invoke(new UiLogMessage($"Клиент отключён: {clientName}."));
    }

    private async Task SendAsync<T>(StreamWriter writer, T message, CancellationToken cancellationToken)
    {
        string text = _serializer.Serialize(message);
        await writer.WriteLineAsync(text.AsMemory(), cancellationToken);
        await writer.FlushAsync(cancellationToken);
    }

    private static FileResponse CreateDrivesResponse()
    {
        return new FileResponse
        {
            Type = FileResponseType.Drives,
            Drives = DriveInfo.GetDrives()
                .Where(drive => drive.IsReady)
                .Select(drive => drive.RootDirectory.FullName)
                .ToList()
        };
    }

    private static FileResponse CreatePathResponse(string? path, SocketOptions options)
    {
        if (string.IsNullOrWhiteSpace(path))
            return FileResponse.Error("Путь к каталогу или файлу не указан.");

        try
        {
            if (Directory.Exists(path))
            {
                DirectoryInfo directory = new(path);

                return new FileResponse
                {
                    Type = FileResponseType.Directory,
                    Path = directory.FullName,
                    Items = CreateDirectoryItems(directory, options.MaxDirectoryDepth)
                };
            }

            if (File.Exists(path))
                return CreateTextFileResponse(path, options.MaxTextFileBytes);

            return FileResponse.Error("Указанный путь не найден.");
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or System.Security.SecurityException)
        {
            return FileResponse.Error($"Ошибка доступа к пути: {exception.Message}");
        }
    }

    private static FileResponse CreateTextFileResponse(string path, int maxTextFileBytes)
    {
        FileInfo file = new(path);
        if (file.Length > maxTextFileBytes)
            return FileResponse.Error($"Файл слишком большой для передачи. Ограничение: {maxTextFileBytes} байт.");

        string content = File.ReadAllText(path, Encoding.UTF8);

        return new FileResponse
        {
            Type = FileResponseType.TextFile,
            Path = file.FullName,
            TextContent = content
        };
    }

    private static List<DirectoryTreeItem> CreateDirectoryItems(DirectoryInfo directory, int depth)
    {
        List<DirectoryTreeItem> items = [];

        foreach (DirectoryInfo subdirectory in SafeEnumerateDirectories(directory))
        {
            DirectoryTreeItem item = new()
            {
                Name = subdirectory.Name,
                FullPath = subdirectory.FullName,
                Type = DirectoryTreeItemType.Directory
            };

            if (depth > 0)
                item.Children = CreateDirectoryItems(subdirectory, depth - 1);

            items.Add(item);
        }

        foreach (FileInfo file in SafeEnumerateFiles(directory))
        {
            items.Add(new DirectoryTreeItem
            {
                Name = file.Name,
                FullPath = file.FullName,
                Type = DirectoryTreeItemType.File
            });
        }

        return items;
    }

    private static IEnumerable<DirectoryInfo> SafeEnumerateDirectories(DirectoryInfo directory)
    {
        try
        {
            return directory.EnumerateDirectories();
        }
        catch
        {
            return [];
        }
    }

    private static IEnumerable<FileInfo> SafeEnumerateFiles(DirectoryInfo directory)
    {
        try
        {
            return directory.EnumerateFiles();
        }
        catch
        {
            return [];
        }
    }

    public void Dispose()
    {
        _server.Dispose();
    }
}
