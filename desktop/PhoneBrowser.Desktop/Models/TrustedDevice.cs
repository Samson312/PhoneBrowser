using LiteDB;
using System.Net;

namespace PhoneBrowser.Desktop.Models;

public record class TrustedDevice
{
    [BsonId]
    public string DeviceId { get; init; }
    public string DeviceName { get; set; }

    public string AuthToken { get; init; }

    public string LastKnownIpAddress { get; set; }

    public int LastKnownPort { get; set; }
    
    public DateTime LastConnectedAt { get; set; }
};


