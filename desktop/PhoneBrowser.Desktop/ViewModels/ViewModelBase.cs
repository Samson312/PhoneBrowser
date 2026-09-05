using CommunityToolkit.Mvvm.ComponentModel;

namespace PhoneBrowser.Desktop.ViewModels;

using System;
using System.Threading.Tasks;

public abstract partial class ViewModelBase:  ObservableObject, IDisposable
{
    public virtual void Dispose() { }
}

