namespace PhoneBrowser.Desktop.ViewModels;

public abstract record GalleryRow;

public record DayHeaderRow(DateTime Date, IReadOnlyList<PhotoTileViewModel> Tiles) : GalleryRow;

public record PhotoTilesRow(IReadOnlyList<PhotoTileViewModel> Tiles) : GalleryRow;