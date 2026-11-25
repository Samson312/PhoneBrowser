namespace PhoneBrowser.Desktop.Services.Appearance;

public static class AppearanceLimits
{
    public const double MinScale = 1.0;
    public const double MaxScale = 2.0;
    public const double Step = 0.1;

    public static double Clamp(double scale) =>
        Math.Round(Math.Clamp(scale, MinScale, MaxScale), 1);
}