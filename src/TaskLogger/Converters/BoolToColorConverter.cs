using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace TaskLogger.Converters
{
    public class BoolToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isSuccess)
            {
                return isSuccess ? new SolidColorBrush(Colors.Green) : new SolidColorBrush(Colors.Orange);
            }
            return new SolidColorBrush(Colors.Green);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
