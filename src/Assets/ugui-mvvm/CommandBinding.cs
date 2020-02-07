using System;
using System.ComponentModel;
using System.Reflection;
#if UNITY_WSA || !NET_LEGACY
using System.Windows.Input;
#endif
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Component = UnityEngine.Component;

namespace uguimvvm
{
    /// <summary>
    /// Defines a binding that connects an <see cref="ICommand"/> source to a supported target.
    /// </summary>
    public class CommandBinding : MonoBehaviour
    {
        [SerializeField]
        [FormerlySerializedAs("_view")]
        Component _target;

#pragma warning disable 0169
        [SerializeField]
        [FormerlySerializedAs("_viewEvent")]
        string _targetEvent;

#if !UNITY_EDITOR
#pragma warning restore 0169
#endif
        [SerializeField]
        [FormerlySerializedAs("_viewModel")]
        PropertyBinding.ComponentPath _source = null;

        [SerializeField]
        private BindingParameter _parameter = null;

        /// <summary>
        /// Parameter to be passed to the <see cref="ICommand"/>'s <see cref="ICommand.Execute(object)"/> and <see cref="ICommand.CanExecute(object)"/> methods when they are executed.
        /// Defaults to the serialized values associated with this <see cref="CommandBinding"/>, but overridable at runtime.
        /// </summary>
        /// <remarks>
        /// This is intended for use with custom parameter types to pass at runtime by binding this property to a <see cref="PropertyBinding"/>.
        /// </remarks>
        public object Parameter
        {
            get
            {
                return _runtimeParameter == null ? _parameter.GetValue() : _runtimeParameter;
            }
            set
            {
                if (_runtimeParameter != value)
                {
                    _runtimeParameter = value;
                    SetViewEnabledState(_command == null || _command.CanExecute(_runtimeParameter));
                }
            }
        }
        private object _runtimeParameter = null;

        PropertyBinding.PropertyPath _vmProp;
        private ICommand _command;

        /// <summary>
        /// Deprecated - use <see cref="Source"/> instead.
        /// </summary>
        [Obsolete("Use the Source property.")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public PropertyBinding.ComponentPath ViewModel
        {
            get
            {
                return this.Source;
            }
        }

        /// <summary>
        /// The source from which the <see cref="ICommand"/> can be fetched.
        /// </summary>
        public PropertyBinding.ComponentPath Source
        {
            get
            {
                return _source;
            }
        }

        void Reset()
        {
            var context = gameObject.GetComponentInParent(typeof(DataContext)) as DataContext;
            if (context != null)
                _source = new PropertyBinding.ComponentPath { Component = context };

            _target = gameObject.GetComponent<UIBehaviour>();
        }

        void Awake()
        {
            FigureBindings();
        }

        void OnDestroy()
        {
            ClearBindings();
        }

        private void ClearBindings()
        {
            if (_source.Component is INotifyPropertyChanged)
                (_source.Component as INotifyPropertyChanged).PropertyChanged -= OnPropertyChanged;
            if (_command != null)
                _command.CanExecuteChanged -= CommandOnCanExecuteChanged;
        }

        private void FigureBindings()
        {
            Type vmtype;
            if (_source.Component is DataContext)
                vmtype = (_source.Component as DataContext).Type;
            else
                vmtype = _source.Component.GetType();

            _vmProp = new PropertyBinding.PropertyPath(_source.Property, vmtype, true);

            if (!_vmProp.IsValid)
            {
                Debug.LogErrorFormat(this, "CommandBinding: Invalid Source property in \"{0}\".",
                    gameObject.GetParentNameHierarchy());
            }

            if (!typeof(ICommand).IsAssignableFrom(_vmProp.PropertyType))
                _vmProp = null;

            if (_vmProp == null)
            {
                Debug.LogWarningFormat(this, "No property named {0} of type ICommand exists in {1}", _source.Property, vmtype);
            }

            if (_vmProp != null && _vmProp.IsValid)
            {
                if (_source.Component is INotifyPropertyChanged)
                    (_source.Component as INotifyPropertyChanged).PropertyChanged += OnPropertyChanged;

                BindCommand();
            }
        }

        private object GetVmValue()
        {
            if (_source.Component is DataContext)
                return (_source.Component as DataContext).GetValue(_vmProp);
            return _vmProp.GetValue(_source.Component, null);
        }

        private void BindCommand()
        {
            var ncommand = GetVmValue() as ICommand;

            if (_command != null)
                _command.CanExecuteChanged -= CommandOnCanExecuteChanged;

            _command = ncommand;

            if (_command != null)
            {
                _command.CanExecuteChanged += CommandOnCanExecuteChanged;
            }
            CommandOnCanExecuteChanged(this, new EventArgs());
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (propertyChangedEventArgs.PropertyName != "" &&
                propertyChangedEventArgs.PropertyName != _source.Property)
                return;

            BindCommand();
        }

        private void CommandOnCanExecuteChanged(object sender, EventArgs eventArgs)
        {
            //enable if we don't have a command, or whatever the command's CanExecute is.
            SetViewEnabledState(_command == null || _command.CanExecute(Parameter));
        }

        private void SetViewEnabledState(bool state)
        {
            if (_target is Selectable)
                (_target as Selectable).interactable = state;
        }

        /// <summary>
        /// Executes the <see cref="ICommand"/> associated with this binding.
        /// </summary>
        public void ExecuteCommand()
        {
            if (_command == null) return;
            _command.Execute(Parameter);
        }
    }

    /// <summary>
    /// Serializable class for defining an optional <see cref="CommandBinding.Parameter"/>.
    /// </summary>
    [Serializable]
    public class BindingParameter
    {
        /// <summary>
        /// The type of value this class should deserialize for the <see cref="CommandBinding.Parameter"/>.
        /// </summary>
        public BindingParameterType Type;

        /// <summary>
        /// The value to be used when <see cref="Type"/> is <see cref="BindingParameterType.ObjectReference"/>.
        /// </summary>
        public UnityEngine.Object ObjectReference;

        /// <summary>
        /// The value to be used when <see cref="Type"/> is <see cref="BindingParameterType.String"/>.
        /// </summary>
        public string String;

        /// <summary>
        /// The value to be used when <see cref="Type"/> is <see cref="BindingParameterType.Int"/>.
        /// </summary>
        public int Int;

        /// <summary>
        /// The value to be used when <see cref="Type"/> is <see cref="BindingParameterType.Float"/>.
        /// </summary>
        public float Float;

        /// <summary>
        /// The value to be used when <see cref="Type"/> is <see cref="BindingParameterType.Bool"/>.
        /// </summary>
        public bool Bool;

        /// <summary>
        /// Fetches the appropriate deserialized value to use for the <see cref="CommandBinding.Parameter"/>.
        /// </summary>
        /// <returns>The deserialized value.</returns>
        public object GetValue()
        {
            switch (Type)
            {
                case BindingParameterType.Bool:
                    return Bool;
                case BindingParameterType.Float:
                    return Float;
                case BindingParameterType.Int:
                    return Int;
                case BindingParameterType.ObjectReference:
                    return ObjectReference;
                case BindingParameterType.String:
                    return String;
                default:
                    return null;
            }
        }
    }

    /// <summary>
    /// The types of values supported by the <see cref="BindingParameter"/> class for serialization.
    /// </summary>
    public enum BindingParameterType
    {
        /// <summary>Value for no type</summary>
        None,
        /// <summary>Value for type of <see cref="UnityEngine.Object"/></summary>
        ObjectReference,
        /// <summary>Value for type of <see cref="string"/></summary>
        String,
        /// <summary>Value for type of <see cref="int"/></summary>
        Int,
        /// <summary>Value for type of <see cref="float"/></summary>
        Float,
        /// <summary>Value for type of <see cref="bool"/></summary>
        Bool,
    }
}
