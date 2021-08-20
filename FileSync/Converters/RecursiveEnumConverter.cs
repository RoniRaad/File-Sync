using FileSync.DomainModel.Enums;
using System;
using System.Globalization;
using System.Windows.Data;

namespace FileSync
{
    public class RecursiveEnumConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is RecursionType.Recursive)
            {
                return "Recursive (Subdirectores)";
            }
            else
            {
                return "None";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
