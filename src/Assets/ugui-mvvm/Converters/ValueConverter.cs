using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;

namespace uguimvvm.converters
{
    /// <summary>
    /// A base class implementation to define a mapping from an item of type <typeparamref name="TSource"/> to an item of type <typeparamref name="TTarget"/>.
    /// </summary>
    [Serializable]
    public abstract class ItemMapping<TSource, TTarget>
    {
        /// <summary>
        /// The starting item which maps to <see cref="TargetValue"/>.
        /// </summary>
        [SerializeField]
        public TSource SourceValue;

        /// <summary>
        /// The resulting item when mapping from <see cref="SourceValue"/>.
        /// </summary>
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

        /// <summary>
        /// Constructs a new <see cref="ValueConverter{ItemMapping, TSource, TTarget}"/> instance.
        /// </summary>
        protected ValueConverter() : this(EqualityComparer<TSource>.Default, EqualityComparer<TTarget>.Default)
        {
        }

        /// <summary>
        /// Constructs a new <see cref="ValueConverter{ItemMapping, TSource, TTarget}"/> instance.
        /// </summary>
        /// <param name="tSourceComparer">A custom comparer to use when comparing source values to convert, against source values from the <typeparamref name="ItemMapping"/>.</param>
        protected ValueConverter(IEqualityComparer<TSource> tSourceComparer) : this(tSourceComparer, EqualityComparer<TTarget>.Default)
        {
        }

        /// <summary>
        /// Constructs a new <see cref="ValueConverter{ItemMapping, TSource, TTarget}"/> instance.
        /// </summary>
        /// <param name="tSourceComparer">A custom comparer to use when comparing source values to convert, against source values from the <typeparamref name="ItemMapping"/>.</param>
        /// <param name="tTargetComparer">A custom comparer to use when comparing target values to convert back, against target values from the <typeparamref name="ItemMapping"/>.</param>
        protected ValueConverter(IEqualityComparer<TSource> tSourceComparer, IEqualityComparer<TTarget> tTargetComparer)
        {
            this.sourceComparer = tSourceComparer;
            this.targetComparer = tTargetComparer;
        }

        /// <inheritdoc />
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

        /// <inheritdoc />
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
