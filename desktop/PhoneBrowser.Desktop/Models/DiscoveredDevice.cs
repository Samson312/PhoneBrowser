namespace PhoneBrowser.Desktop.Models;

using System;
using System.Net;

public record DiscoveredDevice
(
    string DeviceId,
    string DeviceName,
    string Platform,
    IPAddress IpAddress,
    int HttpPort
);
