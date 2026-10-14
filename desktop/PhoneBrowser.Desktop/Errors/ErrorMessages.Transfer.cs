namespace PhoneBrowser.Desktop.Errors;

public static partial class ErrorMessages
{
    public static UserMessage? Transfer(TransferError error, int transferred, int total) => error switch
    {
        TransferError.ConnectionLost => new(
            "Połączenie zostało przerwane",
            $"Przesłano {transferred} z {total}. Sprawdź, czy telefon i komputer są w tej samej sieci Wi-Fi, i spróbuj ponownie."),

        TransferError.Unauthorized => new(
            "Połącz telefon z komputerem ponownie",
            $"Przesłano {transferred} z {total}. Połącz telefon z komputerem ponownie, aby dokończyć przesyłanie zdjęć."),

        TransferError.NoPermission => new(
            "Nie możemy zapisać zdjęć w tym folderze",
            $"Przesłano {transferred} z {total}. Wróć i wybierz inny folder, na przykład „Zdjęcia”."),

        TransferError.NotEnoughSpace => new(
            "Skończyło się miejsce na dysku",
            $"Przesłano {transferred} z {total}. Zwolnij miejsce albo wróć i wybierz inny folder."),

        TransferError.SomePhotosMissing => new(
            "Nie wszystkie zdjęcia udało się przesłać",
            $"Przesłano {transferred} z {total}. Niektórych zdjęć nie ma już w telefonie."),

        TransferError.Unknown => new(
            "Coś poszło nie tak",
            $"Przesłano {transferred} z {total}. Spróbuj ponownie. Jeśli to nie pomoże, uruchom aplikację ponownie."),

        TransferError.None => null,

        _ => throw new ArgumentOutOfRangeException(nameof(error), error, null)
    };
}

