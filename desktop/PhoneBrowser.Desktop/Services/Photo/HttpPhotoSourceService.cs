namespace PhoneBrowser.Desktop.Services.Photo;

using PhoneBrowser.Desktop.Models;
using PhoneBrowser.Desktop.Services.Pairing;
using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Windows.Media.Imaging;

internal class HttpPhotoSourceService : IPhotoSourceService
{
    private readonly PairedDeviceService pairedDevice;

    private readonly HttpClient httpClient;

    public HttpPhotoSourceService(
        PairedDeviceService pairedDevice,
        IHttpClientFactory httpClientFactory
    ){
        this.pairedDevice = pairedDevice;
        httpClient = httpClientFactory.CreateClient();
    }

    public async Task<IReadOnlyList<PhotoItem>> GetPhotosAsync(long? since = null)
    {
        var baseUrl = pairedDevice.BaseUrl
            ?? throw new InvalidOperationException("Brak połączonego urządzenia.");

        var url = $"{baseUrl}/photos";

        if (since.HasValue)
            url += $"?since={since.Value}";

        using var request = AuthorizedRequest(HttpMethod.Get, url);

        var response = await httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();


        return await response.Content.ReadFromJsonAsync<List<PhotoItem>>()
               ?? new List<PhotoItem>();
    }

    public async Task<BitmapImage?> GetThumbnailAsync(string photoId)
    {
        var baseUrl = pairedDevice.BaseUrl
            ?? throw new InvalidOperationException("Brak połączonego urządzenia.");

        try 
        {
            using var request = AuthorizedRequest(HttpMethod.Get, $"{baseUrl}/photos/{photoId}/thumbnail");

            using var response = await httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var bytes = await response.Content.ReadAsByteArrayAsync();

            using var stream = new MemoryStream(bytes);

            var bitmap = new BitmapImage();

            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.StreamSource = stream;
            bitmap.EndInit();
            bitmap.Freeze();

            return bitmap;
        }
        catch(Exception) 
        {
            return null;
        }
    }

    public async Task<Stream?> GetFullPhotoAsync(string photoId)
    {
        throw new NotImplementedException();
    }

    private HttpRequestMessage AuthorizedRequest(HttpMethod method, string url)
    {
        var request = new HttpRequestMessage(method, url);
        request.Headers.Authorization = new("Bearer", pairedDevice.AuthToken);
        return request;
    }
}

