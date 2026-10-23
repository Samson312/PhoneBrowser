namespace PhoneBrowser.Desktop.Errors;

using PhoneBrowser.Desktop.Services.Connection;

public static partial class ErrorMessages
{
    public static UserMessage? Connection(ConnectError error) => error switch
    {
        ConnectError.Busy => new(
            "Do telefonu jest podłączone inne urządzenie",
            "W aplikacji na telefonie rozłącz się z obecnie podłączonym urządzeniem i spróbuj ponownie"),

        ConnectError.Unauthorized => new(
            "Błąd przy próbie połączenia",
            "Urządzenia muszą być najpierw sparowane"),

        ConnectError.Unreachable => new(
            "Telefon nie odpowiada",
            "Sprawdź, czy telefon i komputer są w tej samej sieci Wi-Fi, i spróbuj ponownie."),

        ConnectError.Unknown => new(
            "Coś poszło nie tak",
            "Uruchom aplikacje jeszcze raz i spróbuj ponownie"),

        ConnectError.None => null,

        _ => throw new ArgumentOutOfRangeException(nameof(error), error, null)
    };
}

