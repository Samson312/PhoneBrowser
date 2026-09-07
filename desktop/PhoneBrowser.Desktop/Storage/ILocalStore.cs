using PhoneBrowser.Desktop.Models;

namespace PhoneBrowser.Desktop.Storage;

public interface ILocalStore
{
    public bool HasSettingsProfile();
    public SettingsProfile GetSettingsProfile();
    public void SaveSettingsProfile(SettingsProfile profile);

    IReadOnlyList<TrustedDevice> GetTrustedDevices();
    TrustedDevice? GetTrustedDevice(string deviceId);
    void SaveTrustedDevice(TrustedDevice device);
    bool RemoveTrustedDevice(string deviceId);
}

