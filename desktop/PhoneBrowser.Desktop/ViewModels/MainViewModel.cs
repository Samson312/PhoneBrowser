namespace PhoneBrowser.Desktop.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using PhoneBrowser.Desktop.Services.Navigation;
using PhoneBrowser.Desktop.Storage;

public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty]
    private ViewModelBase? currentViewModel;

    public MainViewModel(
        INavigationService navigation,
        ILocalStore store)
    {
        navigation.CurrentViewModelChanged += vm => CurrentViewModel = vm;


        if (store.HasSettingsProfile())
        {
            navigation.NavigateTo<PairingViewModel>();
        }
        else
        {
            navigation.NavigateTo<InitialConfigurationViewModel>();
        }
    }
}
