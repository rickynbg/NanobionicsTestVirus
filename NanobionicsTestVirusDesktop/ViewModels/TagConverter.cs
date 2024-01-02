using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;


namespace NanobionicsTestVirusDesktop.ViewModels
{
    [ValueConversion(typeof(object), typeof(Brush))]
    public class TagConverter : BaseViewModel, IValueConverter
    {
        private static int count;
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            SolidColorBrush bs1 = new(Color.FromRgb(119, 201, 134));
            SolidColorBrush bs2 = new(Color.FromRgb(105, 158, 201));

            return count++ % 2 == 0 ? bs1 : bs2;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
