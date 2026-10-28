namespace PhoneBrowser.Desktop.Services.Photo;

using Microsoft.Extensions.Logging;
using PhoneBrowser.Desktop.Errors;
using PhoneBrowser.Desktop.Extensions;
using PhoneBrowser.Desktop.Models;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Windows.Media.Imaging;

internal class HttpPhotoSourceService : IPhotoSourceService
{
    private readonly PairedDevice pairedDevice;

    private readonly HttpClient httpClient;
    private readonly ILogger<HttpPhotoSourceService> logger;

    public HttpPhotoSourceService(
        PairedDevice pairedDevice,
        IHttpClientFactory httpClientFactory,
        ILogger<HttpPhotoSourceService> logger
    ){
        this.pairedDevice = pairedDevice;
        httpClient = httpClientFactory.CreateClient();
        this.logger = logger;
    }

    public async Task<Result<IReadOnlyList<PhotoItem>,PhotoSourceError>> GetPhotosAsync(CancellationToken ct)
    {
        var baseUrl = pairedDevice.BaseUrl;

        if (baseUrl == null)
            return Result<IReadOnlyList<PhotoItem>, PhotoSourceError>.Fail(PhotoSourceError.Unreachable);

        var url = $"{baseUrl}/photos";

        try
        {
            using var request = new HttpRequestMessage(
                HttpMethod.Get,
                url).WithBearerToken(pairedDevice.CurrentDevice.AuthToken);

            using var response = await httpClient.SendAsync(request, ct);

            if (!response.IsSuccessStatusCode)
            {
                var status = response.StatusCode;
                logger.LogWarning("Photo list request failed with {Status}", status);

                return Result<IReadOnlyList<PhotoItem>, PhotoSourceError>.Fail(status switch
                {
                    HttpStatusCode.Unauthorized => PhotoSourceError.Unauthorized,
                    _ => PhotoSourceError.Unknown
                });
            }

            var photoList = await response.Content.ReadFromJsonAsync<List<PhotoItem>>(ct);

            if (photoList == null)
                return Result<IReadOnlyList<PhotoItem>, PhotoSourceError>.Fail(PhotoSourceError.Unknown);

            return Result<IReadOnlyList<PhotoItem>, PhotoSourceError>.Ok(photoList);
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Failed to fetch photo list from {Url}", url);
            return Result<IReadOnlyList<PhotoItem>, PhotoSourceError>.Fail(PhotoSourceError.Unreachable);
        }
        catch (JsonException ex)
        {
            logger.LogError(ex, "Failed to parse photo list response from {Url}", url);
            return Result<IReadOnlyList<PhotoItem>, PhotoSourceError>.Fail(PhotoSourceError.Unknown);
        }
        catch (TaskCanceledException ex) when (!ct.IsCancellationRequested)
        {
            logger.LogWarning(ex, "Photo list request to {Url} timed out", url);
            return Result<IReadOnlyList<PhotoItem>, PhotoSourceError>.Fail(PhotoSourceError.Unreachable);
        }
    }

    public async Task<BitmapImage?> GetThumbnailAsync(string photoId)
    {
        var baseUrl = pairedDevice.BaseUrl;

        if (baseUrl == null) return null;

        try 
        {
            using var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"{baseUrl}/photos/{photoId}/thumbnail")
                .WithBearerToken(pairedDevice.CurrentDevice.AuthToken);

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
        catch(Exception ex) 
        {
            logger.LogDebug(ex, "Thumbnail fetch failed for photo {PhotoId}", photoId);
            return null;
        }
    }

    public async Task<Result<Stream, PhotoSourceError>> GetFullPhotoAsync(string photoId, CancellationToken ct)
    {
        var baseUrl = pairedDevice.BaseUrl;

        if (baseUrl == null) return Result<Stream, PhotoSourceError>.Fail(PhotoSourceError.Unreachable);

        HttpResponseMessage? response = null;
        try
        {
            using var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"{baseUrl}/photos/{photoId}/full")
                .WithBearerToken(pairedDevice.CurrentDevice.AuthToken);

            response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);

            if (!response.IsSuccessStatusCode)
            {
                var status = response.StatusCode;
                response.Dispose();
                logger.LogWarning("Full photo {PhotoId} request failed with {Status}", photoId, status);

                return Result<Stream, PhotoSourceError>.Fail(status switch
                {
                    HttpStatusCode.NotFound => PhotoSourceError.NotFound,
                    HttpStatusCode.Unauthorized => PhotoSourceError.Unauthorized,
                    _ => PhotoSourceError.Unknown
                });
            }

            return Result<Stream, PhotoSourceError>.Ok(await response.Content.ReadAsStreamAsync(ct));
        }
        catch (HttpRequestException ex)
        {
            response?.Dispose();
            logger.LogWarning(ex, "Phone unreachable while fetching photo {PhotoId}", photoId);
            return Result<Stream, PhotoSourceError>.Fail(PhotoSourceError.Unreachable);
        }
        catch (TaskCanceledException ex) when (!ct.IsCancellationRequested)
        {
            response?.Dispose();
            logger.LogWarning(ex, "Timeout while fetching photo {PhotoId}", photoId);
            return Result<Stream, PhotoSourceError>.Fail(PhotoSourceError.Unreachable);
        }
        catch (OperationCanceledException)
        {
            response?.Dispose();
            throw;
        }
    }
}

