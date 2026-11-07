namespace PhoneBrowser.Desktop.ViewModels;

using PhoneBrowser.Desktop.Helpers;

public abstract record GalleryRow;

public record DayHeaderRow(DateTime Date, IReadOnlyList<PhotoTileViewModel> Tiles) : GalleryRow
{
    public string CountText => PolishPlural.Photos(Tiles.Count);
}

public record PhotoTilesRow(IReadOnlyList<PhotoTileViewModel> Tiles) : GalleryRow;