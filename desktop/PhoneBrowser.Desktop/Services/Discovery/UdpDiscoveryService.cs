namespace PhoneBrowser.Desktop.Services.Discovery;

using System;
using System.Net.Sockets;
using System.Text;
using PhoneBrowser.Desktop.Models;

internal class UdpDiscoveryService : IDisposable
{
    private readonly UdpClient udpClient;

    public UdpDiscoveryService(int port = 2000)
    {
        udpClient = new UdpClient(port);
    }

    public async Task<DiscoveredDevice> Listen(CancellationToken ct = default)
    {
        var result = await udpClient.ReceiveAsync(ct);

        return new DiscoveredDevice(
            result.RemoteEndPoint.Address,
            Encoding.UTF8.GetString(result.Buffer));
    }

    public void Dispose()
    {
        udpClient.Dispose();
    }
}

