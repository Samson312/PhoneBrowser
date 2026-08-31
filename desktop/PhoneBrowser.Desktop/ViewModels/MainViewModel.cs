namespace PhoneBrowser.Desktop.ViewModels;

using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
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
	}

	public void AddLogEntry(string message)
	{
		var timestamp = DateTime.Now.ToString("HH:mm:ss");
		LogEntries.Add($"{timestamp}: {message}");
	}

	public void StartListening()
	{
		_ = ListenLoop(cts.Token);
	}

    public void Dispose()
    {
        cts.Cancel();
        discoveryService.Dispose();
        cts.Dispose();
    }

    private async Task ListenLoop(CancellationToken ct)
	{
        StatusText = "Nasłuchiwanie UDP...";

		try
		{
			while (!ct.IsCancellationRequested)
			{
				var device = await discoveryService.Listen(ct);
				AddLogEntry(device.DeviceName);
			}
		}
		catch (OperationCanceledException)
		{
            StatusText = "Nasłuchiwanie zakończone";
        }
		catch (Exception ex)
		{
            StatusText = $"Błąd nasłuchu: {ex.Message}";
        }
    }
}
