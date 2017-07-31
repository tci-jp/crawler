// <copyright file="NullToFalse.cs" company="DECTech.Tokyo">
// Copyright (c) DECTech.Tokyo. All rights reserved.
// </copyright>

namespace CrawlerUI.Converters
{
    using System;
    using System.Globalization;
    using System.Windows.Data;

    /// <inheritdoc />
    /// <summary>
    /// Converts Null to false and not-null to true.
    /// </summary>
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