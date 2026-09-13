namespace PhoneBrowser.Desktop;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PhoneBrowser.Desktop.Services.Discovery;
using PhoneBrowser.Desktop.Services.Navigation;
using PhoneBrowser.Desktop.Services.Pairing;
using PhoneBrowser.Desktop.Services.Photo;
using PhoneBrowser.Desktop.Storage;
using PhoneBrowser.Desktop.ViewModels;
using PhoneBrowser.Desktop.Views;
using Serilog;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Markup;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    public static IServiceProvider Services { get; private set; } = null;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var logPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "PhoneBrowser", "logs", "log-.txt");

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Debug()                     
            .WriteTo.File(
                logPath,
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 14,       
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}")
            .Enrich.FromLogContext()
            .CreateLogger();


        var culture = new CultureInfo("pl-PL");
        Thread.CurrentThread.CurrentCulture = culture;
        Thread.CurrentThread.CurrentUICulture = culture;


        FrameworkElement.LanguageProperty.OverrideMetadata(
            typeof(FrameworkElement),
            new FrameworkPropertyMetadata(
                XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag)));


        var services = new ServiceCollection();

        services.AddLogging(builder =>
        {
            builder.ClearProviders();
            builder.AddSerilog(dispose: true); 
        });

        services.AddSingleton<ILocalStore, LiteDbLocalStore>();
        services.AddSingleton<INavigationService, NavigationService>();
        services.AddSingleton<MainViewModel>();
        services.AddSingleton<PairedDeviceService>();
        services.AddHttpClient();

        services.AddTransient<IUdpDiscoveryService, UdpDiscoveryService>();
        services.AddTransient<IPairingService, PairingService>();
        services.AddTransient<IPhotoSourceService, HttpPhotoSourceService>();

        services.AddTransient<InitialConfigurationViewModel>();
        services.AddTransient<HomeViewModel>();
        services.AddTransient<PairingViewModel>();
        services.AddTransient<PhotoGalleryViewModel>();

        Services = services.BuildServiceProvider();

        var window = new MainWindow
        {
            DataContext = Services.GetRequiredService<MainViewModel>()
        };
        window.Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        Log.CloseAndFlush();  
        base.OnExit(e);
    }
}

