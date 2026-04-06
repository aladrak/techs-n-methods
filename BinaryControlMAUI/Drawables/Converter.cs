using System.Globalization;

namespace BinaryControlMAUI.Drawables;

public class DeletedToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => (bool?)value == true ? Colors.Gray : Colors.Black;
    
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}