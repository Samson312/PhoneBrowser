namespace PhoneBrowser.Desktop.Errors;

using PhoneBrowser.Desktop.Services.Photo;
using System;

public static partial class ErrorMessages
{
    public static UserMessage Gallery(PhotoSourceError error) => error switch
    {
        PhotoSourceError.Unauthorized => new(
            "Połącz telefon z komputerem ponownie",
            "Wróć do poprzedniego kroku i połącz się z telefonem ponownie."),

        PhotoSourceError.Unreachable => new(
            "Telefon nie odpowiada",
            "Sprawdź, czy telefon i komputer są w tej samej sieci Wi-Fi, i spróbuj ponownie."),

        PhotoSourceError.NotFound => new(
            "Coś poszło nie tak",
            "Uruchom aplikacje jeszcze raz i spróbuj ponownie"),

        PhotoSourceError.Unknown => new(
            "Coś poszło nie tak",
            "Uruchom aplikację jeszcze raz i spróbuj ponownie"),

        _ => throw new ArgumentOutOfRangeException(nameof(error), error, null)
    };
}

