namespace PhoneBrowser.Desktop.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PhoneBrowser.Desktop.Services.Photo;
using PhoneBrowser.Desktop.Models;
using System.Windows.Media;

public partial class PhotoTileViewModel : ObservableObject
{
    public PhotoItem Photo { get; }

    [ObservableProperty]
    private ImageSource? thumbnail;

    [ObservableProperty]
    private bool thumbnailFailed;

    [ObservableProperty]
    private bool isSelected;

    public PhotoTileViewModel(PhotoItem photo, IPhotoSourceService photoSource)
    {
        Photo = photo;
        _ = LoadThumbnailAsync(photoSource);
    }

    private async Task LoadThumbnailAsync(IPhotoSourceService photoSource)
    {
        var bitmap = await photoSource.GetThumbnailAsync(Photo.PhotoId);

        if (bitmap is null)
        {
            ThumbnailFailed = true; // TODO: placeholder w XAML zamiast pustego kafelka
            return;
        }

        Thumbnail = bitmap;
    }

    [RelayCommand]
    private void Toggle() => IsSelected = !IsSelected;
}

