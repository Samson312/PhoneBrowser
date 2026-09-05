namespace PhoneBrowser.Desktop.Services.Pairing;

using System.Net.Http;
using System.Net.Http.Json;
using PhoneBrowser.Desktop.Models;
using PhoneBrowser.Desktop.Storage;

internal class PairingService: IPairingService
{
    private readonly HttpClient http = new() { };

    private SettingsProfile settings;

    public PairingService(
        ILocalStore store
        )
    {
        settings = store.GetSettingsProfile();
    }

    public async Task<string?> PairAsync(DiscoveredDevice device, CancellationToken ct)
    {
        var requestId = Guid.NewGuid().ToString();
        var baseUrl = $"http://{device.IpAddress}:{device.HttpPort}";

        var request = new PairingRequestDto(
            requestId,
            new DeviceInfoDto(settings.Id, settings.DeviceName, "Windows", 1) 
        );

        var postResponse = await http.PostAsJsonAsync($"{baseUrl}/pairing/request", request, ct);
        if (!postResponse.IsSuccessStatusCode)
            return null;

        var deadline = DateTime.UtcNow.AddSeconds(30);
        while (DateTime.UtcNow < deadline)
        {
            await Task.Delay(1000, ct);

            var statusResponse = await http.GetFromJsonAsync<PairingStatusResponseDto>(
                $"{baseUrl}/pairing/status/{requestId}", ct);

            if (statusResponse is null) continue;

            switch (statusResponse.status)
            {
                case "ACCEPTED":
                    return statusResponse.pairingToken;
                case "REJECTED":
                case "EXPIRED":
                    return null;
            }
        }

        return null;
    }
}