namespace PhoneBrowser.Desktop.Services.Discovery;

using PhoneBrowser.Desktop.Models;
using PhoneBrowser.Desktop.Storage;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Timers;

internal class UdpDiscoveryService : IUdpDiscoveryService
{
    private const int PORT = 47821;

    private const int BroadcastIntervalMs = 2000;

    private readonly UdpClient udpClient;

    private SettingsProfile settings;

    private CancellationTokenSource? internalCts;

    public event Action<DiscoveredDevice>? DeviceDiscovered;

    public UdpDiscoveryService(
        ILocalStore store)
    {
        udpClient = new UdpClient(PORT)
        {
            EnableBroadcast = true,
        };

        settings = store.GetSettingsProfile();
    }

    public async Task StartAsync(CancellationToken ct = default)
    {
        internalCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        var token = internalCts.Token;

        var receiveTask = ReceiveLoopAsync(token);
        var broadcastTask = BroadcastLoopAsync(token);

        await Task.WhenAll(receiveTask, broadcastTask);
    }

    public void Stop()
    {
        internalCts?.Cancel();
    }

    private async Task ReceiveLoopAsync(CancellationToken ct)
    {
        try
        {
            while (!ct.IsCancellationRequested)
            {
                var result = await udpClient.ReceiveAsync(ct);

                ProcessMessage(result);
            }
        }
        catch (OperationCanceledException) { }
    }

    private async Task BroadcastLoopAsync(CancellationToken ct)
    {
        using var timer = new PeriodicTimer(
            TimeSpan.FromMilliseconds(BroadcastIntervalMs));

        try
        {
            await SendDiscoveryAsync();

            while (await timer.WaitForNextTickAsync(ct))
            {
                await SendDiscoveryAsync();
            }
        }
        catch (OperationCanceledException) { }
    }

    private void ProcessMessage(UdpReceiveResult result)
    {
        try
        {
            var json = Encoding.UTF8.GetString(result.Buffer);
            var msg = DiscoveryMessage.FromJson(json);

            if (msg?.type != "ANNOUNCE")
                return;

            var device = new DiscoveredDevice(
                msg.deviceId,
                msg.deviceName ?? "Nieznane urządzenie",
                result.RemoteEndPoint.Address,
                msg.httpPort.Value
            );

            DeviceDiscovered?.Invoke(device);
        }
        catch (Exception ex) {}
    }

    public async Task SendDiscoveryAsync()
    {
        var message = new DiscoveryMessage(settings.Id, settings.DeviceName);

        var json = message.ToJson();

        var data = Encoding.UTF8.GetBytes(json);

        await udpClient.SendAsync(data, data.Length, new IPEndPoint(IPAddress.Broadcast, PORT));
    }

    public void Dispose()
    {
        udpClient.Dispose();
    }
}

