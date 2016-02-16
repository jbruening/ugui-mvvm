using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;
using System.Linq;
using UnityEngine.UI;
using INotifyPropertyChanged = System.ComponentModel.INotifyPropertyChanged;
using PropertyChangedEventArgs = System.ComponentModel.PropertyChangedEventArgs;
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
            private readonly PropertyInfo[] _pPath;
            public PropertyInfo[] PPath { get { return _pPath; } }
            public string[] Parts { get; private set; }

            public PropertyPath(string path, Type type, bool warnOnFailure = false)
            {
                Parts = path.Split('.');
                Path = path;
                _pPath = new PropertyInfo[Parts.Length];
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

// ReSharper disable once ForCanBeConvertedToForeach - unity has bad foreach handling
                for (int i = 0; i < _pPath.Length; i++)
                {
                    var part = _pPath[i] ?? GetProperty(root.GetType(), Parts[i]);

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

                var i = 0;
                for (;i < _pPath.Length - 1; i++)
                {
                    var part = _pPath[i] ?? GetProperty(root.GetType(), Parts[i]);

                    if (part == null)
                        return;

                    root = part.GetValue(root, null);
                }

                _pPath[i].SetValue(root, value, index);
            }

            private PropertyInfo GetProperty(Type type, string name)
            {
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
        }

        [SerializeField]
        ComponentPath _view;
        [SerializeField]
        string _viewEvent;

        [SerializeField]
        ComponentPath _viewModel;

        [SerializeField]
        BindingMode _mode = BindingMode.TwoWay;

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

        public static PropertyPath FigureBinding(ComponentPath path, System.ComponentModel.PropertyChangedEventHandler handler, bool resolveDataContext)
        {
            Type type;
            if (resolveDataContext && path.Component is DataContext)
                type = (path.Component as DataContext).Type;
            else
                type = path.Component.GetType();

            var prop = new PropertyPath(path.Property, type, true);

            if (handler != null)
            {
                var inpc = path.Component as INotifyPropertyChanged;
                if (inpc != null)
                    inpc.PropertyChanged += handler;
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