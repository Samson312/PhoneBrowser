namespace PhoneBrowser.Desktop.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;
using PhoneBrowser.Desktop.Services.Navigation;
using PhoneBrowser.Desktop.Storage;

public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty]
    private ViewModelBase? currentViewModel;

    public MainViewModel(
        INavigationService navigation,
        ILocalStore store,
        ILogger<MainViewModel> logger)
    {
        navigation.CurrentViewModelChanged += vm => CurrentViewModel = vm;

        bool hasProfile;
        try
        {
            hasProfile = store.HasSettingsProfile();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to check for existing settings profile, defaulting to initial configuration");
            hasProfile = false;
        }

        if (hasProfile)
        {
            navigation.NavigateTo<HomeViewModel>();
        }
        else
        {
            navigation.NavigateTo<InitialConfigurationViewModel>();
        }
    }
}
