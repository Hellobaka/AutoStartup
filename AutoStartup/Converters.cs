using System.Globalization;
using System.Windows.Data;

namespace AutoStartup
{
    public class StatusColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var status = value?.ToString() ?? "";
            return status switch
            {
                "Running" => Brushes.Green,
                "Stopped" => Brushes.Gray,
                "Error" => Brushes.Red,
                _ => Brushes.Black,
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}