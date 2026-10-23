namespace PhoneBrowser.Desktop.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using PhoneBrowser.Desktop.Errors;
using PhoneBrowser.Desktop.Models;
using PhoneBrowser.Desktop.Services.Connection;
using PhoneBrowser.Desktop.Services.Discovery;
using PhoneBrowser.Desktop.Services.Navigation;
using PhoneBrowser.Desktop.Services.Pairing;
using PhoneBrowser.Desktop.Storage;
using System;
using System.Windows.Threading;

public partial class PairingViewModel : ViewModelBase
{
	private readonly IUdpDiscoveryService discoveryService;
	private readonly IPairingService pairingService;
    private readonly IConnectionService connectionService;
    private readonly INavigationService navigation;
    private readonly ILocalStore store;
    private readonly ILogger<PairingViewModel> logger;
    private readonly PairedDevice pairedDevice;
    public DeviceListViewModel DeviceList { get; }
    private readonly CancellationTokenSource cts = new();

    private readonly DispatcherTimer availabilityTimer = new() { Interval =  TimeSpan.FromSeconds(1) };

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(PairCommand), nameof(ConnectCommand), nameof(DisconnectCommand))]
    private bool isBusy;
    
    [ObservableProperty]
    private UserMessage? errorMessage = null;

    public PairingViewModel(
        IUdpDiscoveryService discoveryService,
        IPairingService pairingService,
        IConnectionService connectionService,
        INavigationService navigation,
        ILocalStore store,
        ILogger<PairingViewModel> logger,
        PairedDevice pairedDevice)
	{
        this.discoveryService = discoveryService;
        this.pairingService = pairingService;
        this.connectionService = connectionService;
        this.navigation = navigation;
        this.store = store;
        this.logger = logger;
        this.pairedDevice = pairedDevice;

        DeviceList = new DeviceListViewModel(store, pairedDevice.CurrentDevice?.DeviceId);

        availabilityTimer.Tick += DeviceList.RefreshAvailability;
        availabilityTimer.Start();

		discoveryService.DeviceDiscovered += OnDeviceDiscovered;

        _ = StartDiscoveryAsync();
    }

    public override void Dispose()
    {
        availabilityTimer.Tick -= DeviceList.RefreshAvailability;
        availabilityTimer.Stop();
        discoveryService.DeviceDiscovered -= OnDeviceDiscovered;
        cts.Cancel();
        discoveryService.Dispose();
        cts.Dispose();
    }

    private bool CanGoNext => pairedDevice.IsPaired;

    private bool CanOperate => !pairedDevice.IsPaired && !IsBusy;

    private bool CanDisconnect => pairedDevice.IsPaired && !IsBusy;

    private async Task StartDiscoveryAsync()
    {
        try
        {
            await discoveryService.StartAsync(cts.Token);
        }
        catch (OperationCanceledException){ }
        catch (Exception ex)
        {
            logger.LogError(ex, "Discovery service failed to start on pairing screen");
        }
    }

    private TrustedDevice? AddTrustedDevice(DiscoveredDevice device, string token)
    {
        var trustedDevice = new TrustedDevice
        {
            DeviceId = device.DeviceId,
            DeviceName = device.DeviceName,
            AuthToken = token,
            LastKnownIpAddress = device.IpAddress.ToString(),
            LastKnownPort = device.HttpPort,
            LastConnectedAt = DateTime.UtcNow
        };

        try
        {
            store.SaveTrustedDevice(trustedDevice);
            logger.LogInformation("Paired with device {DeviceName} ({DeviceId})", device.DeviceName, device.DeviceId);
            return trustedDevice;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to persist trusted device {DeviceId} after successful pairing", device.DeviceId);
            return null;
        }
    }

    private void UpdateLastConnected(TrustedDevice device)
    {
        var updatedDevice = device with
        {
            LastConnectedAt = DateTime.UtcNow
        };

        PersistAndRefresh(updatedDevice);
    }

    private void OnDeviceDiscovered(DiscoveredDevice device)
    {
        DeviceList.OnDeviceDiscovered(device);

        var trustedDevice = DeviceList.GetTrustedDevice(device.DeviceId);

        if (trustedDevice == null) return;

        var updatedDevice = trustedDevice with
        {
            DeviceName = device.DeviceName,
            LastKnownIpAddress = device.IpAddress.ToString(),
            LastKnownPort = device.HttpPort
        };

        if (updatedDevice == trustedDevice) return;

        PersistAndRefresh(updatedDevice);
    }

    private void PersistAndRefresh(TrustedDevice updatedDevice)
    {
        try
        {
            if (!store.UpdateTrustedDevice(updatedDevice))
            {
                logger.LogWarning("Device not found {DeviceId}", updatedDevice.DeviceId);
            }
            else
            {
                DeviceList.UpdateList(updatedDevice);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to update device: {DeviceId}", updatedDevice.DeviceId);
        }
    }

    private async Task TryConnectAsync(TrustedDevice device)
    {
        var result = await connectionService.ConnectAsync(device, cts.Token);

        if (result == ConnectError.None)
        {
            UpdateLastConnected(device);
            DeviceList.SetConnected(device);
            NextCommand.NotifyCanExecuteChanged();
        }
        else
        {
            ErrorMessage = ErrorMessages.Connection(result);
        }
    }

    [RelayCommand(CanExecute = nameof(CanOperate))]
    private async Task PairAsync(DiscoveredDevice device)
    {
        if (pairedDevice.IsPaired) return;

        try
        {
            IsBusy = true;
            ErrorMessage = null;

            var token = await pairingService.PairAsync(device, cts.Token);
            if (token == null) return;
            
            var trustedDevice = AddTrustedDevice(device, token);
            if (trustedDevice == null) return;

            DeviceList.UpdateList(trustedDevice);

            await TryConnectAsync(trustedDevice);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanOperate))]
    private async Task ConnectAsync(TrustedDevice device)
    {
        if (pairedDevice.IsPaired) return;

        try
        {
            IsBusy = true;
            ErrorMessage = null;
            await TryConnectAsync(device);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanDisconnect))]
    private async Task DisconnectAsync(TrustedDevice device)
    {
        if(pairedDevice.CurrentDevice?.DeviceId != device.DeviceId) return;

        try
        {
            IsBusy = true;
            await connectionService.DisconnectAsync(device, cts.Token);
            DeviceList.SetDisconnected(device);
            NextCommand.NotifyCanExecuteChanged();
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void Back() => navigation.GoBack();

    [RelayCommand(CanExecute = nameof(CanGoNext))]
    private void Next() 
    {
        discoveryService.Stop();
        availabilityTimer.Stop();
        navigation.NavigateTo<PhotoGalleryViewModel>();
    }
}
