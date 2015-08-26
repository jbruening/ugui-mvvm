using uguimvvm;

class ChildViewModel : AViewModel
{
    string _testProperty;
    public string TestProperty
    {
        get { return _testProperty; }
        set { SetProperty("TestProperty", ref _testProperty, value); }
    }

    bool _isSomething;
    public bool IsSomething
    {
        get { return _isSomething; }
        set { SetProperty("IsSomething", ref _isSomething, value); }
    }

    public ChildViewModel()
    {
        _testProperty = "";
    }

    public RelayCommand ToggleSomething
    {
        get
        {
            return new RelayCommand(() =>
            {
                IsSomething = !IsSomething;
            });
        }
    }
}