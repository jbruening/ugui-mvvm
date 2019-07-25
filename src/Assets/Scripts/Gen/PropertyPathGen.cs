#region GENERATED. Regenerate by menu item Assets/Generate PropertyPathGen
using PropertyBinding = uguimvvm.PropertyBinding;
using ppa = uguimvvm.PropertyPathAccessors;
#if UNITY_WSA || !NET_LEGACY
using System.Collections.ObjectModel;
#else
using uguimvvm;
#endif

class PropertyPathGen
{
[UnityEngine.RuntimeInitializeOnLoadMethod]
static void Register()
{
  ppa.Register(
    new[]
    {
        PropertyBinding.PropertyPath.GetProperty(typeof(UnityEngine.UI.InputField), "text")
    },
    obj => 
    {
        if(obj == null) return null; 
        var v0 = ((UnityEngine.UI.InputField)obj).text;
        return v0;
    },
    (obj, value) =>
    {
        ((UnityEngine.UI.InputField)obj).text = (System.String)value;
    });

  ppa.Register(
    new[]
    {
        PropertyBinding.PropertyPath.GetProperty(typeof(UnityEngine.UI.Text), "text")
    },
    obj => 
    {
        if(obj == null) return null; 
        var v0 = ((UnityEngine.UI.Text)obj).text;
        return v0;
    },
    (obj, value) =>
    {
        ((UnityEngine.UI.Text)obj).text = (System.String)value;
    });

  ppa.Register(
    new[]
    {
        PropertyBinding.PropertyPath.GetProperty(typeof(UnityEngine.UI.Toggle), "isOn")
    },
    obj => 
    {
        if(obj == null) return null; 
        var v0 = ((UnityEngine.UI.Toggle)obj).isOn;
        return v0;
    },
    (obj, value) =>
    {
        ((UnityEngine.UI.Toggle)obj).isOn = (System.Boolean)value;
    });

  ppa.Register(
    new[]
    {
        PropertyBinding.PropertyPath.GetProperty(typeof(UnityEngine.Behaviour), "enabled")
    },
    obj => 
    {
        if(obj == null) return null; 
        var v0 = ((UnityEngine.Behaviour)obj).enabled;
        return v0;
    },
    (obj, value) =>
    {
        ((UnityEngine.Behaviour)obj).enabled = (System.Boolean)value;
    });

  ppa.Register(
    new[]
    {
        PropertyBinding.PropertyPath.GetProperty(typeof(uguimvvm.ItemsControl), "ItemsSource")
    },
    obj => 
    {
        if(obj == null) return null; 
        var v0 = ((uguimvvm.ItemsControl)obj).ItemsSource;
        return v0;
    },
    (obj, value) =>
    {
        ((uguimvvm.ItemsControl)obj).ItemsSource = (System.Collections.IEnumerable)value;
    });

  ppa.Register(
    new[]
    {
        PropertyBinding.PropertyPath.GetProperty(typeof(uguimvvm.Primitives.Selector), "Selected")
    },
    obj => 
    {
        if(obj == null) return null; 
        var v0 = ((uguimvvm.Primitives.Selector)obj).Selected;
        return v0;
    },
    (obj, value) =>
    {
        ((uguimvvm.Primitives.Selector)obj).Selected = (System.Object)value;
    });

  ppa.Register(
    new[]
    {
        PropertyBinding.PropertyPath.GetProperty(typeof(uguimvvm.ItemsControl), "ItemsSource")
    },
    obj => 
    {
        if(obj == null) return null; 
        var v0 = ((uguimvvm.ItemsControl)obj).ItemsSource;
        return v0;
    },
    (obj, value) =>
    {
        ((uguimvvm.ItemsControl)obj).ItemsSource = (System.Collections.IEnumerable)value;
    });

  ppa.Register(
    new[]
    {
        PropertyBinding.PropertyPath.GetProperty(typeof(uguimvvm.Primitives.Selector), "Selected")
    },
    obj => 
    {
        if(obj == null) return null; 
        var v0 = ((uguimvvm.Primitives.Selector)obj).Selected;
        return v0;
    },
    (obj, value) =>
    {
        ((uguimvvm.Primitives.Selector)obj).Selected = (System.Object)value;
    });

  ppa.Initialize();
}
}
#endregion