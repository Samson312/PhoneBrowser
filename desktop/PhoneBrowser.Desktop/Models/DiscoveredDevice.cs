namespace PhoneBrowser.Desktop.Models;

using System;
using System.Net;

public record DiscoveredDevice
(
    string DeviceId,
    string DeviceName,
    IPAddress IpAddress,
    int HttpPort
);
