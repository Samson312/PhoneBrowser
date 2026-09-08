namespace PhoneBrowser.Desktop.Services.Pairing;

using PhoneBrowser.Desktop.Models;

public class PairedDeviceService
{
    public DiscoveredDevice? CurrentDevice { get; private set; }

    public string? AuthToken { get; private set; }

    public bool IsPaired => CurrentDevice != null && AuthToken != null;

    public string? BaseUrl => IsPaired ? $"http://{CurrentDevice.IpAddress}:{CurrentDevice.HttpPort}" : null;

    public void SetDevice(DiscoveredDevice device, string token)
    {
        CurrentDevice = device;
        AuthToken = token;
    }

    public void Clear()
    {
        CurrentDevice = null;
        AuthToken = null;
    }
}

