namespace PhoneBrowser.Desktop.Services.Network;

using System.Net;

public interface INetworkService
{
    IReadOnlyList<IPAddress> GetBroadcastAddresses();
}

