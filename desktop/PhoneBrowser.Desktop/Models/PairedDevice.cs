namespace PhoneBrowser.Desktop.Models;

using Microsoft.Extensions.Logging;

public class PairedDevice
{
    private readonly ILogger<PairedDevice> logger;

    public TrustedDevice? CurrentDevice { get; private set; }

    public string? AuthToken { get; private set; }

    public bool IsPaired => CurrentDevice != null && AuthToken != null;

    public string? BaseUrl => IsPaired ? $"http://{CurrentDevice.LastKnownIpAddress}:{CurrentDevice.LastKnownPort}" : null;

    public PairedDevice(ILogger<PairedDevice> logger)
    {
        this.logger = logger;
    }

    public void SetDevice(TrustedDevice device)
    {
        CurrentDevice = device;
        AuthToken = device.AuthToken;
        logger.LogInformation("Paired device set: {DeviceName} ({DeviceId}) at {Address}:{Port}",
            device.DeviceName, device.DeviceId, device.LastKnownIpAddress, device.LastKnownPort);
    }

    public void Clear()
    {
        if (CurrentDevice != null)
            logger.LogInformation("Clearing paired device {DeviceName}", CurrentDevice.DeviceName);

        CurrentDevice = null;
        AuthToken = null;
    }
}

