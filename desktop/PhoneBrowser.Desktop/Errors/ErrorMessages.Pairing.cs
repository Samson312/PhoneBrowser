namespace PhoneBrowser.Desktop.Errors;

public static partial class ErrorMessages
{
    public static UserMessage Pairing(PairingError error) => error switch
    {
        PairingError.Unreachable => new(
            "Telefon nie odpowiada",
            "Sprawdź, czy telefon i komputer są w tej samej sieci Wi-Fi, i spróbuj ponownie."),

        PairingError.Busy => new(
            "Do telefonu jest podłączone inne urządzenie",
            "W aplikacji na telefonie rozłącz się z obecnie połączonym urządzeniem i spróbuj ponownie."),

        PairingError.Rejected => new(
            "Telefon odrzucił prośbę o połączenie",
            "Jeśli to pomyłka, spróbuj jeszcze raz i na telefonie wybierz Akceptuj."),

        PairingError.TimedOut => new(
            "Telefon nie odpowiedział na czas",
            "Prośba o połączenie wygasła. Spróbuj jeszcze raz i od razu wybierz Akceptuj na telefonie"),

        PairingError.Unknown => new(
            "Coś poszło nie tak",
            "Uruchom aplikację jeszcze raz i spróbuj ponownie"),

        _ => throw new ArgumentOutOfRangeException(nameof(error), error, null)
    };
}

