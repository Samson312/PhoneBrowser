namespace PhoneBrowser.Desktop.Services.Connection;

using Microsoft.Extensions.Logging;
using PhoneBrowser.Desktop.Extensions;
using PhoneBrowser.Desktop.Models;
using System.Net;
using System.Net.Http;

internal class ConnectionService : IConnectionService
{
    private readonly HttpClient httpClient;
    private readonly PairedDevice pairedDevice;
    private readonly ILogger<ConnectionService> logger;

    public ConnectionService(
        IHttpClientFactory httpClientFactory,
        PairedDevice pairedDevice,
        ILogger<ConnectionService> logger)
    {
        this.httpClient = httpClientFactory.CreateClient();
        this.pairedDevice = pairedDevice;
        this.logger = logger;
    }

    public async Task<ConnectError> ConnectAsync(TrustedDevice device, CancellationToken ct)
    {
        var url = $"http://{device.LastKnownIpAddress}:{device.LastKnownPort}/connect";

        using var request = new HttpRequestMessage(HttpMethod.Post, url)
                                .WithBearerToken(device.AuthToken);
        try
        {
            using var response = await httpClient.SendAsync(request, ct);

            if (!response.IsSuccessStatusCode)
            {
                var status = response.StatusCode;

                return status switch
                {
                    HttpStatusCode.Unauthorized => ConnectError.Unauthorized,
                    HttpStatusCode.Conflict => ConnectError.Busy,
                    _ => ConnectError.Unknown
                };
            }
        }
        catch(HttpRequestException ex)
        {
            logger.LogWarning(ex, "Phone {DeviceName} unreachable while trying to connect", device.DeviceName);
            return ConnectError.Unreachable;
        }
        catch(TaskCanceledException ex) when (!ct.IsCancellationRequested)
        {
            logger.LogWarning(ex, "Timeout while trying to connect to {DeviceName}", device.DeviceName);
            return ConnectError.Unreachable;
        }
        catch(OperationCanceledException)
        {
            throw; 
        }
        catch(Exception ex)
        {
            logger.LogWarning(ex, "Unhandled exception while trying to connect to {DeviceName}", device.DeviceName);
            return ConnectError.Unknown;
        }

        pairedDevice.SetDevice(device);
        return ConnectError.None;
    }

    public async Task DisconnectAsync(TrustedDevice device, CancellationToken ct)
    {
        var url = $"http://{device.LastKnownIpAddress}:{device.LastKnownPort}/disconnect";

        using var request = new HttpRequestMessage(HttpMethod.Post, url)
                                .WithBearerToken(device.AuthToken);
        try
        {
            using var response = await httpClient.SendAsync(request, ct);

            var status = response.StatusCode;

            if (response.IsSuccessStatusCode)
                logger.LogDebug("Disconnected from {DeviceName}, status {Status}",device.DeviceName, status);
            else
                logger.LogWarning("Failed to disconnect from {DeviceName}, status: {Status}",device.DeviceName, status);

        }
        catch (HttpRequestException ex)
        {
            logger.LogWarning(ex, "Phone {DeviceName} unreachable while trying to disconnect", device.DeviceName);
        }
        catch (TaskCanceledException ex) when (!ct.IsCancellationRequested)
        {
            logger.LogWarning(ex, "Timeout while trying to disconnect from {DeviceName}", device.DeviceName);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Unhandled exception while trying to disconnect from {DeviceName}", device.DeviceName);
        }
        finally
        {
            pairedDevice.Clear();
        }
    }
}

