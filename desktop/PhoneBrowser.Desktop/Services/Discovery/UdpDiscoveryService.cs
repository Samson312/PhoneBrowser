namespace PhoneBrowser.Desktop.Services.Discovery;

using PhoneBrowser.Desktop.Models;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

internal class UdpDiscoveryService : IDisposable
{
    private readonly UdpClient udpClient;

    public event Action<DiscoveredDevice>? DeviceDiscovered;

    public UdpDiscoveryService(int port = 2000)
    {
        udpClient = new UdpClient(port);
    }

    public async Task StartAsync(CancellationToken ct = default)
    {
        try
        {
            while (!ct.IsCancellationRequested)
            {
                var result = await udpClient.ReceiveAsync(ct);
                var json = Encoding.UTF8.GetString(result.Buffer);

                var msg = DiscoveryMessage.FromJson(json);

                if(msg?.Type == "BROADCAST")
                {
                    var device = new DiscoveredDevice(
                    result.RemoteEndPoint.Address,
                    result.RemoteEndPoint.Port,
                    msg.DeviceName ?? "Nieznane urządzenie");

                    DeviceDiscovered?.Invoke(device);
                }  
            }
        }
        catch (OperationCanceledException) { }
    }

    public async Task SendPairReplayAsync(DiscoveredDevice device, int tcpPort)
    {
        var json = new DiscoveryMessage("PAIR_REPLY", TcpPort: tcpPort).ToJson();
        var data = Encoding.UTF8.GetBytes(json);
        await udpClient.SendAsync(data, data.Length, new IPEndPoint(device.IpAddress, device.Port));
    }

    public void Dispose()
    {
        udpClient.Dispose();
    }
}

