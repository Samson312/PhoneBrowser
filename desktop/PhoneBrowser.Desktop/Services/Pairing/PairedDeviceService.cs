namespace PhoneBrowser.Desktop.Services.Pairing;

using Microsoft.Extensions.Logging;
using PhoneBrowser.Desktop.Models;

public class PairedDeviceService
{
    private readonly ILogger<PairedDeviceService> logger;

    public DiscoveredDevice? CurrentDevice { get; private set; }

    public string? AuthToken { get; private set; }

    public bool IsPaired => CurrentDevice != null && AuthToken != null;

    public string? BaseUrl => IsPaired ? $"http://{CurrentDevice.IpAddress}:{CurrentDevice.HttpPort}" : null;

    public PairedDeviceService(ILogger<PairedDeviceService> logger)
    {
        this.logger = logger;
    }

    public void SetDevice(DiscoveredDevice device, string token)
    {
        CurrentDevice = device;
        AuthToken = token;
        logger.LogInformation("Paired device set: {DeviceName} ({DeviceId}) at {Address}:{Port}",
            device.DeviceName, device.DeviceId, device.IpAddress, device.HttpPort);
    }

    public void Clear()
    {
        if (CurrentDevice != null)
            logger.LogInformation("Clearing paired device {DeviceName}", CurrentDevice.DeviceName);

        CurrentDevice = null;
        AuthToken = null;
    }
}

