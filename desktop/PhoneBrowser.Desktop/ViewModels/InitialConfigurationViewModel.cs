namespace PhoneBrowser.Desktop.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using PhoneBrowser.Desktop.Errors;
using PhoneBrowser.Desktop.Models;
using PhoneBrowser.Desktop.Services.Navigation;
using PhoneBrowser.Desktop.Storage;
using System.IO;
using LiteDB;

public partial class InitialConfigurationViewModel : ViewModelBase
{
    private readonly INavigationService navigation;
    private readonly ILocalStore store;
    private readonly ILogger<InitialConfigurationViewModel> logger;

    private SettingsProfile settings;

    [ObservableProperty]
    private string deviceName;

    [ObservableProperty]
    private UserMessage? errorMessage = null;

    public InitialConfigurationViewModel(
        INavigationService navigation,
        ILocalStore store,
        ILogger<InitialConfigurationViewModel> logger)
    {
        this.navigation = navigation;
        this.store = store;
        this.logger = logger;

        settings = store.GetSettingsProfile();
        DeviceName = settings.DeviceName;
    }

    partial void OnDeviceNameChanged(string value) => ErrorMessage = null;

    private InitialConfigError SaveData()
    {
        if (string.IsNullOrWhiteSpace(DeviceName))
            return InitialConfigError.EmptyName;

        try
        {
            settings.DeviceName = DeviceName.Trim();

            store.SaveSettingsProfile(settings);

            return InitialConfigError.None;
        }
        catch (Exception ex) when (ex is InvalidOperationException or LiteException or IOException)
        {
            logger.LogError(ex, "Failed to save initial settings profile for device name {DeviceName}", DeviceName);
            return InitialConfigError.SaveFailed;
        }
    }

    [RelayCommand]
    private void Next()
    {
        var error = SaveData();

        if (error != InitialConfigError.None)
        {
            ErrorMessage = ErrorMessages.InitialConfig(error);
            return;
        }

        navigation.NavigateTo<HomeViewModel>();
    }
}
