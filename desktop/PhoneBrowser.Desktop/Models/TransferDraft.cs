namespace PhoneBrowser.Desktop.Models;

public class TransferDraft
{
    public IReadOnlyList<PhotoItem> Photos { get; private set; } = [];
    public string? TargetDirectory { get; private set; }

    public long TotalBytes => Photos.Sum(p => p.SizeBytes);

    public void SetPhotos(IEnumerable<PhotoItem> photos) => Photos = photos.ToList();
    public void SetTargetDirectory(string path) => TargetDirectory = path;

    public void Reset()
    {
        Photos = [];
        TargetDirectory = null;
    }
}

