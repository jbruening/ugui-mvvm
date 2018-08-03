using System;
using System.ComponentModel;
using System.Reflection;
// MRMW_CHANGE - BEGIN: Replacing uguimvvm.ICommand with ICommand
using System.Windows.Input;
// MRMW_CHANGE - END: Replacing uguimvvm.ICommand with ICommand
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Component = UnityEngine.Component;

namespace uguimvvm
{
    public class CommandBinding : MonoBehaviour
    {
        [SerializeField]
        Component _view;

// MRMW_CHANGE - BEGIN: supress CS0169 warning
#pragma warning disable 0169
// MRMW_CHANGE - END: supress CS0169 warning

        [SerializeField]
        string _viewEvent;

// MRMW_CHANGE - BEGIN: supress CS0169 warning
#if !UNITY_EDITOR
#pragma warning restore 0169
#endif
// MRMW_CHANGE - END: supress CS0169 warning

        [SerializeField]
        INPCBinding.ComponentPath _viewModel = null;

        [SerializeField]
        private BindingParameter _parameter = null;

        INPCBinding.PropertyPath _vmProp;
        private ICommand _command;

//MRMW_CHANGE - BEGIN: Used to bind voice commands for now, looking at adding voice option to command binding
        public INPCBinding.ComponentPath ViewModel
        {
            get
            {
                return _viewModel;
            }
        }
//MRMW_CHANGE - END: Used to bind voice commands for now, looking at adding voice option to command binding

        void Reset()
        {
            var context = gameObject.GetComponentInParent(typeof(DataContext)) as DataContext;
            if (context != null)
                _viewModel = new INPCBinding.ComponentPath { Component = context };

            _view = gameObject.GetComponent<UIBehaviour>();
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
            if (_viewModel.Component is INotifyPropertyChanged)
                (_viewModel.Component as INotifyPropertyChanged).PropertyChanged -= OnPropertyChanged;
            if (_command != null)
                _command.CanExecuteChanged -= CommandOnCanExecuteChanged;
        }

        private void FigureBindings()
        {
            Type vmtype;
            if (_viewModel.Component is DataContext)
                vmtype = (_viewModel.Component as DataContext).Type;
            else
                vmtype = _viewModel.Component.GetType();

            _vmProp = new INPCBinding.PropertyPath(_viewModel.Property, vmtype, true);

            // MRMW_CHANGE - BEGIN: Improve handling of invalid DataContext types
            if (!_vmProp.IsValid)
            {
                Debug.LogErrorFormat("CommandBinding: Invalid ViewModel property in \"{0}\".",
                    gameObject.GetParentNameHierarchy());
            }
            // MRMW_CHANGE - END: Improve handling of invalid DataContext types

            if (!typeof (ICommand).IsAssignableFrom(_vmProp.PropertyType))
                _vmProp = null;

            if (_vmProp == null)
            {
                Debug.LogWarningFormat("No property named {0} of type ICommand exists in {1}", _viewModel.Property, vmtype);
            }

            if (_vmProp != null && _vmProp.IsValid)
            {
                if (_viewModel.Component is INotifyPropertyChanged)
                    (_viewModel.Component as INotifyPropertyChanged).PropertyChanged += OnPropertyChanged;

                BindCommand();
            }
        }

        private object GetVmValue()
        {
            if (_viewModel.Component is DataContext)
                return (_viewModel.Component as DataContext).GetValue(_vmProp);
            return _vmProp.GetValue(_viewModel.Component, null);
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
                propertyChangedEventArgs.PropertyName != _viewModel.Property)
                return;

            BindCommand();
        }

        private void CommandOnCanExecuteChanged(object sender, EventArgs eventArgs)
        {
            //enable if we don't have a command, or whatever the command's canexecute is.
            // MRMW_CHANGE - BEGIN: Adding Binding support for Command Paramater
            SetViewEnabledState(_command == null || _command.CanExecute(GetCommandParamater()));
            // MRMW_CHANGE - END: Adding Binding support for Command Paramater
        }

        private void SetViewEnabledState(bool state)
        {
            if (_view is Selectable)
                (_view as Selectable).interactable = state;
        }

        public void ExecuteCommand()
        {
            if (_command == null) return;
            // MRMW_CHANGE - BEGIN: Adding Binding support for Command Paramater
            _command.Execute(GetCommandParamater());
            // MRMW_CHANGE - END: Adding Binding support for Command Paramater
        }

        // MRMW_CHANGE - BEGIN: Adding Binding support for Command Paramater
        private object GetCommandParamater()
        {
            object val = null;
            if (_parameter.Type == BindingParameterType.Binding)
            {
                var param = (string)_parameter.GetValue();

                Type vmtype = _viewModel.Component is DataContext ?
                                        (_viewModel.Component as DataContext).Type
                                        : _viewModel.Component.GetType();

                var paramPath = new INPCBinding.PropertyPath(param, vmtype, true);

                val = _viewModel.Component is DataContext ?
                            (_viewModel.Component as DataContext).GetValue(paramPath)
                            : paramPath.GetValue(_viewModel.Component, null);
            }
            else if (_parameter.Type == BindingParameterType.ViewBinding)
            {
                var param = (string)_parameter.GetValue();

                Type vtype = _view.GetType();

                var paramPath = new INPCBinding.PropertyPath(param, vtype, true);

                val = paramPath.GetValue(_view, null);
            }
            else
            {
                val = _parameter.GetValue();
            }

            return val;
        }
        // MRMW_CHANGE - END: Adding Binding support for Command Paramater
    }

    [Serializable]
    public class BindingParameter
    {
        public BindingParameterType Type;
        public UnityEngine.Object ObjectReference;
        public string String;
        public int Int;
        public float Float;
        public bool Bool;
        // MRMW_CHANGE - BEGIN: Adding Binding support for Command Paramater
        public string PropertyPath;
        // MRMW_CHANGE - END: Adding Binding support for Command Paramater

        public object GetValue()
        {
            switch(Type)
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
                // MRMW_CHANGE - BEGIN: Adding Binding support for Command Paramater
                case BindingParameterType.Binding:
                case BindingParameterType.ViewBinding:
                    return PropertyPath;
                // MRMW_CHANGE - END: Adding Binding support for Command Paramater
                default:
                    return null;
            }
        }
    }
    public enum BindingParameterType
    {
        None,
        ObjectReference,
        String,
        Int,
        Float,
        Bool,
        // MRMW_CHANGE - BEGIN: Adding Binding support for Command Paramater
        Binding,
        ViewBinding,
        // MRMW_CHANGE - END: Adding Binding support for Command Paramater
    }
}
