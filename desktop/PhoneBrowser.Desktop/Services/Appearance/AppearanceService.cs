namespace PhoneBrowser.Desktop.Services.Appearance;

using Microsoft.Extensions.Logging;
using PhoneBrowser.Desktop.Models;
using System.IO;
using System.Windows;

public class AppearanceService : IAppearanceService
{
    private static readonly string[] TextSizeKeys =
    [
        "FontSizeTitle",
        "FontSizeHeading",
        "FontSizeBody",
        "FontSizeSecondary"
    ];

    private readonly Dictionary<string, double> BaseTextSizes = new();

    private readonly double BaseContentMaxWidth;

    private readonly ILogger<AppearanceService> logger;

    private ResourceDictionary? currentTheme;
    private ColorTheme? appliedTheme;

    public AppearanceService(ILogger<AppearanceService> logger)
    {
        this.logger = logger;

        BaseContentMaxWidth = (double)Application.Current.Resources["ContentMaxWidth"];

        foreach (var key in TextSizeKeys)
        {
            if (Application.Current.Resources[key] is double size)
                BaseTextSizes[key] = size;
        }
    }

    public void ApplyAll(SettingsProfile settings)
    {
        ApplyTextScale(settings.TextScale);
        ApplyTheme(settings.ColorTheme);
    }

    public double ApplyTextScale(double requested)
    {
        var scale = AppearanceLimits.Clamp(requested);

        foreach (var (key, size) in BaseTextSizes)
            Application.Current.Resources[key] = Math.Round(size * scale);

        ApplyControlScale(requested);

        return scale;
    }

    public double ApplyControlScale(double requested)
    {
        var scale = AppearanceLimits.Clamp(requested);
        Application.Current.Resources["ContentMaxWidth"] = Math.Round(BaseContentMaxWidth * scale);
        return scale;
    }

    public void ApplyTheme(ColorTheme theme)
    {
        if (appliedTheme == theme) return;

        ResourceDictionary next;

        try
        {
            next = new ResourceDictionary
            {
                Source = new Uri($"pack://application:,,,/Resources/Themes/{theme}.xaml", UriKind.Absolute)
            };
        }
        catch (IOException ex)
        {
            logger.LogError(ex, "Failed to load theme dictionary for {Theme}", theme);
            return;
        }

        var merged = Application.Current.Resources.MergedDictionaries;

        merged.Add(next);

        if (currentTheme != null)
            merged.Remove(currentTheme);

        currentTheme = next;
        appliedTheme = theme;

        logger.LogInformation("Applied theme {Theme}", theme);
    }
}