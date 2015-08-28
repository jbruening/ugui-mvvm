# ugui-mvvm
Unity GUI (the new one) databinding done via the standard INotifyPropertyChanged/INotifyCollectionChanged interfaces that are used in WPF applications.

## Basic Use
There are two ways to create your viewmodels:
 - As MonoBehaviour-derived components
 - As [POCOs](https://en.wikipedia.org/wiki/Plain_Old_CLR_Object), bound to via DataContext and CommandBinding components

Opening the [example scene](https://github.com/jbruening/ugui-mvvm/tree/master/src/Assets/Examples/Scenes) will show a mix of the ways the binding works

As a reminder, both ways require implementing INotifyPropertyChanged to work. To greatly reduce INPC workload, it is recommended you use [UnityFody](https://github.com/jbruening/UnityFody) with the [PropertyChanged](https://github.com/jbruening/UnityFody/tree/master/src/Assets/Plugins/Editor/PropertyChanged) plugin.

### Referencing components in the binding fields.
As referencing specific components on other objects in unity is pretty much impossible, There is a new menu item that can be accessed by clicking on any component's gear icon -> 'Copy Component Reference'. This will then cause a small square button to the right of component fields of INPCBinding and CommandBinding to show up. Pressing this button will then paste the reference to the component into the field. ![example](http://i.imgur.com/sVXgwIR.png)

## Binding

### Databinding
 All databinding is done through the INPCBinding component.  One component is needed per bound property.
 - MonoBehaviour viewmodels can be directly referenced in the INPCBinding component. ![DataBinding to MonoBehaviour example](http://i.imgur.com/lrtqkIB.png)
 - POCOs are referenced via their instance in the DataContext component (assign the DataContext component to the INPCBinding viewmodel component field)
 
INPCBinding recognizes when a DataContext component is referenced as a viewmodel, and will display the DataContext's referenced type's properties, instead of the DataContext's type properties.

#### Converters
Converters work off of ScriptableObject child classes implementing IValueConverter.  Assign the asset of the scriptableobject (you'll need a separate tool to help you to create them) to the Converter field of INPCBinding

### Command binding
 Command binding is only necessary if you're using DataContext. Otherwise, you can just do normal binding on the button's events to the viewmodel.  Command binding is via properties returning ICommand, just like with WPF.
 
 ICommand's CanExecute and CanExecuteChanged will cause the CommandBinding to modify the view's 'interactable' property (from type Selectable). This is shown in both example scenes with the 'Can Toggle' and 'Can Say Something' toggles.
 ![CommandBinding to POCO DataContext example](http://i.imgur.com/Me30ba3.png)

### Collections
 Collections can be done via ItemsControl components. The viewmodel collection must be an IEnumerable, with optionally implementing INotifyCollectionChanged.

Because the event binding gets set up at compile time, as well as it being harder to create UnityEngine.Object types (cannot simply new()), it is recommended you use POCOs as collection items. ![ItemsControl example](http://i.imgur.com/l4LrN3S.png)
