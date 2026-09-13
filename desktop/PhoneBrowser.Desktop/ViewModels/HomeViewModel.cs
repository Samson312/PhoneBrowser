namespace PhoneBrowser.Desktop.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PhoneBrowser.Desktop.Services.Navigation;
using PhoneBrowser.Desktop.Models;
using Microsoft.Extensions.Logging;

public partial class HomeViewModel : ViewModelBase
{
    private readonly INavigationService navigation;
    private readonly ILogger<HomeViewModel> logger;

    [ObservableProperty]
    private ConnectionMode mode = ConnectionMode.Wireless;


    public HomeViewModel(
        INavigationService navigation,
        ILogger<HomeViewModel> logger)
    {
        this.navigation = navigation;
        this.logger = logger;
    }

    [RelayCommand]
    private void SelectConnectionMode(ConnectionMode mode)
    {
        Mode = mode;
        logger.LogDebug("Connection mode selected: {Mode}", mode);
    }


    [RelayCommand]
    private void Next()
    {
        logger.LogInformation("Starting new transfer in mode {Mode}", Mode);

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

