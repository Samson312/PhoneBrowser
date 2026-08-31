namespace PhoneBrowser.Desktop.Models;

using System;
using System.Net;

public record DiscoveredDevice
(
    IPAddress IpAddress,
    string DeviceName
);
