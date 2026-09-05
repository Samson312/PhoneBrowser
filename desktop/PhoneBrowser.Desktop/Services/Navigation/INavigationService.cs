namespace PhoneBrowser.Desktop.Services.Navigation;

using PhoneBrowser.Desktop.ViewModels;
using System;

public interface INavigationService
{
    event Action<ViewModelBase>? CurrentViewModelChanged;
    void NavigateTo<TViewModel>() where TViewModel : ViewModelBase;
    void GoBack();
    bool CanGoBack { get; }
}

