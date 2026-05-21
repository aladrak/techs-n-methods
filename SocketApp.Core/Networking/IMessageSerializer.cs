namespace SocketApp.Core.Networking;

public interface IMessageSerializer
{
    string Serialize<T>(T message);

    T? Deserialize<T>(string text);
}
