using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;

namespace uguimvvm.converters
{
    [Serializable]
    public abstract class ItemMapping<TSource, TTarget>
    {
        [SerializeField]
        public TSource SourceValue;

        [SerializeField]
        public TTarget TargetValue;
    }

    /// <summary>
    /// A base class implementation to allow conversion between two generic types. 
    /// </summary>
    public abstract class ValueConverter<ItemMapping, TSource, TTarget> : ScriptableObject, IValueConverter where ItemMapping : ItemMapping<TSource, TTarget>, new()
    {
        [SerializeField]
        [Tooltip("List of mappings that can covert to each other")]
        private List<ItemMapping> itemLookup = null;
        
        [SerializeField]
        [Tooltip("If the mappings can covert back to each other")]
        private bool allowTwoWayConversion = false;

        [SerializeField]
        [Tooltip("Default value for SourceValue")]
        private TSource defaultSourceValue = default(TSource);

        [SerializeField]
        [Tooltip("Default value for TargetValue")]
        private TTarget defaultTargetValue = default(TTarget);

        private readonly IEqualityComparer<TSource> sourceComparer;
        private readonly IEqualityComparer<TTarget> targetComparer;

        protected ValueConverter() : this (EqualityComparer<TSource>.Default, EqualityComparer<TTarget>.Default)
        {
        }

        protected ValueConverter(IEqualityComparer<TSource> tSourceComparer ) : this(tSourceComparer, EqualityComparer<TTarget>.Default)
        {
        }

        protected ValueConverter(IEqualityComparer<TSource> tSourceComparer, IEqualityComparer<TTarget> tTargetComparer)
        {
            this.sourceComparer = tSourceComparer;
            this.targetComparer = tTargetComparer;
        }

        public virtual object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (itemLookup == null)
            {
                throw new ArgumentNullException($"Parameter [{itemLookup}] is empty.");
            }

            if (value == null)
            {
                return defaultTargetValue;
            }

            if (!(value is TSource))
            {
                throw new InvalidCastException($"Parameter [value] is invalid type. Expected type is [{typeof(TSource).Name}].");
            }

            var castedValue = (TSource)value;
            var item = itemLookup.FirstOrDefault(x => sourceComparer.Equals(castedValue, x.SourceValue));

            if (item == null)
            {
                return defaultTargetValue;
            }
            else
            {
                return item.TargetValue;
            }
        }

        public virtual object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!allowTwoWayConversion)
            {
                throw new InvalidOperationException();
            }

            if (itemLookup == null)
            {
                throw new ArgumentNullException($"Parameter [{itemLookup}] is empty.");
            }

            if (value == null)
            {
                return defaultSourceValue;
            }

            if (!(value is TTarget))
            {
                throw new InvalidCastException($"Parameter [value] is invalid type. Expected type is [{typeof(TTarget).Name}].");
            }

            var castedValue = (TTarget)value;
            var item = itemLookup.FirstOrDefault(x => targetComparer.Equals(castedValue, x.TargetValue));
            if (item == null)
            {
                return defaultSourceValue;
            }
            else
            {
                return item.SourceValue;
            }
        }
    }
}
