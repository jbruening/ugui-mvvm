using System;
using System.Globalization;
using UnityEngine;

namespace uguimvvm.converters
{
    /// <summary>
    /// Inverts the passed in boolean value 
    /// </summary>
    public class BoolInverter : ScriptableObject, IValueConverter
    {
        /// <inheritdoc />
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return true;
            }

            return !(bool)value;
        }

        /// <inheritdoc />
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
