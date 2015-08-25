using System;
using System.ComponentModel;
using System.Reflection;
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

    void Awake()
    {

    }
    
    public void UpdateValue(object value)
    {
        if (value is INotifyPropertyChanged)
        {
            (value as INotifyPropertyChanged).PropertyChanged += ValuePropertyChanged;
        }

        if (_value is INotifyPropertyChanged)
        {
            (_value as INotifyPropertyChanged).PropertyChanged -= ValuePropertyChanged;
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
            Debug.LogErrorFormat("Cannot get value for {0}. DataContext on {1} has no value", property, name);
            return null;
        }

        return property.GetValue(_value, null);
    }

    internal void SetValue(object value, PropertyInfo property)
    {
        if (_value == null)
        {
            Debug.LogErrorFormat("Cannot set value for {0}. DataContext on {1} has no value", property, name);
            return;
        }

        property.SetValue(_value, value, null);
    }
}