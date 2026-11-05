namespace PhoneBrowser.Desktop.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using PhoneBrowser.Desktop.Errors;
using PhoneBrowser.Desktop.Helpers;
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

    public string ButtonText => SelectedCount > 0 
        ? $"Prześlij {PolishPlural.Photos(SelectedCount)}" 
        : "Wybierz zdjęcia";

    public DateNavigationViewModel DateNavigation { get; } = new();

    public ObservableCollection<GalleryRow> Rows { get; } = new();

    public event Action<int>? ScrollToRowRequested;

    private int columnCount = 4;

    [ObservableProperty]
    private UserMessage? errorMessage = null;

    [ObservableProperty]
    private bool isEmpty = false;

    private readonly CancellationTokenSource cts = new();

    private int SelectedCount => Photos.Count(p => p.IsSelected);

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

        DateNavigation.MonthSelected += OnMonthSelected;

        _ = LoadPhotosAsync();
    }
    public override void Dispose()
    {
        DateNavigation.MonthSelected -= OnMonthSelected;
        cts.Cancel();
        cts.Dispose();
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
                        OnPropertyChanged(nameof(ButtonText));
                        NextCommand.NotifyCanExecuteChanged();
                    }
                };
                Photos.Add(tile);
            }
            RebuildRows();
            DateNavigation.Load(photos);

            logger.LogInformation("Loaded {Count} photos", photos.Count);
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            ErrorMessage = ErrorMessages.Gallery(PhotoSourceError.Unknown);
            logger.LogError(ex, "Unexpected error happened");
        }
    }

    public void SetColumnCount(int count)
    {
        if (count == columnCount) return;

        columnCount = count;
        RebuildRows();
    }

    private void RebuildRows()
    {
        Rows.Clear();

        foreach (var day in Photos.GroupBy(p => p.Photo.DateTakenUtc.Date))
        {
            var tiles = day.ToList();

            Rows.Add(new DayHeaderRow(day.Key, tiles));

            foreach (var chunk in tiles.Chunk(columnCount))
                Rows.Add(new PhotoTilesRow(chunk));
        }
    }

    private void OnMonthSelected(MonthItem month)
    {
        var newest = Photos.FirstOrDefault(p =>
            p.Photo.DateTakenUtc.Year == month.Year &&
            p.Photo.DateTakenUtc.Month == month.Month);

        if (newest == null) return;

        var date = newest.Photo.DateTakenUtc.Date;

        for (var i = 0; i < Rows.Count; i++)
        {
            var header = Rows[i] as DayHeaderRow;

            if (header != null && header.Date == date)
            {
                ScrollToRowRequested?.Invoke(i);
                return;
            }
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
    private void Next() 
    {
        var selectedPhotos = Photos.Where(p => p.IsSelected).Select(p => p.Photo).ToList();

        transferDraft.SetPhotos(selectedPhotos);
        navigation.NavigateTo<SaveLocationViewModel>();
    }
}

