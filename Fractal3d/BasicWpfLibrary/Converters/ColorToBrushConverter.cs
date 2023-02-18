using System.Windows.Data;
using System.Windows.Media;
using System;

namespace BasicWpfLibrary.Converters
{

    public class ColorToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is not Color)
                throw new InvalidOperationException("Value must be a Color");

            return new SolidColorBrush((Color)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

    }
}

