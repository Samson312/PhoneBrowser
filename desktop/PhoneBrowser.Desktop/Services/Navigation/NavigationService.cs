namespace PhoneBrowser.Desktop.Services.Navigation;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PhoneBrowser.Desktop.ViewModels;
using System;
using System.Collections.Generic;

public class NavigationService : INavigationService
{
    private readonly IServiceProvider provider;
    private readonly ILogger<NavigationService> logger;
    private readonly Stack<ViewModelBase> history = new();

    public event Action<ViewModelBase>? CurrentViewModelChanged;

    public bool CanGoBack => history.Count > 1;

    public NavigationService(
        IServiceProvider provider,
        ILogger<NavigationService> logger) 
    {
        this.logger = logger;
        this.provider = provider;
    }

    public void NavigateTo<TViewModel>() where TViewModel : ViewModelBase
    {
        try
        {
            var vm = provider.GetRequiredService<TViewModel>();
            history.Push(vm);
            logger.LogDebug("Navigated to {ViewModel}", typeof(TViewModel).Name);
            CurrentViewModelChanged?.Invoke(vm);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to navigate to {ViewModel}", typeof(TViewModel).Name);
            throw;
        }
    }

    public void GoBack()
    {
        if (!CanGoBack)
        {
            logger.LogWarning("GoBack called with no previous screen in history");
            return;
        }

        var current = history.Pop();

        try
        {
            current.Dispose();
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Error disposing {ViewModel} during back navigation", current.GetType().Name);
        }

        var target = history.Peek();
        logger.LogDebug("Navigated back to {ViewModel}", target.GetType().Name);
        CurrentViewModelChanged?.Invoke(target);
    }
}
