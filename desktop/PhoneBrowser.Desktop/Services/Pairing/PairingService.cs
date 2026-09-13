namespace PhoneBrowser.Desktop.Services.Pairing;

using System.Net.Http;
using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using PhoneBrowser.Desktop.Models;
using PhoneBrowser.Desktop.Storage;

internal class PairingService: IPairingService
{
    private readonly HttpClient httpClient;
    private readonly ILogger<PairingService> logger;

    private SettingsProfile settings;

    public PairingService(
        ILocalStore store,
        IHttpClientFactory httpClientFactory,
        ILogger<PairingService> logger
        )
    {
        settings = store.GetSettingsProfile();
        httpClient = httpClientFactory.CreateClient();
        this.logger = logger;
    }

    public async Task<string?> PairAsync(DiscoveredDevice device, CancellationToken ct)
    {
        var requestId = Guid.NewGuid().ToString();
        var baseUrl = $"http://{device.IpAddress}:{device.HttpPort}";

       
        var request = new PairingRequestDto(requestId, new DeviceInfoDto(settings.Id, settings.DeviceName, 1));

        try
        {
            var postResponse = await httpClient.PostAsJsonAsync($"{baseUrl}/pairing/request", request, ct);
            if (!postResponse.IsSuccessStatusCode)
            {
                logger.LogWarning("Pairing request to {DeviceName} rejected with status {Status}",
                    device.DeviceName, postResponse.StatusCode);
                return null;
            }
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Failed to reach {DeviceName} at {BaseUrl} while sending pairing request", device.DeviceName, baseUrl);
            return null;
        }


        var deadline = DateTime.UtcNow.AddSeconds(30);

        while (DateTime.UtcNow < deadline)
        {
            await Task.Delay(1000, ct);

            PairingStatusResponseDto? statusResponse;
            try
            {
                statusResponse = await httpClient.GetFromJsonAsync<PairingStatusResponseDto>(
                    $"{baseUrl}/pairing/status/{requestId}", ct);
            }
            catch (HttpRequestException ex)
            {
                logger.LogWarning(ex, "Poll failed for pairing request {RequestId}, retrying", requestId);
                continue;
            }
            catch (TaskCanceledException) when (!ct.IsCancellationRequested)
            {
                logger.LogWarning("Pairing status poll timed out for request {RequestId}", requestId);
                continue;
            }

            if (statusResponse == null) continue;

            switch (statusResponse.status)
            {
                case "ACCEPTED":
                    logger.LogInformation("Pairing accepted by {DeviceName}", device.DeviceName);
                    return statusResponse.pairingToken;
                case "REJECTED":
                    logger.LogInformation("Pairing rejected by {DeviceName}", device.DeviceName);
                    return null;
                case "EXPIRED":
                    logger.LogInformation("Pairing request {RequestId} expired", requestId);
                    return null;
            }
        }

        logger.LogWarning("Pairing request {RequestId} to {DeviceName} timed out after 30s", requestId, device.DeviceName);
        return null;

    }
}