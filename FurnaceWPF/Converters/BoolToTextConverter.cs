using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace FurnaceWPF.Converters
{
    public class BoolToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not bool b)
                return "";

            if (parameter is not string param)
                return "";

            var parts = param.Split(';');
            if (parts.Length != 2)
                return "";

            string trueText = parts[0];
            string falseText = parts[1];

            return b ? trueText : falseText;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
