#region GENERATED. Regenerate by menu item Assets/Generate PropertyPathGen
using uguimvvm;
using ppa = uguimvvm.PropertyPathAccessors;

class PropertyPathGen
{
[UnityEngine.RuntimeInitializeOnLoadMethod]
static void Register()
{
  ppa.Register(
    new[]
    {
        INPCBinding.PropertyPath.GetProperty(typeof(UnityEngine.UI.InputField), "text")
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
        INPCBinding.PropertyPath.GetProperty(typeof(TestViewModel), "TestProperty")
    },
    obj => 
    {
        if(obj == null) return null; 
        var v0 = ((TestViewModel)obj).TestProperty;
        return v0;
    },
    (obj, value) =>
    {
        ((TestViewModel)obj).TestProperty = (System.String)value;
    });

  ppa.Register(
    new[]
    {
        INPCBinding.PropertyPath.GetProperty(typeof(UnityEngine.UI.Text), "text")
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
        INPCBinding.PropertyPath.GetProperty(typeof(UnityEngine.UI.Toggle), "isOn")
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
        INPCBinding.PropertyPath.GetProperty(typeof(TestViewModel), "Selected"),
        INPCBinding.PropertyPath.GetProperty(typeof(ChildViewModel), "CanSomething")
    },
    obj => 
    {
        if(obj == null) return null; 
        var v0 = ((TestViewModel)obj).Selected;
        return v0.CanSomething;
    },
    (obj, value) =>
    {
        var v0 = ((TestViewModel)obj).Selected;
        v0.CanSomething = (System.Boolean)value;
    });

  ppa.Register(
    new[]
    {
        INPCBinding.PropertyPath.GetProperty(typeof(UnityEngine.Behaviour), "enabled")
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
        INPCBinding.PropertyPath.GetProperty(typeof(uguimvvm.ItemsControl), "ItemsSource")
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
        INPCBinding.PropertyPath.GetProperty(typeof(TestViewModel), "Children")
    },
    obj => 
    {
        if(obj == null) return null; 
        var v0 = ((TestViewModel)obj).Children;
        return v0;
    },
    (obj, value) =>
    {
        ((TestViewModel)obj).Children = (uguimvvm.ObservableCollection<ChildViewModel>)value;
    });

  ppa.Register(
    new[]
    {
        INPCBinding.PropertyPath.GetProperty(typeof(uguimvvm.Primitives.Selector), "Selected")
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
        INPCBinding.PropertyPath.GetProperty(typeof(TestViewModel), "Selected")
    },
    obj => 
    {
        if(obj == null) return null; 
        var v0 = ((TestViewModel)obj).Selected;
        return v0;
    },
    (obj, value) =>
    {
        ((TestViewModel)obj).Selected = (ChildViewModel)value;
    });

  ppa.Register(
    new[]
    {
        INPCBinding.PropertyPath.GetProperty(typeof(uguimvvm.ItemsControl), "ItemsSource")
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
        INPCBinding.PropertyPath.GetProperty(typeof(uguimvvm.Primitives.Selector), "Selected")
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