# uGUI-MVVM
Unity GUI (the new one) databinding done via the standard INotifyPropertyChanged/INotifyCollectionChanged interfaces that are used in WPF applications.

## Basic Use
Opening the example scene will show a mix of the ways the binding works.  There are two ways to create your viewmodels:
 - As MonoBehaviour-derived components
 - As POCOs, bound to via DataContext and CommandBinding components

### Databinding
 All databinding is done through the INPCBinding component.  One component is needed per bound property.
 - MonoBehaviour viewmodels are referenced in the INPCBinding component. ![DataBinding to MonoBehaviour example](http://i.imgur.com/lrtqkIB.png)
 - POCOs are referenced via their instance in the DataContext component (assign the DataContext to the INPCBinding viewmodel field)

### Command binding
 Command binding is only necessary if you're using POCO viewmodels with DataContext. Otherwise, you can just do normal binding on the button's events to the viewmodel ![CommandBinding to POCO DataContext example](http://i.imgur.com/Emx3c45.png)

### Collections  
  Collections can be done via ItemsControl components. The viewmodel collection must be an IEnumerable, with optionally implementing INotifyCollectionChanged.  Currently, collection changes are only handled via full resets.  
  As it is difficult to easily create/remove components from a collection, it is recommended you use POCOs as collection items. ![ItemsControl example](http://i.imgur.com/hQcMymS.png)
