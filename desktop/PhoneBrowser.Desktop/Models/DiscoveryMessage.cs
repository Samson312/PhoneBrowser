using System.Text.Json;

namespace PhoneBrowser.Desktop.Models;

public record DiscoveryMessage(string Type, string? DeviceName = null, int? TcpPort = null)
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

