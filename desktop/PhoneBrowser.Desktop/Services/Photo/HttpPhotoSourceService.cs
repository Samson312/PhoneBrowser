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

        return await httpClient.GetFromJsonAsync<PhotoItem[]>(url)
            ?? Array.Empty<PhotoItem>();
    }

    public async Task<BitmapImage?> GetThumbnailAsync(string photoId)
    {
        var baseUrl = pairedDevice.BaseUrl
            ?? throw new InvalidOperationException("Brak połączonego urządzenia.");

        try 
        {
            var bytes = await httpClient.GetByteArrayAsync(
                $"{baseUrl}/photos/{photoId}/thumbnail");

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
}

