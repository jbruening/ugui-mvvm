using System;
using System.ComponentModel;
using System.Reflection;
using uguimvvm.Input;
using UnityEngine;

public class DataContext : MonoBehaviour, INotifyPropertyChanged
{
    [SerializeField]
    string _type;

    private Type _rtype;
    public Type Type
    {
        get { return _rtype ?? (_rtype = Type.GetType(_type)); }
    }

    private object _value;
    public object Value
    {
        get { return _value; }
    }

    [Tooltip("Instantiate the type on awake. This will not work for UnityEngine.Object types")]
    [SerializeField]
    bool _instantiateOnAwake = false;

    void Awake()
    {
        if (!_instantiateOnAwake) return;
        if (Type == null)
        {
            Debug.LogError("Set to instantiate on awake, but type is not defined, or is not valid");
            return;
        }
        if (typeof(UnityEngine.Object).IsAssignableFrom(Type))
        {
            Debug.LogErrorFormat("Cannot automatically instantiate type {0}, as it derives from UnityEngine.Object", Type);
            return;
        }

        UpdateValue(Activator.CreateInstance(Type));
    }
    
    public void UpdateValue(object value)
    {
        if (value == _value) return;

        //subtraction is performed first, in case we're somehow updating with the same value
        if (_value is INotifyPropertyChanged)
        {
            (_value as INotifyPropertyChanged).PropertyChanged -= ValuePropertyChanged;
        }

        if (value is INotifyPropertyChanged)
        {
            (value as INotifyPropertyChanged).PropertyChanged += ValuePropertyChanged;
        }

        _value = value;

        //update all properties
        if (_value != null && PropertyChanged != null)
            PropertyChanged(_value, new PropertyChangedEventArgs(""));
    }

    void OnDestroy()
    {
        if (_value is INotifyPropertyChanged)
        {
            (_value as INotifyPropertyChanged).PropertyChanged -= ValuePropertyChanged;
        }
    }

    void ValuePropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (PropertyChanged == null) return;
        PropertyChanged(sender, e);
    }

    public event PropertyChangedEventHandler PropertyChanged;

    internal object GetValue(PropertyInfo property)
    {
        if (_value == null)
        {
            //Debug.LogErrorFormat("Cannot get value for {0}. DataContext on {1} has no value", property, name);
            return null;
        }

        return property.GetValue(_value, null);
    }

    internal void SetValue(object value, PropertyInfo property)
    {
        if (_value == null)
        {
            //Debug.LogErrorFormat("Cannot set value for {0}. DataContext on {1} has no value", property, name);
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
}