using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Data;

namespace TESTER.Converters
{
    public class VisibilityMultiElement : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter,
        System.Globalization.CultureInfo culture)
        {
            if ((bool)values[0] || (bool)values[1])
                return Visibility.Visible;
            else
                return Visibility.Collapsed;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter,
            System.Globalization.CultureInfo culture)
        {
            return new object[2];
        }
    }
}
