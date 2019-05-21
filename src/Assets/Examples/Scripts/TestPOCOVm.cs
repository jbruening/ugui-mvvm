using uguimvvm;
#if UNITY_WSA || !NET_LEGACY
using System.Collections.ObjectModel;
#endif

class TestPOCOVm : AViewModel, IParentVm
{
    string _testString;
    public string TestString
    {
        get { return _testString; }
        set { SetProperty("TestString", ref _testString, value); }
    }

    private bool _testBool;
    public bool TestBool
    {
        get { return _testBool; }
        set { SetProperty("TestBool", ref _testBool, value); }
    }

    private bool _canModifyChildren;
    public bool CanModifyChildren
    {
        get { return _canModifyChildren; }
        set
        {
            if (SetProperty("CanModifyChildren", ref _canModifyChildren, value))
            {
                AddChild.RaiseCanExecuteChanged();
                RemoveChild.RaiseCanExecuteChanged();
            }
        }
    }

    private bool _canSaySomething;
    public bool CanSaySomething
    {
        get { return _canSaySomething; }
        set
        {
            if (SetProperty("CanSaySomething", ref _canSaySomething, value))
                SaySomething.RaiseCanExecuteChanged();
        }
    }

    public ObservableCollection<ChildViewModel> Children { get; private set; }

    public TestPOCOVm()
    {
        Children = new ObservableCollection<ChildViewModel>();
        
        AddChild = new RelayCommand(ImplAddChild, ImplCanModifyChildren);
        RemoveChild = new RelayCommand(ImplRemoveChild, ImplCanModifyChildren);
        SaySomething = new RelayCommand(ImplSaySomething, ImplCanSaySomething);
    }

    public RelayCommand SaySomething { get; private set; }
    public RelayCommand AddChild { get; private set; }
    public RelayCommand RemoveChild { get; private set; }

    bool ImplCanModifyChildren()
    {
        return CanModifyChildren;
    }
    void ImplAddChild()
    {
        Children.Add(new ChildViewModel(this));
    }
    void ImplRemoveChild()
    {
        if (Children.Count == 0) return;
        Children.RemoveAt(Children.Count - 1);
    }

    void ImplSaySomething()
    {
        TestString = "Something!";
    }
    bool ImplCanSaySomething()
    {
        return CanSaySomething;
    }

    void IParentVm.RemoveChild(ChildViewModel child)
    {
        Children.Remove(child);
    }
}

interface IParentVm
{
    void RemoveChild(ChildViewModel child);
}
