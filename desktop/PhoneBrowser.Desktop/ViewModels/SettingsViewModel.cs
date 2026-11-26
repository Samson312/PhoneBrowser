namespace PhoneBrowser.Desktop.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using PhoneBrowser.Desktop.Errors;
using PhoneBrowser.Desktop.Models;
using PhoneBrowser.Desktop.Services.Navigation;
using PhoneBrowser.Desktop.Storage;
using PhoneBrowser.Desktop.Services.Appearance;
using System.IO;
using LiteDB;

public partial class SettingsViewModel : ViewModelBase
{
    private readonly INavigationService navigation;
    private readonly ILocalStore store;
    private readonly IAppearanceService appearance;
    private readonly ILogger<SettingsViewModel> logger;

    private readonly SettingsProfile savedSettings;

    public string TextScaleLabel => $"{Math.Round(TextScale * 100)}%";

    private bool CanIncreaseText => TextScale < AppearanceLimits.MaxScale;
    private bool CanDecreaseText => TextScale > AppearanceLimits.MinScale;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TextScaleLabel))]
    [NotifyCanExecuteChangedFor(nameof(IncreaseTextCommand), nameof(DecreaseTextCommand))]
    private double textScale;

    [ObservableProperty]
    private ColorTheme colorTheme;

    [ObservableProperty]
    private string deviceName;

    [ObservableProperty]
    private UserMessage? errorMessage = null;

    public SettingsViewModel(
        INavigationService navigation,
        ILocalStore store,
        IAppearanceService appearance,
        ILogger<SettingsViewModel> logger)
    {
        this.navigation = navigation;
        this.store = store;
        this.appearance = appearance;
        this.logger = logger;

        savedSettings = store.GetSettingsProfile();

        textScale = savedSettings.TextScale;
        colorTheme = savedSettings.ColorTheme;
        deviceName = savedSettings.DeviceName;
    }

    partial void OnDeviceNameChanged(string value) => ErrorMessage = null;

    partial void OnColorThemeChanged(ColorTheme value) => appearance.ApplyTheme(value);

    private void ChangeTextScale(double delta)
    {
        TextScale = appearance.ApplyTextScale(TextScale + delta);
        appearance.ApplyControlScale(TextScale + delta);
    }
        

    private InitialConfigError SaveData()
    {
        if (string.IsNullOrWhiteSpace(DeviceName))
            return InitialConfigError.EmptyName;

        try
        {
            store.SaveSettingsProfile(savedSettings with
            {
                DeviceName = DeviceName.Trim(),
                TextScale = TextScale,
                ColorTheme = ColorTheme
            });

            return InitialConfigError.None;
        }
        catch (Exception ex) when (ex is InvalidOperationException or LiteException or IOException)
        {
            logger.LogError(ex, "Failed to save settings profile for device name {DeviceName}", DeviceName);
            return InitialConfigError.SaveFailed;
        }
    }

    [RelayCommand(CanExecute = nameof(CanIncreaseText))]
    private void IncreaseText() => ChangeTextScale(AppearanceLimits.Step);

    [RelayCommand(CanExecute = nameof(CanDecreaseText))]
    private void DecreaseText() => ChangeTextScale(-AppearanceLimits.Step);

    [RelayCommand]
    private void Back()
    {
        appearance.ApplyAll(savedSettings);
        navigation.GoBack();
    }

    [RelayCommand]
    private void Next()
    {
        var error = SaveData();

        if (error != InitialConfigError.None)
        {
            ErrorMessage = ErrorMessages.InitialConfig(error);
            return;
        }

        navigation.GoBack();
    }
}
