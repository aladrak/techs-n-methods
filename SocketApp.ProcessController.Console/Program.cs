using SocketApp.Core.Common;
using SocketApp.Core.ProcessParameters;

string host = args.Length > 0 ? args[0] : AppConstants.Localhost;
int port = ReadInt(args, index: 1, defaultValue: AppConstants.ProcessPort);
int intervalMs = ReadInt(args, index: 2, defaultValue: AppConstants.ProcessIntervalMs);

using CancellationTokenSource cancellationTokenSource = new();

Console.CancelKeyPress += (_, eventArgs) =>
{
    eventArgs.Cancel = true;
    cancellationTokenSource.Cancel();
};

Console.WriteLine("Контроллер технологического процесса");
Console.WriteLine($"Пульт диспетчера: {host}:{port}");
Console.WriteLine($"Интервал передачи: {intervalMs} мс");
Console.WriteLine("Нажмите Enter для завершения.");
Console.WriteLine();

ProcessControllerSimulator simulator = new();

Task workTask = simulator.RunAsync(
    host,
    port,
    intervalMs,
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

static int ReadInt(string[] args, int index, int defaultValue)
{
    if (args.Length <= index)
        return defaultValue;

    return int.TryParse(args[index], out int value) ? value : defaultValue;
}
