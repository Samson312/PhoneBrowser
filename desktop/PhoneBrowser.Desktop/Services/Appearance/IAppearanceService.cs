namespace PhoneBrowser.Desktop.Services.Appearance;

using PhoneBrowser.Desktop.Models;

public interface IAppearanceService
{
    void ApplyAll(SettingsProfile settings);
    double ApplyTextScale(double requested);
    double ApplyControlScale(double requested);
    void ApplyTheme(ColorTheme theme);
}