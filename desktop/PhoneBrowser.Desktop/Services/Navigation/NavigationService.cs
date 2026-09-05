namespace PhoneBrowser.Desktop.Services.Navigation;

using Microsoft.Extensions.DependencyInjection;
using PhoneBrowser.Desktop.ViewModels;
using System;
using System.Collections.Generic;

public class NavigationService : INavigationService
{
    private readonly IServiceProvider provider;
    private readonly Stack<ViewModelBase> history = new();

    public event Action<ViewModelBase>? CurrentViewModelChanged;

    public bool CanGoBack => history.Count > 1;

    public NavigationService(IServiceProvider provider) => this.provider = provider;

    public void NavigateTo<TViewModel>() where TViewModel : ViewModelBase
    {
        var vm = provider.GetRequiredService<TViewModel>();
        history.Push(vm);
        CurrentViewModelChanged?.Invoke(vm);
    }

    public void GoBack()
    {
        if (!CanGoBack) return;

        history.Pop().Dispose();
        CurrentViewModelChanged?.Invoke(history.Peek());
    }
}
