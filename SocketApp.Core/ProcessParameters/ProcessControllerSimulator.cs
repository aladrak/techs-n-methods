using SocketApp.Core.Common;
using SocketApp.Core.Networking;

namespace SocketApp.Core.ProcessParameters;

public sealed class ProcessControllerSimulator
{
    private readonly Random _random = new();
    private readonly IMessageSerializer _serializer;

    public ProcessControllerSimulator(IMessageSerializer? serializer = null)
    {
        _serializer = serializer ?? new JsonMessageSerializer();
    }

    public ProcessSample GenerateNext()
    {
        return new ProcessSample
        {
            Time = DateTime.Now,
            Temperature = NextDouble(ProcessLimits.MinTemperature, ProcessLimits.MaxTemperature),
            Pressure = NextDouble(ProcessLimits.MinPressure, ProcessLimits.MaxPressure)
        };
    }

    public async Task RunAsync(
        string host = AppConstants.Localhost,
        int port = AppConstants.ProcessPort,
        int intervalMs = AppConstants.ProcessIntervalMs,
        Action<UiLogMessage>? log = null,
        CancellationToken cancellationToken = default)
    {
        await using TcpMessageClient client = new(_serializer);
        await client.ConnectAsync(host, port, cancellationToken);
        log?.Invoke(new UiLogMessage($"Контроллер подключён к пульту: {host}:{port}."));

        while (!cancellationToken.IsCancellationRequested)
        {
            ProcessSample sample = GenerateNext();
            await client.SendAsync(sample, cancellationToken);
            log?.Invoke(new UiLogMessage($"T={sample.Temperature:F1}; P={sample.Pressure:F2}."));
            await Task.Delay(intervalMs, cancellationToken);
        }
    }

    private double NextDouble(double min, double max)
    {
        return min + (_random.NextDouble() * (max - min));
    }
}
