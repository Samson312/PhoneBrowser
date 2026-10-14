namespace PhoneBrowser.Desktop.Services.Photo;

using PhoneBrowser.Desktop.Errors;
using PhoneBrowser.Desktop.Models;
using System.IO;
using System.Windows.Media.Imaging;

public enum PhotoSourceError { Unreachable, NotFound, Unauthorized, Unknown }

public interface IPhotoSourceService
{
    Task<IReadOnlyList<PhotoItem>> GetPhotosAsync(long? since);

    Task<BitmapImage?> GetThumbnailAsync(string photoId);

    Task<Result<Stream, PhotoSourceError>> GetFullPhotoAsync(string photoId, CancellationToken ct);
}

