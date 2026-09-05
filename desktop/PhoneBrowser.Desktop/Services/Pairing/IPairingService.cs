namespace PhoneBrowser.Desktop.Services.Pairing;


using PhoneBrowser.Desktop.Models;

public interface IPairingService
{
    Task<string?> PairAsync(DiscoveredDevice device, CancellationToken ct);
}

