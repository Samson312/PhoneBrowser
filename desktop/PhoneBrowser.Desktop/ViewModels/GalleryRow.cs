namespace PhoneBrowser.Desktop.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using PhoneBrowser.Desktop.Helpers;

public abstract class GalleryRow : ObservableObject { }

public sealed class DayHeaderRow : GalleryRow
{
    public DateTime Date { get; }
    public IReadOnlyList<PhotoTileViewModel> Tiles { get; }

    public string CountText => PolishPlural.Photos(Tiles.Count);

    public string ToggleText => Tiles.All(t => t.IsSelected) ? "Odznacz dzień" : "Zaznacz dzień";

    public DayHeaderRow(DateTime date, IReadOnlyList<PhotoTileViewModel> tiles)
    {
        Date = date;
        Tiles = tiles;
    }

    public void Refresh() => OnPropertyChanged(nameof(ToggleText));
}

public sealed class PhotoTilesRow(IReadOnlyList<PhotoTileViewModel> tiles) : GalleryRow
{
    public IReadOnlyList<PhotoTileViewModel> Tiles { get; } = tiles;
}