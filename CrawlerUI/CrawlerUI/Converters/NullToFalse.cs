namespace CrawlerUI
{
    using System;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Data;

    /// <inheritdoc />
    public class NullToFalse : IValueConverter
    {
        /// <inheritdoc />
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value != null;
        }

        /// <inheritdoc />
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}