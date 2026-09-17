namespace PhoneBrowser.Desktop.Services.Network;

using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

public class NetworkService : INetworkService, IDisposable
{
    private readonly ILogger<NetworkService> logger;

    private IReadOnlyList<IPAddress>? cachedBroadcastAddresses;

    public NetworkService(ILogger<NetworkService> logger)
    {
        this.logger = logger;
        NetworkChange.NetworkAddressChanged += InvalidateCache;
    }

    public IReadOnlyList<IPAddress> GetBroadcastAddresses()
    {
        return cachedBroadcastAddresses ??= CalculateBroadcastAddresses();
    }

    public void Dispose()
    {
        NetworkChange.NetworkAddressChanged -= InvalidateCache;
    }

    private void InvalidateCache(object? sender, EventArgs e)
    {
        logger.LogDebug("Network address changed, invalidating broadcast address cache");
        cachedBroadcastAddresses = null;
    }

    private List<IPAddress> CalculateBroadcastAddresses()
    {
        NetworkInterface[] networkInterfaces;

        try
        {
            networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
        }
        catch (NetworkInformationException ex)
        {
            logger.LogError(ex, "Failed to enumerate network interfaces");
            return new List<IPAddress>();
        }

        var broadcastAddresses = new List<IPAddress>();

        foreach (var networkInterface in networkInterfaces)
        {
            if (networkInterface.OperationalStatus != OperationalStatus.Up ||
                networkInterface.NetworkInterfaceType == NetworkInterfaceType.Loopback)
                continue;

            var ipProperties = networkInterface.GetIPProperties();

            foreach (var unicastAddress in ipProperties.UnicastAddresses)
            {
                if (unicastAddress.Address.AddressFamily == AddressFamily.InterNetwork)
                {
                    var ipAddress = unicastAddress.Address;
                    var subnetMask = unicastAddress.IPv4Mask;
                    if (subnetMask != null)
                    {
                        var broadcastAddress = GetBroadcastAddress(ipAddress, subnetMask);
                        broadcastAddresses.Add(broadcastAddress);
                    }
                }
            }
        }
        cachedBroadcastAddresses = broadcastAddresses;

        return broadcastAddresses;
    }

    private IPAddress GetBroadcastAddress(IPAddress ipAddress, IPAddress subnetMask)
    {
        var ipBytes = ipAddress.GetAddressBytes();
        var maskBytes = subnetMask.GetAddressBytes();
        var broadcastBytes = new byte[ipBytes.Length];
        for (int i = 0; i < ipBytes.Length; i++)
        {
            broadcastBytes[i] = (byte)(ipBytes[i] | ~maskBytes[i]);
        }
        return new IPAddress(broadcastBytes);
    }
}

