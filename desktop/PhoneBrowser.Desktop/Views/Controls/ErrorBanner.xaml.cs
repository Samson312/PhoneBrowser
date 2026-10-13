namespace PhoneBrowser.Desktop.Views.Controls;

using PhoneBrowser.Desktop.Errors;
using System.Windows;
using System.Windows.Controls;

public partial class ErrorBanner : UserControl
{
    public static readonly DependencyProperty MessageProperty =
    DependencyProperty.Register(nameof(Message), typeof(UserMessage), typeof(ErrorBanner));

    public UserMessage? Message
    {
        get => (UserMessage?) GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
    }

    public ErrorBanner()
    {
        InitializeComponent();
    }
}

