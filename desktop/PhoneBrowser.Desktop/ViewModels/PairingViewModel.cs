namespace PhoneBrowser.Desktop.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PhoneBrowser.Desktop.Models;
using PhoneBrowser.Desktop.Services.Discovery;
using PhoneBrowser.Desktop.Services.Navigation;
using PhoneBrowser.Desktop.Services.Pairing;
using System;
using System.Collections.ObjectModel;
using System.Windows;

public partial class PairingViewModel : ViewModelBase
{
	private readonly IUdpDiscoveryService discoveryService;

	private readonly IPairingService pairingService;

    private readonly INavigationService navigation;

    private readonly HashSet<string> pairingInFlight = new();

    private readonly CancellationTokenSource cts = new();

	public ObservableCollection<DiscoveredDevice> DevicesDiscovered { get; } = new();

    [ObservableProperty]
    private DiscoveredDevice? pairedDevice;

    public bool IsPaired => PairedDevice != null;

    public PairingViewModel(
        IUdpDiscoveryService discoveryService,
        IPairingService pairingService,
        INavigationService navigation)
	{
        this.discoveryService = discoveryService;
        this.pairingService = pairingService;
        this.navigation = navigation;

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
        if (!DevicesDiscovered.Any(d => d.DeviceId == device.DeviceId))
            DevicesDiscovered.Add(device);
    }

    [RelayCommand]
    private async Task PairAsync(DiscoveredDevice device)
    {
        if (IsPaired && PairedDevice?.DeviceId == device.DeviceId) return;

        if (!pairingInFlight.Add(device.DeviceId))
        return;
        

        var token = await pairingService.PairAsync(device, cts.Token);

        if (token != null)
        {
            PairedDevice = device;
            NextCommand.NotifyCanExecuteChanged();
            discoveryService.Stop();
        }
        
        pairingInFlight.Remove(device.DeviceId);
    }

    [RelayCommand]
    private void Back() => navigation.GoBack();

    [RelayCommand(CanExecute = nameof(IsPaired))]
    private void Next() {}
}
