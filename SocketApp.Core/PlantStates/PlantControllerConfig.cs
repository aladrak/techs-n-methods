using SocketApp.Core.Common;

namespace SocketApp.Core.PlantStates;

public sealed class PlantControllerConfig
{
    public string Host { get; set; } = AppConstants.Localhost;

    public int Port { get; set; } = AppConstants.PlantPort;

    public int PlantCount { get; set; } = 8;

    public int IntervalMs { get; set; } = AppConstants.PlantIntervalMs;
}
