using UnityEngine;
using System;
using System.Reflection;
using System.Linq;
using INotifyPropertyChanged = System.ComponentModel.INotifyPropertyChanged;
using PropertyChangedEventArgs = System.ComponentModel.PropertyChangedEventArgs;

namespace uuguimvvm
{
    public class INPCBinding : MonoBehaviour
    {
        [Serializable]
        public class ComponentPath
        {
            public Component Component;
            public string Property;
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
        PropertyInfo _vProp;
        PropertyInfo _vmProp;

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
            else if (value == null)
                value = GetDefaultValue(_vmType);

            if (value != null && value.GetType() != _vmType)
            {
                Debug.LogErrorFormat("Could not bind {0} to type {1}", value.GetType(), _vmType);
                return;
            }

            SetVmValue(value);
        }

        public void ApplyVMToV()
        {
            if (_vmProp == null || _vProp == null) return;

            if (_mode == BindingMode.OneWayToViewModel) return;

            var value = GetVmValue();

            if (_ci != null)
                value = _ci.Convert(value, _vType, null, System.Threading.Thread.CurrentThread.CurrentCulture);
            else if (value != null)
            {
                if (!_vType.IsAssignableFrom(value.GetType()))
                {
                    value = System.Convert.ChangeType(value, _vType);
                }
            }
            else if (value == null)
                value = GetDefaultValue(_vType);

            if (value != null && !_vType.IsAssignableFrom(value.GetType()))
            {
                Debug.LogErrorFormat("Could not bind {0} to type {1}", value.GetType(), _vType);
                return;
            }

            //this is a workaround for text objects getting screwed up if assigned null values
            if (value == null && _vProp.PropertyType == typeof(string))
                value = "";

            _vProp.SetValue(_view.Component, value, null);
        }

        private object GetVmValue()
        {
            if (_viewModel.Component is DataContext)
                return (_viewModel.Component as DataContext).GetValue(_vmProp);
            else
                return _vmProp.GetValue(_viewModel.Component, null);
        }

        private void SetVmValue(object value)
        {
            if (_viewModel.Component is DataContext)
                (_viewModel.Component as DataContext).SetValue(value, _vmProp);
            else
                _vmProp.SetValue(_viewModel.Component, value, null);
        }

        private void FigureBindings()
        {
            //post processing will have set up our _view.
            Type vmtype;
            if (_viewModel.Component is DataContext)
                vmtype = (_viewModel.Component as DataContext).Type;
            else
                vmtype = _viewModel.Component.GetType();

            _vmProp = vmtype.GetProperties().FirstOrDefault(p => string.Equals(p.Name, _viewModel.Property, StringComparison.OrdinalIgnoreCase));

            _vProp = _view.Component.GetType().GetProperties().FirstOrDefault(p => string.Equals(p.Name, _view.Property, StringComparison.OrdinalIgnoreCase));
            //Debug.LogFormat("vprop is {0}", _vProp);

            if (_vmProp != null)
                _vmType = _vmProp.PropertyType;
            if (_vProp != null)
                _vType = _vProp.PropertyType;

            var inpc = _viewModel.Component as INotifyPropertyChanged;
            if (inpc == null) return;
            inpc.PropertyChanged += inpc_PropertyChanged;
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