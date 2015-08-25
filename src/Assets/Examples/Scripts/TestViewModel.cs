using UnityEngine;
using System.Collections;
using System.ComponentModel;
using uguimvvm;

class TestViewModel : ABehaviourViewModel
{
    string _testProperty;
    public string TestProperty
    {
        get { return _testProperty; }
        set { SetProperty("TestProperty", ref _testProperty, value); }
    }

    public ObservableCollection<ChildViewModel> Children { get; set; }

    public TestViewModel()
    {
        Children = new ObservableCollection<ChildViewModel>();
    }

    public void AddChild()
    {
        Children.Add(new ChildViewModel());
    }

    public void RemoveChild()
    {
        if (Children.Count == 0) return;
        Children.RemoveAt(Children.Count - 1);
    }
}
