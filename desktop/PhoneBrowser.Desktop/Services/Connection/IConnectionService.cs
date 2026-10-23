namespace PhoneBrowser.Desktop.Services.Connection;

using PhoneBrowser.Desktop.Models;


public enum ConnectError { None, Unreachable, Unauthorized, Busy, Unknown }

public interface IConnectionService
{
    Task<ConnectError> ConnectAsync(TrustedDevice device, CancellationToken ct);

    Task DisconnectAsync(TrustedDevice device, CancellationToken ct);
}

