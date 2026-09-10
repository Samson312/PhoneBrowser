namespace PhoneBrowser.Desktop.Converters;

using System.Globalization;
using System.Windows;
using System.Windows.Data;

public class SelectedToThicknessConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
        value is true ? new Thickness(8) : new Thickness(0);

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
