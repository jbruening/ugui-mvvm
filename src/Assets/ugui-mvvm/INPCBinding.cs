using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;
using System.Linq;
using UnityEngine.UI;
using INotifyPropertyChanged = System.ComponentModel.INotifyPropertyChanged;
using PropertyChangedEventArgs = System.ComponentModel.PropertyChangedEventArgs;
using PropertyChangedEventHandler = System.ComponentModel.PropertyChangedEventHandler;
using UnityEngine.EventSystems;

namespace uguimvvm
{
    public class INPCBinding : MonoBehaviour
    {
        [Serializable]
        public class ComponentPath
        {
            public Component Component;
            public string Property;
        }

        public class PropertyPath
        {
            /// <summary>
            /// Emit warnings when GetValue fails due to nulls in the path
            /// </summary>
            public static bool WarnOnGetValue = false;
            /// <summary>
            /// Emit warnings when SetValue fails due to nulls in the path
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
            private PropertyChangedEventHandler _handler;
            public PropertyInfo[] PPath { get { return _pPath; } }
            public string[] Parts { get; private set; }

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

            public string Path { get; private set; }
            public bool IsValid { get; private set; }
            public Type PropertyType { get; private set; }

            /// <summary>
            /// Resolve the value by traversing the property path
            /// </summary>
            /// <param name="root"></param>
            /// <param name="index"></param>
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

                    root = part.GetValue(root, null);
                }

                return root;
            }

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
                for (;i < _pPath.Length - 1; i++)
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

            public void AddHandler(object root, PropertyChangedEventHandler handler)
            {
                for (var i = 0; i < _pPath.Length; i++)
                {
                    var part = GetIdxProperty(i, root);
                    if (part == null) return;

                    TrySubscribe(root, i);

                    if (root != null)
                        root = part.GetValue(root, null);
                    else
                        break;
                }

                _handler = handler;
            }

            internal void TriggerHandler(object sender)
            {
                if (_handler != null)
                    _handler(sender, new PropertyChangedEventArgs(Path));
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

                    if (i+1 < _notifies.Length)
                        TrySubscribe(root, i+1);
                }

                _handler(sender, new PropertyChangedEventArgs(Path));
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
                    for (result = null; result == null && type != null; type = type.BaseType)
                        result = type.GetProperty(name,
                            BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                    return result;
                }
            }

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
        ComponentPath _view;
        [SerializeField]
        string _viewEvent;

        [SerializeField]
        ComponentPath _viewModel;

        [SerializeField]
        BindingMode _mode = BindingMode.TwoWay;
        public BindingMode Mode { get { return _mode; } }

        [SerializeField]
        ScriptableObject _converter;

        IValueConverter _ci;
        Type _vType;
        Type _vmType;
        PropertyPath _vProp;
        PropertyPath _vmProp;

        void Reset()
        {
            var context = gameObject.GetComponentInParent(typeof(DataContext)) as DataContext;
            if (context != null)
                _viewModel = new ComponentPath { Component = context };

            var view = gameObject.GetComponents<UIBehaviour>().OrderBy((behaviour => OrderOnType(behaviour))).FirstOrDefault();
            if (view != null)
                _view = new ComponentPath { Component = view };
        }

        private int OrderOnType(UIBehaviour item)
        {
            if (item is Button) return 0;
            if (item is Text) return 1;
            return 10;
        }

        void Awake()
        {
            _ci = _converter as IValueConverter;

            FigureBindings();
        }

        void OnEnable()
        {
            ApplyVMToV();

            if (_mode == BindingMode.OneTime)
            {
                _vProp = null;
                _vmProp = null;
            }
        }

        void OnDestroy()
        {
            ClearBindings();
        }

        public void ApplyVToVM()
        {
            //Debug.Log("Applying v to vm");
            if (_vmProp == null || _vProp == null) return;

            if (_mode == BindingMode.OneWayToView) return;

            if (_view.Component == null) return;

            if (!enabled) return;

            var value = _vProp.GetValue(_view.Component, null);

            if (_ci != null)
                value = _ci.ConvertBack(value, _vmType, null, System.Threading.Thread.CurrentThread.CurrentCulture);
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

        public void ApplyVMToV()
        {
            if (_vmProp == null || _vProp == null) return;

            if (_mode == BindingMode.OneWayToViewModel) return;

            if (_viewModel.Component == null) return;

            if (!enabled) return;

            var value = GetValue(_viewModel, _vmProp);

            if (_ci != null)
                value = _ci.Convert(value, _vType, null, System.Threading.Thread.CurrentThread.CurrentCulture);
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
                Debug.LogErrorFormat("Could not bind {0} to type {1}", value.GetType(), _vType);
                return;
            }

            //this is a workaround for text objects getting screwed up if assigned null values
            if (value == null && _vProp.PropertyType == typeof(string))
                value = "";

            _vProp.SetValue(_view.Component, value, null);
        }

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
                Debug.LogErrorFormat("Could not bind {0} to type {1}", value.GetType(), _vmType);
                return;
            }

            if (_viewModel.Component is DataContext)
                (_viewModel.Component as DataContext).SetValue(value, _vmProp);
            else
                _vmProp.SetValue(_viewModel.Component, value, null);
        }

        private void FigureBindings()
        {
            _vmProp = FigureBinding(_viewModel, inpc_PropertyChanged, true);
            //post processing will have set up our _view.
            _vProp = FigureBinding(_view, null, false);

            if (_vmProp.IsValid)
                _vmType = _vmProp.PropertyType;
            if (_vProp.IsValid)
                _vType = _vProp.PropertyType;
        }

        public static PropertyPath FigureBinding(ComponentPath path, PropertyChangedEventHandler handler, bool resolveDataContext)
        {
            Type type;
            if (resolveDataContext && path.Component is DataContext)
                type = (path.Component as DataContext).Type;
            else
                type = path.Component.GetType();

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

        void inpc_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "" || e.PropertyName == _viewModel.Property)
                ApplyVMToV();
        }

        private void ClearBindings()
        {
            var inpc = _viewModel.Component as INotifyPropertyChanged;
            if (inpc == null) return;
            inpc.PropertyChanged -= inpc_PropertyChanged;
            //todo: clean up unity events
        }

        object GetDefaultValue(Type t)
        {
            if (t.IsValueType)
                return Activator.CreateInstance(t);

            return null;
        }
    }
}