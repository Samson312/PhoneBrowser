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

        navigation.NavigateTo<InitialConfigurationViewModel>();
        return;


        if (store.HasSettingsProfile())
        {
            navigation.NavigateTo<HomeViewModel>();
        }
        else
        {
            navigation.NavigateTo<InitialConfigurationViewModel>();
        }
    }
}
