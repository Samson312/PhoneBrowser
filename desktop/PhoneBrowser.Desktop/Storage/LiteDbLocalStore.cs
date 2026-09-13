namespace PhoneBrowser.Desktop.Storage;

using Microsoft.Extensions.Logging;
using LiteDB;
using PhoneBrowser.Desktop.Models;
using Serilog;
using System.IO;
using System.Runtime;

public sealed class LiteDbLocalStore: ILocalStore
{
    private readonly ILogger<LiteDbLocalStore> logger;

    private readonly LiteDatabase db;

    private readonly ILiteCollection<SettingsProfile> profile;

    private readonly ILiteCollection<TrustedDevice> trustedDevices;

    public LiteDbLocalStore(ILogger<LiteDbLocalStore> logger)
    {
        this.logger = logger;

        var path = Path.Combine(
            Environment.GetFolderPath(
                Environment.SpecialFolder.LocalApplicationData),
            "PhoneBrowser",
            "PhoneBrowser.db");

        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            db = new LiteDatabase(path);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or LiteException)
        {
            logger.LogCritical(ex, "Failed to initialize local database at {Path}", path);
            throw;
        }

        profile = db.GetCollection<SettingsProfile>("settings");
        trustedDevices = db.GetCollection<TrustedDevice>("trustedDevice");
        ConfigureIndexes();
    }


    private void ConfigureIndexes()
    {
        try
        {
            profile.EnsureIndex(x => x.Id, unique: true);
        }
        catch (LiteException ex)
        {
            logger.LogError(ex, "Failed to create index on settings collection");
        }
    }

    public bool HasSettingsProfile()
    {
        try
        {
            return profile.Exists(_ => true);
        }
        catch (LiteException ex)
        {
            logger.LogError(ex, "Failed to check for existing settings profile");
            throw;
        }
    }

    public SettingsProfile GetSettingsProfile()
    {
        try
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
            logger.LogInformation("Created default settings profile {ProfileId}", settings.Id);

            return settings;
        }
        catch (LiteException ex)
        {
            logger.LogError(ex, "Failed to read or create settings profile");
            throw;
        }
    }

    public void SaveSettingsProfile(SettingsProfile settings)
    {
        var existing = GetSettingsProfile();

        if (existing.Id != settings.Id)
            throw new InvalidOperationException("Settings profile ID cannot be changed.");

        try
        {
            profile.Update(settings);
        }
        catch (LiteException ex)
        {
            logger.LogError(ex, "Failed to update settings profile {ProfileId}", settings.Id);
            throw;
        }
    }

    public IReadOnlyList<TrustedDevice> GetTrustedDevices()
    {
        try
        {
            return trustedDevices.Query().OrderBy(x => x.Name).ToList();
        }
        catch (LiteException ex)
        {
            logger.LogError(ex, "Failed to read trusted devices from local database");
            return Array.Empty<TrustedDevice>();
        }
    }

    public TrustedDevice? GetTrustedDevice(string deviceId)
    {
        if (string.IsNullOrWhiteSpace(deviceId))
            return null;

        try
        {
            return trustedDevices.FindOne(x => x.Id == deviceId);
        }
        catch (LiteException ex)
        {
            logger.LogError(ex, "Failed to read trusted device {DeviceId}", deviceId);
            return null;
        }
    }

    public void SaveTrustedDevice(TrustedDevice device)
    {
        if (string.IsNullOrWhiteSpace(device.Id))
            throw new ArgumentException("DeviceId cannot be empty.", nameof(device));

        try
        {
            trustedDevices.Upsert(device);
            logger.LogInformation("Saved trusted device {DeviceId} ({DeviceName})", device.Id, device.Name);
        }
        catch (LiteException ex)
        {
            logger.LogError(ex, "Failed to save trusted device {DeviceId}", device.Id);
            throw;
        }
    }

    public bool RemoveTrustedDevice(string deviceId)
    {
        if (string.IsNullOrWhiteSpace(deviceId)) return false;

        try
        {
            var removed = trustedDevices.Delete(deviceId);
            logger.LogInformation("Removed trusted device {DeviceId}: {Removed}", deviceId, removed);
            return removed;
        }
        catch (LiteException ex)
        {
            logger.LogError(ex, "Failed to remove trusted device {DeviceId}", deviceId);
            return false;
        }
    }
}

