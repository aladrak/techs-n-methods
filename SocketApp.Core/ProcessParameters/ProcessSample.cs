namespace SocketApp.Core.ProcessParameters;

public sealed class ProcessSample
{
    public DateTime Time { get; set; } = DateTime.Now;

    public double Temperature { get; set; }

    public double Pressure { get; set; }
}
