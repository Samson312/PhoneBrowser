namespace PhoneBrowser.Desktop.Views;

using System.Windows;
using PhoneBrowser.Desktop.ViewModels;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        var vm = new PairingViewModel();

        DataContext = vm;

        vm.StartListening();

        Closed += (_, _) => vm.Dispose();
    }
}
