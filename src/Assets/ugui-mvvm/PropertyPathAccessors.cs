using System;
using System.Collections.Generic;
using System.Reflection;

namespace uguimvvm
{
    /// <summary>
    /// Functionality for caching accessors for properties.
    /// </summary>
    public static class PropertyPathAccessors
    {
        class PathComparer : IEqualityComparer<PropertyInfo[]>
        {
            public bool Equals(PropertyInfo[] x, PropertyInfo[] y)
            {
                if (x.Length != y.Length) return false;
                for (var i = 0; i < x.Length; i++)
                    if (x[i] != y[i]) return false;
                return true;
            }

            public int GetHashCode(PropertyInfo[] obj)
            {
                unchecked
                {
                    var hash = 17;

                    // get hash code for all items in array
                    // ReSharper disable once ForCanBeConvertedToForeach - unity has bad foreach handling
                    for (var i = 0; i < obj.Length; i++)
                    {
                        var item = obj[i];
                        hash = hash * 23 + ((item != null) ? item.GetHashCode() : 0);
                    }

                    return hash;
                }
            }
        }

        /// <summary>
        /// Placeholder getter used when no valid property getter can be found.
        /// </summary>
        public static readonly Func<object, object> NoGetter = o => null;

        /// <summary>
        /// Placeholder setter used when no valid property setter can be found.
        /// </summary>
        public static readonly Action<object, object> NoSetter = (o, o1) => { };

        /// <summary>
        /// Comparer used for distinguishing getter and setter instances.
        /// </summary>
        public static readonly IEqualityComparer<PropertyInfo[]> Comparer = new PathComparer();

        /// <summary>
        /// Flags to use when looking up properties.
        /// </summary>
        public static readonly BindingFlags Flags = BindingFlags.Public | BindingFlags.Instance;

        private static readonly Dictionary<PropertyInfo[], Func<object, object>> Getters = new Dictionary<PropertyInfo[], Func<object, object>>(Comparer);
        private static readonly Dictionary<PropertyInfo[], Action<object, object>> Setters = new Dictionary<PropertyInfo[], Action<object, object>>(Comparer);

        static bool _initialized;

        /// <summary>
        /// Registers the getter and setter for the given <see cref="PropertyInfo"/> in the cache maintained by this component.
        /// </summary>
        /// <param name="prop">The <see cref="PropertyInfo"/> sequence identifying the property.</param>
        /// <param name="getter">The getter for the property.</param>
        /// <param name="setter">The setter for the property.</param>
        public static void Register(PropertyInfo[] prop, Func<object, object> getter, Action<object, object> setter)
        {
            Getters[prop] = getter;
            Setters[prop] = setter;
        }

        internal static bool ValidateGetter(PropertyInfo[] path, ref Func<object, object> getter)
        {
            if (!_initialized) return false;

            //rather than performing a lookup every single time (which is slow. Look at PathComparer)
            //we instead check if the accessor was marked as 'invalid'
            if (getter != null)
                return getter != NoGetter;

            if (Getters.TryGetValue(path, out getter))
                return true;
            getter = NoGetter;
            return false;
        }

        internal static bool ValidateSetter(PropertyInfo[] path, ref Action<object, object> setter)
        {
            if (!_initialized) return false;

            //rather than performing a lookup every single time (which is slow. Look at PathComparer)
            //we instead check if the accessor was marked as 'invalid'
            if (setter != null)
                return setter != NoSetter;

            if (Setters.TryGetValue(path, out setter))
                return true;
            setter = NoSetter;
            return false;
        }

        /// <summary>
        /// Allow the PropertyBinding.PropertyPath objects to start using the cached property paths.
        /// Registering accessors after this is run is likely to result in the accessors to not be used, due to the paths being set to NoSetter/NoGetter
        /// </summary>
        public static void Initialize()
        {
            //This allows us to delay setting accessors as invalid due to registration possibly happening after PropertyPath objects are constructed
            _initialized = true;
        }
    }
}
