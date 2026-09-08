namespace PhoneBrowser.Desktop.ViewModels;

using CommunityToolkit.Mvvm.Input;
using PhoneBrowser.Desktop.Models;
using PhoneBrowser.Desktop.Services.Discovery;
using PhoneBrowser.Desktop.Services.Navigation;
using PhoneBrowser.Desktop.Services.Pairing;
using PhoneBrowser.Desktop.Storage;
using System;
using System.Collections.ObjectModel;
using System.Windows;

public partial class PairingViewModel : ViewModelBase
{
	private readonly IUdpDiscoveryService discoveryService;

	private readonly IPairingService pairingService;

    private readonly INavigationService navigation;

    private readonly ILocalStore store;

    private readonly PairedDeviceService pairedDevice;

    private readonly HashSet<string> pairingInFlight = new();

    private readonly CancellationTokenSource cts = new();

	public ObservableCollection<DiscoveredDevice> DevicesDiscovered { get; } = new();

    public PairingViewModel(
        IUdpDiscoveryService discoveryService,
        IPairingService pairingService,
        INavigationService navigation,
        ILocalStore store,
        PairedDeviceService pairedDevice)
	{
        this.discoveryService = discoveryService;
        this.pairingService = pairingService;
        this.navigation = navigation;
        this.store = store;
        this.pairedDevice = pairedDevice;

		discoveryService.DeviceDiscovered += OnDeviceDiscovered;


        discoveryService.StartAsync(cts.Token);
    }

    public override void Dispose()
    {
		discoveryService.DeviceDiscovered -= OnDeviceDiscovered;
        cts.Cancel();
        discoveryService.Dispose();
        cts.Dispose();
    }

	private void OnDeviceDiscovered(DiscoveredDevice device)
	{
        Application.Current.Dispatcher.Invoke(() =>
        {
            if (!DevicesDiscovered.Any(d => d.DeviceId == device.DeviceId))
                DevicesDiscovered.Add(device);
        });
    }

    [RelayCommand]
    private async Task PairAsync(DiscoveredDevice device)
    {
        if (pairedDevice.IsPaired) return;

        if (!pairingInFlight.Add(device.DeviceId)) return;
        

        var token = await pairingService.PairAsync(device, cts.Token);

        if (token != null)
        {
            pairedDevice?.SetDevice(device, token);
            NextCommand.NotifyCanExecuteChanged();
            AddTrustedDevice(device, token);
            discoveryService.Stop();
        }
        
        pairingInFlight.Remove(device.DeviceId);
    }

    private void AddTrustedDevice(DiscoveredDevice device, string token)
    {
        var trustedDevice = new TrustedDevice
        {
            Id = device.DeviceId,
            Name = device.DeviceName,
            PairingToken = token,
            LastKnownIpAddress = device.IpAddress.ToString(),
            LastKnownPort = device.HttpPort,
            LastConnectedAt = DateTime.UtcNow
        };

        try
        {
            store.SaveTrustedDevice(trustedDevice);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex);
        }
    }

    private bool CanGoNext => pairedDevice.IsPaired;

    [RelayCommand]
    private void Back() => navigation.GoBack();

    [RelayCommand(CanExecute = nameof(CanGoNext))]
    private void Next() { }
}
