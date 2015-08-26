using System;
using System.ComponentModel;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using Component = UnityEngine.Component;

namespace uguimvvm
{
    public class CommandBinding : MonoBehaviour
    {
        [SerializeField]
        Component _view;
        [SerializeField]
        string _viewEvent;

        [SerializeField]
        INPCBinding.ComponentPath _viewModel = null;

        [SerializeField]
        private object _parameter = null;

        PropertyInfo _vmProp;
        private ICommand _command;

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

            _vmProp = vmtype.GetProperty(_viewModel.Property);
            if (!typeof (ICommand).IsAssignableFrom(_vmProp.PropertyType))
                _vmProp = null;

            if (_vmProp == null)
            {
                Debug.LogWarningFormat("No property named {0} of type ICommand exists in {1}", _viewModel.Property, vmtype);
            }

            if (_vmProp != null)
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
            SetViewEnabledState(_command == null || _command.CanExecute(_parameter));
        }

        private void SetViewEnabledState(bool state)
        {
            if (_view is Selectable)
                (_view as Selectable).interactable = state;
        }

        public void ExecuteCommand()
        {
            if (_command == null) return;

            _command.Execute(_parameter);
        }
    }
}
