namespace PhoneBrowser.Desktop.Storage;

using LiteDB;
using PhoneBrowser.Desktop.Models;
using System.IO;
using System.Runtime;

public sealed class LiteDbLocalStore: ILocalStore
{
    private readonly LiteDatabase db;

    private readonly ILiteCollection<SettingsProfile> profile;

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

}

