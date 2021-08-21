using System;
using System.Globalization;
using System.Windows.Data;

namespace Launcher
{
    public class FileSizeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo _)
        {
            long fileSize = 0;
            if (value is string) fileSize = long.Parse((string)value);
            if (value is long) fileSize = (long)value;
            if (value is int) fileSize = (int)value;
            if (value is double) fileSize = (long)(double)value;
            return NativeMethod.StrFormatByteSize(fileSize);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo _)
        {
            throw new NotImplementedException();
        }
    }
}
