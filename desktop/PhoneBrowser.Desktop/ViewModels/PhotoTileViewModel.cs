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
    private bool isSelected;

    private readonly IPhotoSourceService photoSource;

    private bool thumbnailRequested = false;

    public PhotoTileViewModel(PhotoItem photo, IPhotoSourceService photoSource)
    {
        this.photoSource = photoSource;
        Photo = photo;
    }

    [RelayCommand]
    private async Task LoadThumbnailAsync()
    {
        if (thumbnailRequested == true) return;
            thumbnailRequested = true;

        var bitmap = await photoSource.GetThumbnailAsync(Photo.PhotoId);

        if (bitmap == null)
        {
            thumbnailRequested = false; 
            return;
        }

        Thumbnail = bitmap;
    }

    [RelayCommand]
    private void Toggle() => IsSelected = !IsSelected;
}

