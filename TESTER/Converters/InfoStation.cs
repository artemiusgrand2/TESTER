using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Data;

namespace TESTER.Converters
{
    public class InfoStation : IMultiValueConverter
    {


        public object Convert(object[] values, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            return string.Empty;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter,
            System.Globalization.CultureInfo culture)
        {
            return new object[2];
        }

    }
}
