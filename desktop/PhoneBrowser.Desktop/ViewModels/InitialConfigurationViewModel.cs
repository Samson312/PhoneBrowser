namespace PhoneBrowser.Desktop.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using PhoneBrowser.Desktop.Models;
using PhoneBrowser.Desktop.Services.Navigation;
using PhoneBrowser.Desktop.Storage;
using System.IO;
using LiteDB;

public partial class InitialConfigurationViewModel: ViewModelBase
{
    private readonly INavigationService navigation;
    private readonly ILocalStore store;
    private readonly ILogger<InitialConfigurationViewModel> logger;

    private SettingsProfile settings;

    [ObservableProperty]
    public string deviceName;

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

    private bool SaveData()
    {
        try
        {
            settings.DeviceName = DeviceName;

            store.SaveSettingsProfile(settings);

            return true;
        }
        catch(Exception ex) when (ex is InvalidOperationException or LiteException or IOException)
        {
            logger.LogError(ex, "Failed to save initial settings profile for device name {DeviceName}", DeviceName);
            return false;
        }
    }


    [RelayCommand]
    private void Next() 
    {
        if (SaveData())
        {
            navigation.NavigateTo<HomeViewModel>();
        }
    }
}

