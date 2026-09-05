namespace PhoneBrowser.Desktop.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PhoneBrowser.Desktop.Models;
using PhoneBrowser.Desktop.Services.Navigation;
using PhoneBrowser.Desktop.Storage;

public partial class InitialConfigurationViewModel: ViewModelBase
{
    private readonly INavigationService navigation;
    private readonly ILocalStore store;

    private SettingsProfile settings;

    [ObservableProperty]
    public string deviceName;

    public InitialConfigurationViewModel(
        INavigationService navigation,
        ILocalStore store)
    {
        this.navigation = navigation;
        this.store = store;

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
        catch(InvalidOperationException ex)
        {
            return false;
        }
    }


    [RelayCommand]
    private void Next() 
    {
        if (SaveData())
        {
            navigation.NavigateTo<PairingViewModel>();
        }
    }
}

