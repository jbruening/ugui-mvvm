using System.Collections.Generic;
using UnityEngine;
using System;
using System.Globalization;
using System.Reflection;
using System.Linq;
using UnityEngine.UI;
using INotifyPropertyChanged = System.ComponentModel.INotifyPropertyChanged;
using PropertyChangedEventArgs = System.ComponentModel.PropertyChangedEventArgs;
using PropertyChangedEventHandler = System.ComponentModel.PropertyChangedEventHandler;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

namespace uguimvvm
{
    /// <summary>
    /// Helper methods for performing additional operations on <see cref="Type"/> values.
    /// </summary>
    public static class TypeExtensions
    {
        /// <summary>
        /// Determines if the given <see cref="Type"/> is a <see cref="ValueType"/>.
        /// </summary>
        /// <param name="type">The <see cref="Type"/> to evaluate.</param>
        /// <returns><c>true</c> if type is a <see cref="ValueType"/>, otherwise <c>false</c>.</returns>
        public static bool IsValueType(this Type type)
        {
#if UNITY_WSA && ENABLE_DOTNET && !UNITY_EDITOR
            return type.GetTypeInfo().IsValueType;
#else
            return type.IsValueType;
#endif
        }

        /// <summary>
        /// Gets the type from which the given <see cref="Type"/> directly inherits.
        /// </summary>
        /// <param name="type">The <see cref="Type"/> to evaluate.</param>
        /// <returns>The <see cref="Type"/> from which the current <see cref="Type"/> directly inherits, or <c>null</c> if the current <see cref="Type"/> does not inherit (e.g. an <see cref="object"/> or interface).</returns>
        public static Type BaseType(this Type type)
        {
#if UNITY_WSA && ENABLE_DOTNET && !UNITY_EDITOR
            return type.GetTypeInfo().BaseType;
#else
            return type.BaseType;
#endif
        }
    }

    /// <summary>
    /// Defines a binding that connects the properties of binding targets and data sources.
    /// </summary>
    public class PropertyBinding : MonoBehaviour
    {
        /// <summary>
        /// Defines a path to a property, relative to a defined <see cref="UnityEngine.Component"/>.
        /// </summary>
        [Serializable]
        public class ComponentPath
        {
            /// <summary>
            /// The root object to which the <see cref="Property"/> is relative.
            /// </summary>
            public Component Component;

            /// <summary>
            /// The property path from the <see cref="Component"/> to the property being represented.
            /// </summary>
            public string Property;
        }

        /// <summary>
        /// Implements a data structure for describing a property as a path below another property, or below an owning type.
        /// </summary>
        public class PropertyPath
        {
            /// <summary>
            /// Emit warnings when GetValue fails due to nulls in the path.
            /// </summary>
            public static bool WarnOnGetValue = false;

            /// <summary>
            /// Emit warnings when SetValue fails due to nulls in the path.
            /// </summary>
            public static bool WarnOnSetValue = false;

            class Notifier
            {
                public INotifyPropertyChanged Object;
                public PropertyChangedEventHandler Handler;
                public int Idx;
            }

            private Func<object, object> _getter;
            private Action<object, object> _setter;

            private readonly PropertyInfo[] _pPath;
            private readonly Notifier[] _notifies;
            private Action _handler;

            /// <summary>
            /// The collection of <see cref="PropertyInfo"/> objects for each <see cref="Parts"/>.
            /// </summary>
            public PropertyInfo[] PPath { get { return _pPath; } }

            /// <summary>
            /// The collection of sections the complete path is divided into.
            /// </summary>
            public string[] Parts { get; private set; }

            /// <summary>
            /// Initializes a new instance of the <see cref="PropertyPath"/> class based on the given parameters.
            /// </summary>
            /// <param name="path">The path string to the property.</param>
            /// <param name="type">The <see cref="Type"/> from which the <paramref name="path"/> is relatively defined from.</param>
            /// <param name="warnOnFailure">Flag indicating if a warning should be logged if the <see cref="PropertyPath"/> could not be initialized as expected.</param>
            public PropertyPath(string path, Type type, bool warnOnFailure = false)
            {
                if (path == "this")
                {
                    Path = path;
                    Parts = new[] { path };
                    PropertyType = type;
                    IsValid = true;
                    _pPath = new PropertyInfo[0];
                    _notifies = new Notifier[0];
                    return;
                }

                Parts = path.Split('.');
                Path = path;
                _pPath = new PropertyInfo[Parts.Length];
                _notifies = new Notifier[Parts.Length];
                for (var i = 0; i < Parts.Length; i++)
                {
                    var part = Parts[i];
                    var info = GetProperty(type, part);
                    if (info == null)
                    {
                        if (warnOnFailure)
                            Debug.LogWarningFormat("Could not resolve property {0} on type {1}", part, type);
                        return;
                    }

                    _pPath[i] = info;

                    type = info.PropertyType;
                }

                PropertyType = type;
                IsValid = true;
            }

            /// <summary>
            /// The path string to the property.
            /// </summary>
            public string Path { get; private set; }

            /// <summary>
            /// Flag indicating if the <see cref="Path"/> was successfully resolved to a property of type <see cref="PropertyType"/>.
            /// </summary>
            public bool IsValid { get; private set; }

            /// <summary>
            /// The <see cref="Type"/> of the property.
            /// </summary>
            public Type PropertyType { get; private set; }

            /// <summary>
            /// Gets the value of the property defined by this <see cref="PropertyPath"/>, relative to a given object.
            /// </summary>
            /// <param name="root">The object from which the <see cref="Path"/> is relative.</param>
            /// <param name="index">Optional index values for indexed properties. This value should be <c>null</c> for non-indexed properties.</param>
            /// <returns></returns>
            public object GetValue(object root, object[] index)
            {
                if (!IsValid)
                    return null;

                if (root == null)
                {
                    if (WarnOnGetValue)
                        Debug.LogWarningFormat("Cannot get value to {0} on a null object", Path);
                    return null;
                }

                if (PropertyPathAccessors.ValidateGetter(_pPath, ref _getter))
                    return _getter(root);

                // ReSharper disable once ForCanBeConvertedToForeach - unity has bad foreach handling
                for (int i = 0; i < _pPath.Length; i++)
                {
                    if (root == null)
                    {
                        if (WarnOnGetValue)
                            Debug.LogWarningFormat("value of {0} was null when getting {1}", Parts[i - 1], Path);
                        return null;
                    }

                    var part = GetIdxProperty(i, root);

                    if (part == null)
                        return null;

                    root = part.GetValue(root, (i == (_pPath.Length - 1)) ? index : null);
                }

                return root;
            }

            /// <summary>
            /// Sets the value of the property defined by this <see cref="PropertyPath"/>, relative to a given object.
            /// </summary>
            /// <param name="root">The object from which the <see cref="Path"/> is relative.</param>
            /// <param name="value">The value to assign as the property's value.</param>
            /// <param name="index">Optional index values for indexed properties. This value should be <c>null</c> for non-indexed properties.</param>
            public void SetValue(object root, object value, object[] index)
            {
                if (!IsValid)
                    return;

                if (PropertyPathAccessors.ValidateSetter(_pPath, ref _setter))
                {
                    _setter(root, value);
                    return;
                }

                var i = 0;
                for (; i < _pPath.Length - 1; i++)
                {
                    var part = GetIdxProperty(i, root);

                    if (part == null)
                        return;

                    root = part.GetValue(root, null);
                    if (root == null)
                    {
                        if (WarnOnSetValue)
                            Debug.LogWarningFormat("value of {0} was null when attempting to set {1}", part.Name, Path);
                        return;
                    }
                }

                _pPath[i].SetValue(root, value, index);
            }

            /// <summary>
            /// Set the callback to be invoked if the property (or any of its parent properties) change.
            /// </summary>
            /// <param name="root">The object from which the <see cref="Path"/> is relative.</param>
            /// <param name="handler">The callback to be invoked on property change.</param>
            public void AddHandler(object root, Action handler)
            {
                for (var i = 0; i < _pPath.Length; i++)
                {
                    var part = GetIdxProperty(i, root);
                    if (part == null) return;

                    TrySubscribe(root, i);

                    if (root != null && part.CanRead)
                        root = part.GetValue(root, null);
                    else
                        break;
                }

                _handler = handler;
            }

            internal void TriggerHandler()
            {
                if (_handler != null)
                    _handler();
            }

            private void OnPropertyChanged(object sender, PropertyChangedEventArgs args, Notifier notifier)
            {
                if (args.PropertyName != "" && args.PropertyName != Parts[notifier.Idx])
                    return;

                //get rid of old subscriptions, just in case the objects aren't fully cleaned up
                for (var i = notifier.Idx + 1; i < _notifies.Length; i++)
                {
                    var ni = _notifies[i];
                    if (ni == null) continue;
                    ni.Object.PropertyChanged -= ni.Handler;
                    _notifies[i] = null;
                }

                //and now re-subscribe to the new 'tree'
                object root = notifier.Object;
                for (var i = notifier.Idx; i < _notifies.Length; i++)
                {
                    var part = GetIdxProperty(i, root);
                    if (part == null) return; //nope. invalid path.

                    root = part.GetValue(root, null);

                    if (root == null) return; //nope. new tree is lacking value somewhere

                    if (i + 1 < _notifies.Length)
                        TrySubscribe(root, i + 1);
                }

                _handler();
            }

            private void TrySubscribe(object root, int idx)
            {
                if (root is INotifyPropertyChanged)
                {
                    var notifier = new Notifier { Object = root as INotifyPropertyChanged, Idx = idx };
                    notifier.Handler = (sender, args) => OnPropertyChanged(sender, args, notifier);
                    notifier.Object.PropertyChanged += notifier.Handler;
                    _notifies[idx] = notifier;
                }
            }

            private PropertyInfo GetIdxProperty(int idx, object root)
            {
                return _pPath[idx] ?? GetProperty(root.GetType(), Parts[idx]);
            }

            /// <summary>
            /// Gets the <see cref="PropertyInfo"/> for a named property of a given <see cref="Type"/>.
            /// </summary>
            /// <param name="type">The base <see cref="Type"/> that defines the property.</param>
            /// <param name="name">The name of the property.</param>
            /// <returns>The <see cref="PropertyInfo"/> of the named property of the specified <see cref="Type"/>.</returns>
            public static PropertyInfo GetProperty(Type type, string name)
            {
                if (type == null)
                    return null;
                try
                {
                    return type.GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);
                }
                catch (AmbiguousMatchException)
                {
                    PropertyInfo result;
                    for (result = null; result == null && type != null; type = type.BaseType())
                    {
                        result = type.GetProperty(name,
                            BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                    }
                    return result;
                }
            }

            /// <summary>
            /// Unsubscribe/clear the callback registered to be invoked on property change.
            /// </summary>
            public void ClearHandlers()
            {
                for (var i = 0; i < _notifies.Length; i++)
                {
                    var n = _notifies[i];
                    _notifies[i] = null;
                    if (n == null) continue;
                    n.Object.PropertyChanged -= n.Handler;
                }
                _handler = null;
            }
        }

        [SerializeField]
        [FormerlySerializedAs("_view")]
        ComponentPath _target;

        /// <summary>
        /// The object and path to use as the binding target.
        /// </summary>
        public ComponentPath Target => _target;

        [SerializeField]
        private BindingUpdateTrigger _targetUpdateTrigger = BindingUpdateTrigger.None;

#pragma warning disable 0414 // _targetEvent is not used directly, only via SerializedObject.FindProperty

        [SerializeField]
        [FormerlySerializedAs("_viewEvent")]
        string _targetEvent = null;

#pragma warning restore 0414

        [SerializeField]
        [FormerlySerializedAs("_viewModel")]
        ComponentPath _source;

        /// <summary>
        /// The object and path to use as the binding source.
        /// </summary>
        public ComponentPath Source => _source;

        [SerializeField]
        private BindingUpdateTrigger _sourceUpdateTrigger = BindingUpdateTrigger.None;

#pragma warning disable 0414 // _sourceEvent is not used directly, only via SerializedObject.FindProperty

        [SerializeField]
        private string _sourceEvent = null;

#pragma warning restore 0414

        [SerializeField]
        BindingMode _mode = BindingMode.OneWayToTarget;

        /// <summary>
        /// Value indicating the direction of the data flow in the binding.
        /// </summary>
        public BindingMode Mode { get { return _mode; } }

        [SerializeField]
        ScriptableObject _converter = null;

        IValueConverter _ci;
        Type _vType;
        Type _vmType;
        PropertyPath _vProp;
        PropertyPath _vmProp;

        void Reset()
        {
            var context = gameObject.GetComponentInParent(typeof(DataContext)) as DataContext;
            if (context != null)
                _source = new ComponentPath { Component = context };

            var view = gameObject.GetComponents<UIBehaviour>().OrderBy((behaviour => OrderOnType(behaviour))).FirstOrDefault();
            if (view != null)
                _target = new ComponentPath { Component = view };
        }

        private int OrderOnType(UIBehaviour item)
        {
            if (item is Button) return 0;
            if (item is Text) return 1;
            return 10;
        }

        private void Awake()
        {
            _ci = _converter as IValueConverter;

            FigureBindings();
        }

        private void OnEnable()
        {
            UpdateTarget();

            if (_mode == BindingMode.OneTime)
            {
                _vProp = null;
                _vmProp = null;
            }
        }

        private void OnDestroy()
        {
            ClearBindings();
        }

        /// <summary>
        /// Deprecated - use <see cref="UpdateSource"/> instead.
        /// </summary>
        [Obsolete("Use UpdateSource")]
        public void ApplyVToVM()
        {
            UpdateSource();
        }

        /// <summary>
        /// Updates the value of the <see cref="Source"/> based on the value of the <see cref="Target"/>, if the binding is configured to allow data flow in that direction.
        /// </summary>
        public void UpdateSource()
        {
            //Debug.Log("Applying v to vm");
            if (_vmProp == null || _vProp == null) return;

            if (_mode == BindingMode.OneWayToTarget) return;

            if (_target.Component == null) return;

            if (!enabled) return;

            var value = _vProp.GetValue(_target.Component, null);

            if (_ci != null)
            {
                var currentCulture = CultureInfo.CurrentCulture;
                value = _ci.ConvertBack(value, _vmType, null, currentCulture);
            }
            else if (value != null)
                value = System.Convert.ChangeType(value, _vmType);
            else
                value = GetDefaultValue(_vmType);

            if (value is IDelayedValue)
            {
                if (!(value as IDelayedValue).ValueOrSubscribe(SetVmValue, ref value))
                    return;
            }

            SetVmValue(value);
        }

        /// <summary>
        /// Deprecated - use <see cref="UpdateTarget"/> instead.
        /// </summary>
        [Obsolete("Use UpdateTarget")]
        public void ApplyVMToV()
        {
            UpdateTarget();
        }

        /// <summary>
        /// Updates the value of the <see cref="Target"/> based on the value of the <see cref="Source"/>, if the binding is configured to allow data flow in that direction.
        /// </summary>
        public void UpdateTarget()
        {
            if (_vmProp == null || _vProp == null) return;

            if (_mode == BindingMode.OneWayToSource) return;

            if (_source.Component == null) return;

            if (!enabled) return;

            var value = GetValue(_source, _vmProp);

            if (_ci != null)
            {
                var currentCulture = CultureInfo.CurrentCulture;
                value = _ci.Convert(value, _vType, null, currentCulture);
            }
            else if (value != null)
            {
                if (!_vType.IsInstanceOfType(value))
                {
                    value = System.Convert.ChangeType(value, _vType);
                }
            }
            else
                value = GetDefaultValue(_vType);

            if (value is IDelayedValue)
            {
                if (!(value as IDelayedValue).ValueOrSubscribe(SetVValue, ref value))
                    return;
            }

            SetVValue(value);
        }

        private void SetVValue(object value)
        {
            if (value != null && !_vType.IsInstanceOfType(value))
            {
                Debug.LogErrorFormat(this, "Could not bind {0} to type {1}", value.GetType(), _vType);
                return;
            }

            //this is a workaround for text objects getting screwed up if assigned null values
            if (value == null && _vProp.PropertyType == typeof(string))
                value = "";

            _vProp.SetValue(_target.Component, value, null);
        }

        /// <summary>
        /// Gets the value of the requested property on the requested component.
        /// </summary>
        /// <param name="path">The path to the base component from which the <paramref name="prop"/> is relatively defined.</param>
        /// <param name="prop">The property to fetch the value of.</param>
        /// <param name="resolveDataContext">
        /// Flag indicating that if the <paramref name="path"/> points to a <see cref="DataContext"/>, the <paramref name="prop"/> will
        /// be evaluated as relative to the <see cref="DataContext"/>'s <see cref="DataContext.Value"/>, rather than the default behaviour
        /// of evaluating against the <see cref="DataContext"/> object directly.
        /// If the <paramref name="path"/> does not point to a <see cref="DataContext"/> settings this flag will have no effect.
        /// </param>
        /// <returns></returns>
        public static object GetValue(ComponentPath path, PropertyPath prop, bool resolveDataContext = true)
        {
            if (resolveDataContext && path.Component is DataContext)
                return (path.Component as DataContext).GetValue(prop);
            else
                return prop.GetValue(path.Component, null);
        }

        private void SetVmValue(object value)
        {
            if (value != null && value.GetType() != _vmType)
            {
                Debug.LogErrorFormat(this, "Could not bind {0} to type {1}", value.GetType(), _vmType);
                return;
            }

            if (_source.Component is DataContext)
                (_source.Component as DataContext).SetValue(value, _vmProp);
            else
                _vmProp.SetValue(_source.Component, value, null);
        }

        private void FigureBindings()
        {
            // Post processing will have set up our _target iff the update trigger is a Unity event.
            Action sourceUpdateHandler = null;
            if (_sourceUpdateTrigger != BindingUpdateTrigger.UnityEvent)
            {
                sourceUpdateHandler = UpdateTarget;
            }
            _vmProp = FigureBinding(_source, sourceUpdateHandler, true);

            // Post processing will have set up our _target iff the update trigger is a Unity event.
            Action targetUpdateHandler = null;
            if (_targetUpdateTrigger != BindingUpdateTrigger.UnityEvent)
            {
                targetUpdateHandler = UpdateSource;
            }
            _vProp = FigureBinding(_target, targetUpdateHandler, false);

            if (_vmProp.IsValid)
            {
                _vmType = _vmProp.PropertyType;
            }
            else
            {
                Debug.LogErrorFormat(this, "INPCBinding: Invalid Source property in \"{0}\".",
                    gameObject.GetParentNameHierarchy());
            }

            if (_vProp.IsValid)
            {
                _vType = _vProp.PropertyType;
            }
            else
            {
                Debug.LogErrorFormat(this, "INPCBinding: Invalid Target property in \"{0}\".",
                    gameObject.GetParentNameHierarchy());
            }
        }

        /// <summary>
        /// Creates and registers a <see cref="PropertyPath"/> to connect a property to a property-changed handler.
        /// </summary>
        /// <param name="path">The property of interest.</param>
        /// <param name="handler">The callback to be invoked when the value of the property changes.</param>
        /// <param name="resolveDataContext">
        /// Flag indicating that if the root of the <paramref name="path"/> is a <see cref="DataContext"/>, the relative portion
        /// of the <paramref name="path"/> should be evaluated against the <see cref="DataContext"/>'s <see cref="DataContext.Value"/>,
        /// rather than the default behavior of evaluating against the <see cref="DataContext"/> directly.
        /// If the root of the <paramref name="path"/> is not a <see cref="DataContext"/> setting this flag will have no effect.
        /// </param>
        /// <returns></returns>
        public static PropertyPath FigureBinding(ComponentPath path, Action handler, bool resolveDataContext)
        {
            Type type = PropertyBinding.GetComponentType(path.Component, resolveDataContext);

            var prop = new PropertyPath(path.Property, type, true);

            if (handler != null)
            {
                if (resolveDataContext && path.Component is DataContext)
                    (path.Component as DataContext).AddDependentProperty(prop, handler);
                else
                    prop.AddHandler(path.Component, handler);
            }

            return prop;
        }

        /// <summary>
        /// Get the effective <see cref="Type"/> of the given <see cref="Component"/>.
        /// </summary>
        /// <param name="component">The object to determine the <see cref="Type"/> of.</param>
        /// <param name="resolveDataContext">
        /// Flag indicating that if the <paramref name="component"/> is a <see cref="DataContext"/>, the returned <see cref="Type"/>
        /// should by the <see cref="Type"/> of the <see cref="DataContext"/>'s <see cref="DataContext.Value"/>, rather than the
        /// <see cref="DataContext"/>'s type directly.
        /// If the <paramref name="component"/> is not a <see cref="DataContext"/> settings this flag will have no effect.
        /// </param>
        public static Type GetComponentType(Component component, bool resolveDataContext)
        {
            if (component == null)
            {
                return null;
            }

            if (resolveDataContext)
            {
                DataContext dataContext = component as DataContext;
                if (dataContext != null)
                {
                    return dataContext.Type;
                }
            }

            return component.GetType();
        }

        private void ClearBindings()
        {
            _vmProp?.ClearHandlers();
            _vProp?.ClearHandlers();
            //todo: clean up unity events
        }

        object GetDefaultValue(Type t)
        {
            if (t.IsValueType())
            {
                return Activator.CreateInstance(t);
            }

            return null;
        }
    }
}
