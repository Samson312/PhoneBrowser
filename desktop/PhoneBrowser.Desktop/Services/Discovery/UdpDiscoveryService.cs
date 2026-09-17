namespace PhoneBrowser.Desktop.Services.Discovery;

using Microsoft.Extensions.Logging;
using PhoneBrowser.Desktop.Models;
using PhoneBrowser.Desktop.Services.Network;
using PhoneBrowser.Desktop.Storage;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

internal class UdpDiscoveryService : IUdpDiscoveryService
{
    private readonly ILogger<UdpDiscoveryService> logger;

    private readonly INetworkService networkService;

    private const int PORT = 47821;

    private const int BroadcastIntervalMs = 2000;

    private readonly UdpClient udpClient;

    private SettingsProfile settings;

    private CancellationTokenSource? internalCts;

    public event Action<DiscoveredDevice>? DeviceDiscovered;

    public UdpDiscoveryService(
        ILocalStore store,
        ILogger<UdpDiscoveryService> logger,
        INetworkService networkService)
    {
        this.logger = logger;
        this.networkService = networkService;
        settings = store.GetSettingsProfile();

        try
        {
            udpClient = new UdpClient(PORT) { EnableBroadcast = true };
        }
        catch (SocketException ex)
        {
            logger.LogError(ex, "Failed to bind UDP socket on port {Port}", PORT);
            throw;
        }
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
        while (!ct.IsCancellationRequested)
        {
            try
            {
                var result = await udpClient.ReceiveAsync(ct);
                ProcessMessage(result);
            }
            catch(OperationCanceledException)
            {
                break;
            }
            catch(SocketException ex)
            {
                logger.LogWarning(ex, "UDP receive failed, continuing listen loop");
            }
        }
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
        catch (OperationCanceledException)
        {
            logger.LogDebug("Broadcast loop cancelled");
        }
        catch (SocketException ex)
        {
            logger.LogError(ex, "Failed to send discovery broadcast");
        }
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

            logger.LogDebug("Discovered device {DeviceName} ({DeviceId}) at {Address}",
                device.DeviceName, device.DeviceId, result.RemoteEndPoint.Address);

            DeviceDiscovered?.Invoke(device);
        }
        catch (JsonException ex)
        {
            logger.LogWarning(ex, "Received malformed discovery message from {Endpoint}", result.RemoteEndPoint);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error while processing discovery message from {Endpoint}", result.RemoteEndPoint);
        }
    }

    public async Task SendDiscoveryAsync()
    {
        try
        {
            var message = new DiscoveryMessage(settings.Id, settings.DeviceName);
            var data = Encoding.UTF8.GetBytes(message.ToJson());

            foreach (var broadcastAddress in networkService.GetBroadcastAddresses())
            {
                await udpClient.SendAsync(data, data.Length, new IPEndPoint(broadcastAddress, PORT));
                logger.LogDebug("Sent discovery broadcast to {BroadcastAddress}:{Port}", broadcastAddress, PORT);
            }   
        }
        catch(SocketException ex)
        {
            logger.LogWarning(ex, "Broadcast send failed, will retry next cycle");
        }
    }

    public void Dispose()
    {
        udpClient.Dispose();
    }
}

