using System;
using System.Globalization;
using UnityEngine;

namespace uguimvvm.Converters
{
    public abstract class ValueToBoolConverter<T> : ScriptableObject, IValueConverter
    {
        protected abstract T TrueValue { get; }
        protected abstract T FalseValue { get; }

        public bool StrictTrue;
        public bool StrictFalse;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return false;
            try
            {
                var val = (T) value;

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

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool)
                return (bool) value ? TrueValue : FalseValue;
            throw new NotImplementedException("Can only convert boolean values back to " + typeof(T));
        }
    }
}