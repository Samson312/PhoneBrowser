namespace PhoneBrowser.Desktop.Views;

using PhoneBrowser.Desktop.ViewModels;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

public partial class PhotoGalleryView : UserControl
{
    private const double TileCellWidth = 258;

    public PhotoGalleryView()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        var oldVm = e.OldValue as PhotoGalleryViewModel;
        if (oldVm != null) oldVm.ScrollToRowRequested -= OnScrollToRowRequested;

        var newVm = e.NewValue as PhotoGalleryViewModel;
        if (newVm != null) newVm.ScrollToRowRequested += OnScrollToRowRequested;
    }

    private void OnPhotosListSizeChanged(object sender, SizeChangedEventArgs e)
    {
        var vm = DataContext as PhotoGalleryViewModel;
        if (vm == null) return;

        var width = e.NewSize.Width - SystemParameters.VerticalScrollBarWidth;
        vm.SetColumnCount(Math.Max(1, (int)(width / TileCellWidth)));
    }

    private void OnScrollToRowRequested(int index)
    {
        var scrollViewer = FindChild<ScrollViewer>(PhotosList);
        if (scrollViewer == null) return;

        scrollViewer.ScrollToVerticalOffset(index);
    }

    private void OnPhotosListPreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        var scrollViewer = FindChild<ScrollViewer>(PhotosList);
        if (scrollViewer == null) return;

        scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - Math.Sign(e.Delta));
        e.Handled = true;
    }

    private static T? FindChild<T>(DependencyObject parent) where T : DependencyObject
    {
        for (var i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);

            var match = child as T;
            if (match != null) return match;

            var nested = FindChild<T>(child);
            if (nested != null) return nested;
        }

        return null;
    }
}

