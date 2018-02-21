using UnityEngine;
using uguimvvm;

class TestViewModel : ABehaviourViewModel, IParentVm
{
    string _testProperty;

    public string TestProperty
    {
        get { return _testProperty; }
        set { SetProperty("TestProperty", ref _testProperty, value); }
    }

    private ChildViewModel _selected;
    public ChildViewModel Selected
    {
        get { return _selected; }
        set { SetProperty("Selected", ref _selected, value); }
    }

    public ObservableCollection<ChildViewModel> Children { get; set; }

    public TestViewModel()
    {
        Children = new ObservableCollection<ChildViewModel>();
    }

    public void AddChild()
    {
        Children.Add(new ChildViewModel(this));
    }

    public void RemoveChild()
    {
        if (Children.Count == 0) return;
        Children.RemoveAt(Children.Count - 1);
    }

    public void RemoveChild(ChildViewModel child)
    {
        Children.Remove(child);
    }

    public void MoveSelectedToFirst()
    {
        if (Children.Count == 0) return;
        if (Selected == null) return;
        var selIdx = Children.IndexOf(Selected);
        Children.Move(selIdx, 0);
    }
}
