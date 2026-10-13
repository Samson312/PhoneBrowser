namespace PhoneBrowser.Desktop.Errors;

public static partial class ErrorMessages
{
    public static UserMessage? SaveLocation(SaveLocationError error, long requiredBytes = 0) => error switch
    {
        SaveLocationError.NoPermission => new(
            "Nie możemy zapisać zdjęć w tym folderze",
            "Wybierz inny folder, na przykład „Zdjęcia”."),

        SaveLocationError.Unavailable => new(
            "Ten folder jest niedostępny",
            "Sprawdź, czy dysk jest podłączony, albo wybierz inny folder."),

        SaveLocationError.NotEnoughSpace => new(
            "Za mało miejsca na dysku",
            $"Potrzeba około {FormatSize(requiredBytes)}. Wybierz inny folder albo zwolnij miejsce."),

        SaveLocationError.Unknown => new(
            "Coś poszło nie tak",
            "Spróbuj wybrać inny folder. Jeśli to nie pomoże, uruchom aplikację ponownie."),

        SaveLocationError.None => null,

        _ => throw new ArgumentOutOfRangeException(nameof(error), error, null)
    };

    private static string FormatSize(long bytes)
    {
        const double MB = 1024 * 1024;
        const double GB = MB * 1024;

        return bytes >= GB
            ? $"{Math.Ceiling(bytes / GB * 10) / 10:0.#} GB"
            : $"{Math.Max(1, Math.Ceiling(bytes / MB))} MB";
    }
}

