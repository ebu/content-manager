﻿    using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace io.ebu.eis.contentmanager.Utils
{
    internal class AspectRatioConverter : IValueConverter
    {
        private static double Ratio = (320.0 / 240.0);

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if(value is double)
            {
                double val = (double)value;
                return val * Ratio;
            }
            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
