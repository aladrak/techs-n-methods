using System.Text.Json;
using SocketApp.Core.PlantStates;

string configPath = args.Length > 0 ? args[0] : "plant-config.json";
PlantControllerConfig config = await ReadConfigAsync(configPath);

using CancellationTokenSource cancellationTokenSource = new();

Console.CancelKeyPress += (_, eventArgs) =>
{
    eventArgs.Cancel = true;
    cancellationTokenSource.Cancel();
};

Console.WriteLine("Контроллер состояния технологических установок");
Console.WriteLine($"Файл конфигурации: {Path.GetFullPath(configPath)}");
Console.WriteLine($"Пульт диспетчера: {config.Host}:{config.Port}");
Console.WriteLine($"Количество установок: {config.PlantCount}");
Console.WriteLine($"Интервал передачи: {config.IntervalMs} мс");
Console.WriteLine("Нажмите Enter для завершения.");
Console.WriteLine();

PlantControllerSimulator simulator = new();

Task workTask = simulator.RunAsync(
    config,
    message => Console.WriteLine(message.ToString()),
    cancellationTokenSource.Token);

_ = Task.Run(() =>
{
    Console.ReadLine();
    cancellationTokenSource.Cancel();
});

try
{
    await workTask;
}
catch (OperationCanceledException)
{
}
catch (Exception exception)
{
    Console.WriteLine($"Ошибка: {exception.Message}");
}

Console.WriteLine("Контроллер остановлен.");

static async Task<PlantControllerConfig> ReadConfigAsync(string path)
{
    if (!File.Exists(path))
    {
        PlantControllerConfig defaultConfig = new();
        string json = JsonSerializer.Serialize(defaultConfig, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        await File.WriteAllTextAsync(path, json);
        return defaultConfig;
    }

    string configText = await File.ReadAllTextAsync(path);
    PlantControllerConfig? config = JsonSerializer.Deserialize<PlantControllerConfig>(
        configText,
        new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

    if (config is null)
        throw new InvalidOperationException("Не удалось прочитать файл конфигурации.");

    return config;
}
