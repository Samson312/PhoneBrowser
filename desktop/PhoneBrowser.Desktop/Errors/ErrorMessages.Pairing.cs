namespace PhoneBrowser.Desktop.Errors;

public static partial class ErrorMessages
{
    public static UserMessage Pairing(PairingError error) => error switch
    {
        PairingError.Unreachable => new(
            "Telefon nie odpowiada",
            "Uruchom aplikację na telefonie i połącz go z tą samą siecią Wi-Fi co komputer."),

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

