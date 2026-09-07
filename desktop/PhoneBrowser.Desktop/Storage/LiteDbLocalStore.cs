namespace PhoneBrowser.Desktop.Storage;

using LiteDB;
using PhoneBrowser.Desktop.Models;
using System.IO;
using System.Runtime;

public sealed class LiteDbLocalStore: ILocalStore
{
    private readonly LiteDatabase db;

    private readonly ILiteCollection<SettingsProfile> profile;

    private readonly ILiteCollection<TrustedDevice> trustedDevices;

    public LiteDbLocalStore()
    {
        var path = Path.Combine(
            Environment.GetFolderPath(
                Environment.SpecialFolder.LocalApplicationData),
            "PhoneBrowser",
            "PhoneBrowser.db");

        Directory.CreateDirectory(Path.GetDirectoryName(path));

        db = new LiteDatabase(path);

        profile = db.GetCollection<SettingsProfile>("settings");
        trustedDevices = db.GetCollection<TrustedDevice>("trustedDevice");

        ConfigureIndexes();
    }


    private void ConfigureIndexes()
    {
        profile.EnsureIndex(
            x => x.Id,
            unique: true);
    }

    public bool HasSettingsProfile()
    {
        return profile.Exists(_ => true);
    }

    public SettingsProfile GetSettingsProfile()
    {
        var settings = profile.FindOne(_ => true);

        if (settings != null)
            return settings;

        settings = new SettingsProfile
        {
            Id = Guid.NewGuid().ToString(),
            DeviceName = "Komputer w Salonie"
        };

        profile.Insert(settings);

        return settings;
    }

    public void SaveSettingsProfile(SettingsProfile settings)
    {
        var existing = GetSettingsProfile();

        if (existing.Id != settings.Id)
            throw new InvalidOperationException(
                "Settings profile ID cannot be changed.");

        profile.Update(settings);
    }

    public IReadOnlyList<TrustedDevice> GetTrustedDevices()
    {
        return trustedDevices
            .Query()
            .OrderBy(x => x.Name)
            .ToList();
    }

    public TrustedDevice? GetTrustedDevice(string deviceId)
    {
        if (string.IsNullOrWhiteSpace(deviceId))
            return null;

        return trustedDevices.FindOne(
            x => x.Id == deviceId);
    }

    public void SaveTrustedDevice(TrustedDevice device)
    {
        if (string.IsNullOrWhiteSpace(device.Id))
            throw new ArgumentException(
                "DeviceId cannot be empty.",
                nameof(device));

        trustedDevices.Upsert(device);
    }

    public bool RemoveTrustedDevice(string deviceId)
    {
        if (string.IsNullOrWhiteSpace(deviceId))
            return false;

        return trustedDevices.Delete(deviceId);
    }
}

