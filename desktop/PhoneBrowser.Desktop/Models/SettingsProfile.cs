namespace PhoneBrowser.Desktop.Models;

public record SettingsProfile
{
    public string Id { get; init; }

    public string DeviceName { get; set; } = string.Empty;
}


