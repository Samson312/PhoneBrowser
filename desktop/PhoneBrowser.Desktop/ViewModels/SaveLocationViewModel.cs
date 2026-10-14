namespace PhoneBrowser.Desktop.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using PhoneBrowser.Desktop.Services.Navigation;
using PhoneBrowser.Desktop.Helpers;
using PhoneBrowser.Desktop.Models;
using PhoneBrowser.Desktop.Errors;
using System;
using System.IO;

public partial class SaveLocationViewModel : ViewModelBase
{
    private readonly INavigationService navigation;

    private readonly ILogger<SaveLocationViewModel> logger;

    private readonly TransferDraft transferDraft;

    public string Title => $"Gdzie zapisać wybrane {PolishPlural.Photos(transferDraft.Photos.Count)}?";

    public string DirectoryName
    {
        get
        {
            var trimmed = Path.TrimEndingDirectorySeparator(DirectoryPath);
            var name = Path.GetFileName(trimmed);
            return string.IsNullOrEmpty(name) ? DirectoryPath : name;
        }
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DirectoryName))]
    private string directoryPath = "";

    [ObservableProperty] 
    private UserMessage? errorMessage = null;

    public SaveLocationViewModel(
        TransferDraft transferDraft,
        INavigationService navigation,
        ILogger<SaveLocationViewModel> logger)
    {
        this.transferDraft = transferDraft;
        this.navigation = navigation;
        this.logger = logger;


        DirectoryPath = transferDraft.TargetDirectory ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),
            "Zdjęcia z telefonu");
    }

    private SaveLocationError Validate(string path, long requiredBytes) 
    {
        try
        {
            Directory.CreateDirectory(path);

            var probe = Path.Combine(path, $".probe-{Guid.NewGuid():N}");
            File.WriteAllBytes(probe, []);
            File.Delete(probe);

            var root = Path.GetPathRoot(Path.GetFullPath(path));
            if (root is { Length: > 0 } && !root.StartsWith(@"\\"))
            {
                if (new DriveInfo(root).AvailableFreeSpace < requiredBytes)
                {
                    logger.LogWarning("Not enough space on {Root}: need {Required} B", root, requiredBytes);
                    return SaveLocationError.NotEnoughSpace;
                }
            }
            return SaveLocationError.None;
        }
        catch (UnauthorizedAccessException ex)
        {
            logger.LogWarning(ex, "No write permission for {Path}", path);
            return SaveLocationError.NoPermission;
        }
        catch (Exception ex) when (ex is IOException or ArgumentException or NotSupportedException)
        {
            logger.LogWarning(ex, "Target directory unavailable: {Path}", path);
            return SaveLocationError.Unavailable;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error validating {Path}", path);
            return SaveLocationError.Unknown;
        }
    }

    [RelayCommand]
    private void ChangeDirectory()
    {
        var dlg = new OpenFolderDialog { InitialDirectory = DirectoryPath };
        if (dlg.ShowDialog() == true)
        {
            DirectoryPath = dlg.FolderName;
            ErrorMessage = null;  
        }
    }

    [RelayCommand]
    private void Back() => navigation.GoBack();

    [RelayCommand]
    private void Next()
    {
        ErrorMessage = null;

        var transferDataSize = transferDraft.TotalBytes;

        var error = Validate(DirectoryPath, transferDataSize);

        if(error != SaveLocationError.None)
        {
            ErrorMessage = ErrorMessages.SaveLocation(error, transferDataSize);
            logger.LogDebug("Validation failed: {Error}", error);
            return;
        }

        transferDraft.SetTargetDirectory(DirectoryPath);
        logger.LogInformation("Save location confirmed: {Path}, {Count} photos, {Bytes} B",
            DirectoryPath, transferDraft.Photos.Count, transferDataSize);
    }
}

