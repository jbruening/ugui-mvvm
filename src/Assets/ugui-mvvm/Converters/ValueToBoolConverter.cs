using System;
using System.Globalization;
using UnityEngine;

namespace uguimvvm.Converters
{
    /// <summary>
    /// A base class implementation to allow conversion from a generic type <typeparamref name="T"/> to a <see cref="bool"/>.
    /// </summary>
    /// <typeparam name="T">The generic type to convert to a <see cref="bool"/>.</typeparam>
    public abstract class ValueToBoolConverter<T> : ScriptableObject, IValueConverter
    {
        /// <summary>
        /// A value that should be treated as <c>true</c> when converted to a <see cref="bool"/>.
        /// </summary>
        protected abstract T TrueValue { get; }

        /// <summary>
        /// A value that should be treated as <c>false</c> when converted to a <see cref="bool"/>.
        /// </summary>
        protected abstract T FalseValue { get; }

        /// <summary>
        /// A flag indicating that when converting a given value to a <see cref="bool"/>, it should only evaluate to <c>true</c> if it matches <see cref="TrueValue"/>.
        /// </summary>
        public bool StrictTrue;

        /// <summary>
        /// A flag indicating that when converting a given value to a <see cref="bool"/>, it should only evaluate to <c>false</c> if it matches <see cref="FalseValue"/>.
        /// </summary>
        public bool StrictFalse;

        /// <inheritdoc />
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return false;
            try
            {
                var val = (T)value;

                if (val.Equals(TrueValue))
                    return true;
                if (val.Equals(FalseValue))
                    return false;

                if (StrictTrue)
                    return false;
                if (StrictFalse)
                    return true;

                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <inheritdoc />
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool)
                return (bool)value ? TrueValue : FalseValue;
            throw new NotImplementedException("Can only convert boolean values back to " + typeof(T));
        }
    }
}
