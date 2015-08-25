using UnityEngine;
using System.Collections;
using System.ComponentModel;
using System.Collections.ObjectModel;

class TestViewModel : AViewModel
{
    string _testProperty;
    public string TestProperty
    {
        get { return _testProperty; }
        set { SetProperty("TestProperty", ref _testProperty, value); }
    }

    public ObservableCollection<TestViewModel> Children { get; set; }

    public TestViewModel()
    {
        Children = new ObservableCollection<TestViewModel>();
    }
}
