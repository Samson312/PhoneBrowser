using LiteDB;
using System.Net;

namespace PhoneBrowser.Desktop.Models;

public record class TrustedDevice
{
    public string Id { get; init; }
    public string Name { get; set; }

    public string Platform { get; init; }

    public string PairingToken { get; init; }

    public string LastKnownIpAddress { get; set; }

    public int LastKnownPort { get; set; }
    
    public DateTime LastConnectedAt { get; set; }
};


