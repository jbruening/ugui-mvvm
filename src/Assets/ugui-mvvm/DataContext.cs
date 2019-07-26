using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
#if UNITY_WSA || !NET_LEGACY
using System.Windows.Input;
#endif
using UnityEngine;
using Component = UnityEngine.Component;

namespace uguimvvm
{
    public class DataContext : MonoBehaviour, INotifyPropertyChanged
    {
#pragma warning disable 0649
        [SerializeField]
        string _type;
#pragma warning restore 0649

        private Type _rtype;
        public Type Type
        {
            get { return _rtype ?? (_rtype = Type.GetType(_type)); }
        }

        private object _value;
        public object Value
        {
            get { return _value; }
            set { UpdateValue(value); }
        }

        [SerializeField]
        private PropertyBinding.ComponentPath _propertyBinding = null;
        public PropertyBinding.ComponentPath PropertyBinding => _propertyBinding;
        public Component Component { get { return _propertyBinding?.Component; } }
        private PropertyBinding.PropertyPath _prop;

        [Tooltip("Instantiate the type on awake. This will not work for UnityEngine.Object types")]
        [SerializeField]
        bool _instantiateOnAwake = false;

        private readonly List<DependentProperty> _dependents = new List<DependentProperty>();

        void Awake()
        {
            if (_propertyBinding != null && _propertyBinding.Component != null)
                _prop = uguimvvm.PropertyBinding.FigureBinding(_propertyBinding, this.BindingPropertyChanged, true);

            ApplyBindingToValue();

            if (!_instantiateOnAwake) return;
            if (Type == null)
            {
                Debug.LogError("Set to instantiate on awake, but type is not defined, or is not valid", this);
                return;
            }
            if (typeof(UnityEngine.Object).IsAssignableFrom(Type))
            {
                Debug.LogErrorFormat(this, "Cannot automatically instantiate type {0}, as it derives from UnityEngine.Object", Type);
                return;
            }

            UpdateValue(Activator.CreateInstance(Type));
        }

        public void UpdateValue(object value)
        {
            if (value == _value) return;

            _value = value;

            for (int i = 0; i < _dependents.Count; i++)
            {
                var item = _dependents[i];
                item.Prop.ClearHandlers();
                if (_value != null)
                {
                    item.Prop.AddHandler(_value, item.Handler);
                    item.Prop.TriggerHandler(_value);
                }
            }

            //update all properties
            if (PropertyChanged != null)
                PropertyChanged(_value, new PropertyChangedEventArgs(""));
        }

        void OnDestroy()
        {
            for (int i = 0; i < _dependents.Count; i++)
            {
                var d = _dependents[i];
                d.Prop.ClearHandlers();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        internal object GetValue(PropertyBinding.PropertyPath property)
        {
            if (_value == null)
            {
                return null;
            }

            return property.GetValue(_value, null);
        }

        internal void SetValue(object value, PropertyBinding.PropertyPath property)
        {
            if (_value == null)
            {
                return;
            }

            property.SetValue(_value, value, null);
        }

        public void Command(string commandName, object parameter)
        {
            if (_value == null) return;
            var cprop = Type.GetProperty(commandName, typeof(ICommand));
            if (cprop == null) return;
            var cval = cprop.GetValue(_value, null) as ICommand;
            if (cval == null) return;
            cval.Execute(parameter);
        }

        public void Command(string commandName)
        {
            Command(commandName, null);
        }

#region property binding
        private void BindingPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "" || e.PropertyName == _propertyBinding.Property)
                ApplyBindingToValue();
        }

        private void ApplyBindingToValue()
        {
            if (_propertyBinding == null) return;
            if (_propertyBinding.Component == null) return;
            if (_prop == null) return;

            var value = uguimvvm.PropertyBinding.GetValue(_propertyBinding, _prop);
            UpdateValue(value);
        }
#endregion

        public void AddDependentProperty(PropertyBinding.PropertyPath prop, PropertyChangedEventHandler handler)
        {
            var dependentProperty = new DependentProperty(prop, handler);
            dependentProperty.Prop.AddHandler(_value, dependentProperty.Handler);

            _dependents.Add(dependentProperty);
        }

        public class DependentProperty
        {
            public PropertyBinding.PropertyPath Prop { get; private set; }
            public PropertyChangedEventHandler Handler { get; private set; }

            public DependentProperty(PropertyBinding.PropertyPath prop, PropertyChangedEventHandler handler)
            {
                Prop = prop;
                Handler = handler;
            }
        }
    }
}
