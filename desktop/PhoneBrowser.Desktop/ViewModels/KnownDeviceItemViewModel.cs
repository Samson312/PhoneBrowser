namespace PhoneBrowser.Desktop.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using PhoneBrowser.Desktop.Models;

public enum KnownDeviceStatus { Checking, Available, Connected, Unreachable }

public partial class KnownDeviceItemViewModel : ObservableObject
{
    [ObservableProperty]
    private TrustedDevice device;

    [ObservableProperty]
    private KnownDeviceStatus status = KnownDeviceStatus.Checking;

    private DateTime lastSeenUtc;

    public KnownDeviceItemViewModel(TrustedDevice device, bool connected = false)
    {
        Device = device;

        lastSeenUtc = DateTime.UtcNow;

        if (connected) Status = KnownDeviceStatus.Connected;
    }

    public void EvaluateAvailability(DateTime nowUtc, TimeSpan timeout)
    {
        if (Status == KnownDeviceStatus.Connected) return;

        if (nowUtc - lastSeenUtc > timeout)
            Status = KnownDeviceStatus.Unreachable;
    }

    public void SetConnected() => Status = KnownDeviceStatus.Connected;

    public void SetAvailable()
    {
        lastSeenUtc = DateTime.UtcNow;

        if (Status != KnownDeviceStatus.Connected) Status = KnownDeviceStatus.Available; 
    }

    public void SetDisconnected() => Status = KnownDeviceStatus.Available;
}

