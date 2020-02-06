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
    /// <summary>
    /// Acts as a component to configure bindings against, handling initialization and update logic.
    /// </summary>
    public class DataContext : MonoBehaviour, INotifyPropertyChanged
    {
#pragma warning disable 0649
        [SerializeField]
        string _type;
#pragma warning restore 0649

        private Type _rtype;

        /// <summary>
        /// The expected <see cref="System.Type"/> of <see cref="Value"/>.
        /// </summary>
        public Type Type
        {
            get { return _rtype ?? (_rtype = Type.GetType(_type)); }
        }

        private object _value;

        /// <summary>
        /// The <see cref="PropertyBinding.Source"/> to be used for <see cref="uguimvvm.PropertyBinding"/>s.
        /// </summary>
        public object Value
        {
            get { return _value; }
            set { UpdateValue(value); }
        }

        [SerializeField]
        private PropertyBinding.ComponentPath _propertyBinding = null;

        /// <summary>
        /// Optional binding used to initialize <see cref="Value"/> at <see cref="Awake"/> time.
        /// </summary>
        public PropertyBinding.ComponentPath PropertyBinding => _propertyBinding;

        /// <summary>
        /// <see cref="UnityEngine.Component"/> portion of the <see cref="PropertyBinding"/>.
        /// </summary>
        public Component Component { get { return _propertyBinding?.Component; } }
        private PropertyBinding.PropertyPath _prop;

        [Tooltip("Instantiate the type on awake. This will not work for UnityEngine.Object types")]
        [SerializeField]
        bool _instantiateOnAwake = false;

        private readonly List<DependentProperty> _dependents = new List<DependentProperty>();

        void Awake()
        {
            if (_propertyBinding != null && _propertyBinding.Component != null)
                _prop = uguimvvm.PropertyBinding.FigureBinding(_propertyBinding, this.ApplyBindingToValue, true);

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

        /// <summary>
        /// Updates <see cref="Value"/> and triggers the updates of all associated <see cref="DependentProperty"/> instances.
        /// </summary>
        /// <param name="value">The new value to assign to <see cref="Value"/>.</param>
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
                    item.Prop.TriggerHandler();
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

        /// <inheritdoc />
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

        /// <summary>
        /// Executes the requested <see cref="ICommand"/> defined on this <see cref="Value"/>.
        /// </summary>
        /// <param name="commandName">The name of the property on the <see cref="Value"/> object that returns the desired command</param>
        /// <param name="parameter">The parameter to pass to the <see cref="ICommand.Execute(object)"/></param>
        public void Command(string commandName, object parameter)
        {
            if (_value == null) return;
            var cprop = Type.GetProperty(commandName, typeof(ICommand));
            if (cprop == null) return;
            var cval = cprop.GetValue(_value, null) as ICommand;
            if (cval == null) return;
            cval.Execute(parameter);
        }

        /// <summary>
        /// Executes the requested <see cref="ICommand"/> defined on this <see cref="Value"/>.
        /// </summary>
        /// <param name="commandName">The name of the property on the <see cref="Value"/> object that returns the desired command</param>
        public void Command(string commandName)
        {
            Command(commandName, null);
        }

        #region property binding
        private void ApplyBindingToValue()
        {
            if (_propertyBinding == null) return;
            if (_propertyBinding.Component == null) return;
            if (_prop == null) return;

            var value = uguimvvm.PropertyBinding.GetValue(_propertyBinding, _prop);
            UpdateValue(value);
        }
        #endregion

        /// <summary>
        /// Registers a new property as dependent on this <see cref="DataContext"/> so that this <see cref="DataContext"/> takes responsibility for notifying the property of value changes.
        /// </summary>
        /// <param name="prop">The path to the property from this <see cref="DataContext"/>'s <see cref="Value"/> on which the binding depends.</param>
        /// <param name="handler">The method to invoke when the value of <see cref="Value"/>, or any of the sub properties in <paramref name="prop"/>'s values, change.</param>
        public void AddDependentProperty(PropertyBinding.PropertyPath prop, Action handler)
        {
            var dependentProperty = new DependentProperty(prop, handler);
            dependentProperty.Prop.AddHandler(_value, dependentProperty.Handler);

            _dependents.Add(dependentProperty);
        }

        /// <summary>
        /// Represents a property that is dependent on a <see cref="DataContext"/> due to a <see cref="uguimvvm.PropertyBinding"/>.
        /// </summary>
        public class DependentProperty
        {
            /// <summary>
            /// The path to the property from a <see cref="DataContext"/>'s <see cref="Value"/> on which the binding depends.
            /// </summary>
            public PropertyBinding.PropertyPath Prop { get; private set; }

            /// <summary>
            /// The method to invoke when the value of <see cref="Value"/>, or any of the sub properties in <see cref="Prop"/>'s values, change.
            /// </summary>
            public Action Handler { get; private set; }

            /// <summary>
            /// Creates a new instance.
            /// </summary>
            /// <param name="prop">The path to the property from a <see cref="DataContext"/>'s <see cref="Value"/> on which the binding depends.</param>
            /// <param name="handler">The method to invoke when the value of <see cref="Value"/>, or any of the sub properties in <paramref name="prop"/>'s values, change.</param>
            public DependentProperty(PropertyBinding.PropertyPath prop, Action handler)
            {
                Prop = prop;
                Handler = handler;
            }
        }
    }
}
