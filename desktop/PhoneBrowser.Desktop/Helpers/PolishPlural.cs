namespace PhoneBrowser.Desktop.Helpers;

public static class PolishPlural
{
    public static string Photos(int n)
    {
        if (n == 1) return "zdjęcie";
        var last = n % 10;
        var lastTwo = n % 100;
        return last is >= 2 and <= 4 && lastTwo is not (>= 12 and <= 14)
            ? $"{n} zdjęcia"
            : $"{n} zdjęć";
    }
}

