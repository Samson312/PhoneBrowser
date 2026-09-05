namespace PhoneBrowser.Desktop.Services.Discovery;

using PhoneBrowser.Desktop.Models;

public interface IUdpDiscoveryService : IDisposable
{
    event Action<DiscoveredDevice>? DeviceDiscovered;
    Task StartAsync(CancellationToken ct);
    void Stop();
}
