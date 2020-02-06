using System;
using System.Globalization;
using UnityEngine;
using UnityEngine.Assertions;

namespace uguimvvm.converters
{
    /// <summary>
    /// A base class implementation to allow an item index to passed in to the converter and pick the item at the collection index
    /// </summary>
    /// <typeparam name="T">Unity Type</typeparam>
    public abstract class IndexToObjectConverter<T> : ScriptableObject, IValueConverter
    {
        [SerializeField]
        [Tooltip("Provide a default item to pick (optional)")]
        private T defaultValue = default(T);

        [SerializeField]
        [Tooltip("List of items that the converted index should pick from")]
        private T[] convertToValues = null;

        /// <inheritdoc />
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return defaultValue;
            }

            int realValue = (int)value;

            Assert.IsTrue(convertToValues.Length >= realValue, $"Invalid index [{realValue}] for IntToUnityConverter({typeof(T).Name}). Maximum value is {convertToValues.Length}");

            return convertToValues[realValue];
        }

        /// <inheritdoc />
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
