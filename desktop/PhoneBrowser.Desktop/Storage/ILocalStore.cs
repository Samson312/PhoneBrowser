using PhoneBrowser.Desktop.Models;

namespace PhoneBrowser.Desktop.Storage;

public interface ILocalStore
{
    public bool HasSettingsProfile();
    public SettingsProfile GetSettingsProfile();
    public void SaveSettingsProfile(SettingsProfile profile);
}

