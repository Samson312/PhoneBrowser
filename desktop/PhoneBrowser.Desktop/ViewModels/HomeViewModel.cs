namespace PhoneBrowser.Desktop.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PhoneBrowser.Desktop.Services.Navigation;
using PhoneBrowser.Desktop.Models;

public partial class HomeViewModel : ViewModelBase
{
    private readonly INavigationService navigation;

    [ObservableProperty]
    private ConnectionMode mode = ConnectionMode.Wireless;


    public HomeViewModel(
        INavigationService navigation)
    {
        this.navigation = navigation;
    }

    [RelayCommand]
    private void SelectConnectionMode(ConnectionMode mode)
    {
        Mode = mode;
    }


    [RelayCommand]
    private void Next()
    {
        switch (Mode)
        {
            case ConnectionMode.Wireless:
                navigation.NavigateTo<PairingViewModel>();
                break;
            case ConnectionMode.Usb:
                throw new NotImplementedException();
        }
    }
}

