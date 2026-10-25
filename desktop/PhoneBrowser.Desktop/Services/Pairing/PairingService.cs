namespace PhoneBrowser.Desktop.Services.Pairing;

using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using PhoneBrowser.Desktop.Errors;
using PhoneBrowser.Desktop.Models;
using PhoneBrowser.Desktop.Storage;

internal class PairingService: IPairingService
{
    private readonly HttpClient httpClient;
    private readonly ILogger<PairingService> logger;

    private SettingsProfile settings;

    private readonly TimeSpan PairingTimeout = TimeSpan.FromSeconds(30);

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

    public async Task<Result<string, PairingError>> PairAsync(DiscoveredDevice device, CancellationToken ct)
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

                if(postResponse.StatusCode == HttpStatusCode.Conflict)
                    return Result<string, PairingError>.Fail(PairingError.Busy);

                return Result<string, PairingError>.Fail(PairingError.Unknown);
            }
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Failed to reach {DeviceName} at {BaseUrl} while sending pairing request", device.DeviceName, baseUrl);
            return Result<string, PairingError>.Fail(PairingError.Unreachable);
        }
        catch (TaskCanceledException) when (!ct.IsCancellationRequested)
        {
            logger.LogWarning("Pairing status poll timed out for request {RequestId}", requestId);
            return Result<string, PairingError>.Fail(PairingError.Unreachable);
        }


        var deadline = DateTime.UtcNow.Add(PairingTimeout);
        bool anyPollSucceeded = false;

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
            catch (JsonException ex)
            {
                logger.LogError(ex, "Phone responded with wrong format");
                return Result<string, PairingError>.Fail(PairingError.Unknown);
            }

            if (statusResponse == null) continue;

            anyPollSucceeded = true;

            switch (statusResponse.status)
            {
                case "ACCEPTED":
                    if (string.IsNullOrEmpty(statusResponse.pairingToken))
                    {
                        logger.LogError("Pairing accepted by {DeviceName} but no token provided", device.DeviceName);
                        return Result<string, PairingError>.Fail(PairingError.Unknown);
                    }

                    logger.LogInformation("Pairing accepted by {DeviceName}", device.DeviceName);
                    return Result<string, PairingError>.Ok(statusResponse.pairingToken);
                case "REJECTED":
                    logger.LogInformation("Pairing rejected by {DeviceName}", device.DeviceName);
                    return Result<string, PairingError>.Fail(PairingError.Rejected);
                case "EXPIRED":
                    logger.LogInformation("Pairing request {RequestId} expired", requestId);
                    return Result<string, PairingError>.Fail(PairingError.TimedOut);
            }
        }

        if (anyPollSucceeded)
        {
            logger.LogWarning("Pairing request {RequestId} to {DeviceName} timed out after {Timeout}s", requestId, device.DeviceName, PairingTimeout.TotalSeconds);
            return Result<string, PairingError>.Fail(PairingError.TimedOut);
        }

        logger.LogWarning("Failed to reach {DeviceName} at {BaseUrl} while sending pairing request", device.DeviceName, baseUrl);
        return Result<string, PairingError>.Fail(PairingError.Unreachable);
    }
}