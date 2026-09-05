namespace PhoneBrowser.Desktop.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PhoneBrowser.Desktop.Models;
using PhoneBrowser.Desktop.Services.Discovery;
using PhoneBrowser.Desktop.Services.Pairing;
using System;
using System.Collections.ObjectModel;
using System.Windows;

public partial class PairingViewModel : ObservableObject, IDisposable
{
	private readonly UdpDiscoveryService discoveryService;

	private readonly PairingService pairingService;

    private readonly HashSet<string> pairingInFlight = new();

    private readonly CancellationTokenSource cts = new();

	public ObservableCollection<DiscoveredDevice> DevicesDiscovered { get; } = new();

    [ObservableProperty]
    private DiscoveredDevice? pairedDevice;

    public bool CanGoNext => PairedDevice != null;

    public PairingViewModel()
	{
		discoveryService = new UdpDiscoveryService();
		discoveryService.DeviceDiscovered += OnDeviceDiscovered;

		pairingService = new PairingService("test", "PC");
    }

	public async void StartListening()
	{
        try
        {
            await discoveryService.StartAsync(cts.Token);
        }
        catch (Exception ex){ }
    }

    public void Dispose()
    {
		discoveryService.DeviceDiscovered -= OnDeviceDiscovered;
        cts.Cancel();
        discoveryService.Dispose();
        cts.Dispose();
    }

	private async void OnDeviceDiscovered(DiscoveredDevice device)
	{
        if (!DevicesDiscovered.Any(d => d.DeviceId == device.DeviceId))
            DevicesDiscovered.Add(device);
    }

    [RelayCommand]
    private async Task PairAsync(DiscoveredDevice device)
    {
        if (!pairingInFlight.Add(device.DeviceId))
            return;

        var token = await pairingService.PairAsync(device, cts.Token);

        if (token != null)
        {
            PairedDevice = device;
            OnPropertyChanged(nameof(CanGoNext));
            discoveryService.Stop();
        }
    }
}
