namespace PhoneBrowser.Desktop.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using PhoneBrowser.Desktop.Errors;
using PhoneBrowser.Desktop.Models;
using PhoneBrowser.Desktop.Services.Navigation;
using PhoneBrowser.Desktop.Services.Photo;
using System.Collections.ObjectModel;

public partial class PhotoGalleryViewModel : ViewModelBase
{
    private readonly IPhotoSourceService photoSource;
    private readonly INavigationService navigation;
    private readonly ILogger<PhotoGalleryViewModel> logger;
    private readonly TransferDraft transferDraft;

    public ObservableCollection<PhotoTileViewModel> Photos { get; } = new();

    [ObservableProperty]
    private UserMessage? errorMessage = null;

    [ObservableProperty]
    private bool isEmpty = false;

    private readonly CancellationTokenSource cts = new();

    public int SelectedCount => Photos.Count(p => p.IsSelected);

    private bool CanGoNext => SelectedCount > 0;

    public PhotoGalleryViewModel(
        IPhotoSourceService photoSource,
        TransferDraft transferDraft,
        INavigationService navigation,
        ILogger<PhotoGalleryViewModel> logger
        )
    {
        this.photoSource = photoSource;
        this.navigation = navigation;
        this.logger = logger;
        this.transferDraft = transferDraft;

        _ = LoadPhotosAsync();
    }

    private async Task LoadPhotosAsync()
    {

        try
        {
            var result = await photoSource.GetPhotosAsync(cts.Token);

            if (!result.IsSuccess)
            {
                ErrorMessage = ErrorMessages.Gallery(result.Error.Value);
                return;
            }

            var photos = result.Value;

            if (photos.Count == 0)
            {
                IsEmpty = true;
                return;
            }

            foreach (var item in photos.OrderByDescending(p => p.DateTakenUtc))
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

            logger.LogInformation("Loaded {Count} photos", photos.Count);
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            ErrorMessage = ErrorMessages.Gallery(PhotoSourceError.Unknown);
            logger.LogError(ex, "Unexpected error happened");
        }
    }

    public override void Dispose()
    {
        cts.Cancel();
        cts.Dispose();
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
    private void Next() 
    {
        var selectedPhotos = Photos.Where(p => p.IsSelected).Select(p => p.Photo).ToList();

        transferDraft.SetPhotos(selectedPhotos);
        navigation.NavigateTo<SaveLocationViewModel>();
    }
}

