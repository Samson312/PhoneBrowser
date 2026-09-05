namespace PhoneBrowser.Desktop;

using Microsoft.Extensions.DependencyInjection;
using PhoneBrowser.Desktop.Services.Discovery;
using PhoneBrowser.Desktop.Services.Navigation;
using PhoneBrowser.Desktop.Services.Pairing;
using PhoneBrowser.Desktop.ViewModels;
using PhoneBrowser.Desktop.Views;
using System.Windows;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    public static IServiceProvider Services { get; private set; } = null;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var services = new ServiceCollection();

        services.AddSingleton<INavigationService, NavigationService>();
        services.AddSingleton<MainViewModel>();

        services.AddTransient<IUdpDiscoveryService, UdpDiscoveryService>();
        services.AddTransient<IPairingService, PairingService>();

        services.AddTransient<PairingViewModel>();

        Services = services.BuildServiceProvider();

        var window = new MainWindow
        {
            DataContext = Services.GetRequiredService<MainViewModel>()
        };
        window.Show();
    }
}

