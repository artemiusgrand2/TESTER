using System;
using System.Globalization;
using System.Collections.Generic;
using System.Windows.Data;
using System.Windows.Media;
using System.Linq;
using System.Text;

namespace TESTER
{
    class CurrentPlayMesToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool)value)
                return new LinearGradientBrush(Color.FromArgb(255, 255, 255, 50), Color.FromArgb(127, 255, 255, 220), 45);
            else
                return Brushes.Silver;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new Exception("The method or operation is not implemented.");
        }
    }
}
