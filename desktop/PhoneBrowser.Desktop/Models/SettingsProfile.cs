namespace PhoneBrowser.Desktop.Models;

public record SettingsProfile
{
    public string Id { get; init; }

    public string DeviceName { get; set; } = string.Empty;

    public double TextScale { get; set; } = 1.5;
    public double ControlScale { get; set; } = 1.5;
    public ColorTheme ColorTheme { get; set; } = ColorTheme.Dark;
}


