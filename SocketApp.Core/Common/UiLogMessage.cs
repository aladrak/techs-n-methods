namespace SocketApp.Core.Common;

public sealed class UiLogMessage
{
    public UiLogMessage(string text, bool isError = false)
    {
        Text = text;
        IsError = isError;
        CreatedAt = DateTime.Now;
    }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public string Text { get; set; } = string.Empty;

    public bool IsError { get; set; }

    public override string ToString()
    {
        return $"[{CreatedAt:HH:mm:ss}] {Text}";
    }
}
