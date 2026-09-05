namespace PhoneBrowser.Desktop.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using PhoneBrowser.Desktop.Services.Navigation;

public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private ViewModelBase? currentViewModel;

    public MainViewModel(INavigationService navigation)
    {
        navigation.CurrentViewModelChanged += vm => CurrentViewModel = vm;
        navigation.NavigateTo<PairingViewModel>(); 
    }
}
