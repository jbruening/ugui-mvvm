using System;
using System.Reflection;
using UnityEngine;

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
            //todo: do we need to clean up unity bindings?
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
        }

        private object GetVmValue()
        {
            if (_viewModel.Component is DataContext)
                return (_viewModel.Component as DataContext).GetValue(_vmProp);
            return _vmProp.GetValue(_viewModel.Component, null);
        }

        public void ExecuteCommand()
        {
            if (_vmProp == null) return;

            var command = GetVmValue() as ICommand;
            if (command == null) return;

            command.Execute(_parameter);
        }
    }
}
