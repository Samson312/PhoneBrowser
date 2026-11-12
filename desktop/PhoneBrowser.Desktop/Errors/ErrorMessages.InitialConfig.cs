namespace PhoneBrowser.Desktop.Errors;

public static partial class ErrorMessages
{
    public static UserMessage? InitialConfig(InitialConfigError error) => error switch
    {
        InitialConfigError.EmptyName => new(
            "Wpisz nazwę komputera",
            "Pole nie może być puste. Wpisz na przykład „Komputer w salonie”."),

        InitialConfigError.SaveFailed => new(
            "Nie udało się zapisać nazwy",
            "Kliknij „Zapisz i zacznij” jeszcze raz. Jeśli to nie pomoże, zamknij aplikację i uruchom ją ponownie."),

        InitialConfigError.None => null,

        _ => throw new ArgumentOutOfRangeException(nameof(error), error, null)
    };
}
