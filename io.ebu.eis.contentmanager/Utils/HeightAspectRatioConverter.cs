    using System;
    using System.Globalization;
    using System.Windows.Data;

namespace io.ebu.eis.contentmanager.Utils
{
    internal class HeightAspectRatioConverter : IValueConverter
    {
        private static double Ratio = (240.0 / 320.0);

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value is double)
            {
                double val = (double)value;
                return val * Ratio;
            }
            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
