namespace PhoneBrowser.Desktop.ViewModels;

using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using PhoneBrowser.Desktop.Models;
using PhoneBrowser.Desktop.Services.Discovery;

public partial class MainViewModel : ObservableObject, IDisposable
{
	private readonly UdpDiscoveryService discoveryService;
	private readonly CancellationTokenSource cts = new();

	[ObservableProperty]
	private string statusText = "Aplikacja uruchomiona. Oczekiwanie na start nasłuchu...";

	public ObservableCollection<string> LogEntries { get; } = new();

    public MainViewModel()
	{
		discoveryService = new UdpDiscoveryService();
		discoveryService.DeviceDiscovered += OnDeviceDiscovered;
	}

	public void AddLogEntry(string message)
	{
		var timestamp = DateTime.Now.ToString("HH:mm:ss");
		LogEntries.Add($"{timestamp}: {message}");
	}

	public void StartListening()
	{
		_ = discoveryService.StartAsync(cts.Token);
		StatusText = "Nasłuchiwanie UDP...";
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
		AddLogEntry(device.DeviceName);

		await discoveryService.SendPairReplayAsync(device, 54321);

		AddLogEntry($"Wysłano PAIR_REPLY do {device.IpAddress}:{device.Port}, port TCP {54321}");
	}
}
