namespace PhoneBrowser.Desktop.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using PhoneBrowser.Desktop.Models;
using PhoneBrowser.Desktop.Storage;
using System.Collections.ObjectModel;

public partial class DeviceListViewModel : ObservableObject
{
    public ObservableCollection<KnownDeviceItemViewModel> KnownDevices { get; } = new();
    public ObservableCollection<DiscoveredDevice> NewDevices { get; } = new();

    private readonly TimeSpan availabilityTimeout = TimeSpan.FromSeconds(7);

    public DeviceListViewModel(ILocalStore storage, string? connectedDeviceID)
    {
        foreach(var device in storage.GetTrustedDevices())
        {
            var isConnected = connectedDeviceID == device.DeviceId;

            KnownDevices.Add(new KnownDeviceItemViewModel(device, isConnected));
        }
    }

    public void OnDeviceDiscovered(DiscoveredDevice device)
    {
        var knownDevice = KnownDevices.FirstOrDefault(d => d.Device.DeviceId == device.DeviceId);
                                
        if(knownDevice == null)
        {
            if (!NewDevices.Any(d => d.DeviceId == device.DeviceId))
            {
                NewDevices.Add(device);
            }

            return;
        }

        knownDevice.SetAvailable();
    }

    public void RefreshAvailability(object? sender, EventArgs e)
    {
        var now = DateTime.UtcNow;

        foreach (var device in KnownDevices)
            device.EvaluateAvailability(now, availabilityTimeout);
    }

    public void UpdateList(TrustedDevice device)
    {
        var deviceToRemove = NewDevices.FirstOrDefault(d => d.DeviceId == device.DeviceId);

        if (deviceToRemove != null)
        {
            NewDevices.Remove(deviceToRemove);

            var newKnownDevice = new KnownDeviceItemViewModel(device);

            KnownDevices.Add(newKnownDevice);

            return;
        }

        var knownDevice = KnownDevices.FirstOrDefault(d => d.Device.DeviceId == device.DeviceId);
        knownDevice?.Device = device;
    }

    public TrustedDevice? GetTrustedDevice(string deviceId)
    {
        return KnownDevices.FirstOrDefault(d => d.Device.DeviceId == deviceId)?.Device;
    }

    public void SetConnected(TrustedDevice device)
    {
        var known = KnownDevices.FirstOrDefault(d => d.Device.DeviceId == device.DeviceId);
        known?.SetConnected();
    }

    public void SetDisconnected(TrustedDevice device)
    {
        var known = KnownDevices.FirstOrDefault(d => d.Device.DeviceId == device.DeviceId);
        known?.SetDisconnected();
    }
}

