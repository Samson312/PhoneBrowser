using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PhoneBrowser.Desktop.Models;
using PhoneBrowser.Desktop.Services.Navigation;
using PhoneBrowser.Desktop.Services.Photo;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net.Http;
using System.Windows.Media.Imaging;

namespace PhoneBrowser.Desktop.ViewModels;

public partial class PhotoGalleryViewModel : ViewModelBase
{
    private readonly IPhotoSourceService photoSource;
    private readonly INavigationService navigation;

    public ObservableCollection<PhotoTileViewModel> Photos { get; } = new();

    [ObservableProperty]
    private bool isLoading;

    public int SelectedCount => Photos.Count(p => p.IsSelected);

    private bool CanGoNext => SelectedCount > 0;

    public PhotoGalleryViewModel(
        IPhotoSourceService photoSource,
        INavigationService navigation
        )
    {
        this.photoSource = photoSource;
        this.navigation = navigation;

        _ = LoadPhotosAsync();
    }

    private async Task LoadPhotosAsync()
    {
        IsLoading = true;

        var items = await photoSource.GetPhotosAsync(null);

        if (items.Count == 0)
        {
            return;
        }

        foreach (var item in items.OrderByDescending(p => p.DateTakenUtc))
        {
            var tile = new PhotoTileViewModel(item, photoSource);
            tile.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(PhotoTileViewModel.IsSelected))
                {
                    OnPropertyChanged(nameof(SelectedCount));
                    NextCommand.NotifyCanExecuteChanged();
                }
            };
            Photos.Add(tile);
        }
    }

    [RelayCommand]
    private void ToggleDay(IEnumerable<object> items)
    {
        var tiles = items.Cast<PhotoTileViewModel>().ToList();
        bool allSelected = tiles.All(t => t.IsSelected);

        foreach (var tile in tiles)
            tile.IsSelected = !allSelected;
    }

    [RelayCommand]
    private void Back() => navigation.GoBack();

    [RelayCommand(CanExecute = nameof(CanGoNext))]
    private void Next() {}
}

