using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Data;

namespace BoardgamSolver
{
    public class TokenConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            byte val = (byte)value;
            if (val == 0)
            {
                return " ";
            }
            else if (val == 10)
            {
                return "o";
            }
            else
            {
                return val;
            }
        }



        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
