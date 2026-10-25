namespace PhoneBrowser.Desktop.Services.Pairing;

using PhoneBrowser.Desktop.Errors;
using PhoneBrowser.Desktop.Models;

public interface IPairingService
{
    Task<Result<string, PairingError>> PairAsync(DiscoveredDevice device, CancellationToken ct);
}

