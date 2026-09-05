using System.Text.Json;

namespace PhoneBrowser.Desktop.Models;

public record DiscoveryMessage(string deviceId, string deviceName, string type = "DISCOVER", string platform = "PC", int? httpPort = null, int protocolVersion = 1)
{
    private static readonly JsonSerializerOptions jsonOptions = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true
    };

    public string ToJson()
    {
        return JsonSerializer.Serialize(this);
    }

    public static DiscoveryMessage FromJson(string raw)
    {
        return JsonSerializer.Deserialize<DiscoveryMessage>(raw, jsonOptions)
               ?? throw new JsonException("Invalid DiscoveryMessage JSON.");
    }
}

