using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Data;

namespace TESTER.Converters
{
    public class VisibilityElement : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            var valueConvert = (bool)value;
            if (valueConvert)
                return Visibility.Visible;
            else
                return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            return value;
        }
    }
}
