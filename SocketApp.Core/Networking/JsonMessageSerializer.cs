using System.Text.Json;
using System.Text.Json.Serialization;

namespace SocketApp.Core.Networking;

public sealed class JsonMessageSerializer : IMessageSerializer
{
    private readonly JsonSerializerOptions _options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        Converters =
        {
            new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
        }
    };

    public string Serialize<T>(T message)
    {
        return JsonSerializer.Serialize(message, _options);
    }

    public T? Deserialize<T>(string text)
    {
        return JsonSerializer.Deserialize<T>(text, _options);
    }
}
