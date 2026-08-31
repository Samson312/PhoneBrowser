namespace PhoneBrowser.Desktop.Views;

using System.Windows;
using PhoneBrowser.Desktop.ViewModels;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        DataContext = new MainViewModel();
    }
}
