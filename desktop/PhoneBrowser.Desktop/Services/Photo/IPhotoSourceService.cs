namespace PhoneBrowser.Desktop.Services.Photo;

using PhoneBrowser.Desktop.Models;
using System.IO;
using System.Windows.Media.Imaging;

public interface IPhotoSourceService
{
    Task<IReadOnlyList<PhotoItem>> GetPhotosAsync(long? since);

    Task<BitmapImage?> GetThumbnailAsync(string photoId);

    Task<Stream?> GetFullPhotoAsync(string photoId);
}

