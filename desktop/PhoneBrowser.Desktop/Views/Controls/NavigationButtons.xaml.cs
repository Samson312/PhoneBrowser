namespace PhoneBrowser.Desktop.Views.Controls;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

public partial class NavigationButtons : UserControl
{
    public static readonly DependencyProperty BackCommandProperty =
        DependencyProperty.Register(nameof(BackCommand), typeof(ICommand), typeof(NavigationButtons));

    public static readonly DependencyProperty NextCommandProperty =
        DependencyProperty.Register(nameof(NextCommand), typeof(ICommand), typeof(NavigationButtons));

    public static readonly DependencyProperty NextTextProperty =
        DependencyProperty.Register(nameof(NextText), typeof(string), typeof(NavigationButtons),
            new PropertyMetadata("Dalej"));

    public ICommand? BackCommand
    {
        get => (ICommand?)GetValue(BackCommandProperty);
        set => SetValue(BackCommandProperty, value);
    }

    public ICommand? NextCommand
    {
        get => (ICommand?)GetValue(NextCommandProperty);
        set => SetValue(NextCommandProperty, value);
    }

    public string NextText
    {
        get => (string)GetValue(NextTextProperty);
        set => SetValue(NextTextProperty, value);
    }

    public NavigationButtons()
    {
        InitializeComponent();
    }
}