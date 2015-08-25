using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

class ABehaviourViewModel : MonoBehaviour, INotifyPropertyChanged
{
    protected void SetProperty<T>(string propertyName, ref T backingStore, T value)
    {
        if (EqualityComparer<T>.Default.Equals(backingStore, value)) return;
        backingStore = value;
        RaisePropertyChanged(propertyName);
    }

    protected void RaisePropertyChanged(string path)
    {
        if (PropertyChanged == null) return;
        PropertyChanged(this, new PropertyChangedEventArgs(path));
    }

    public event PropertyChangedEventHandler PropertyChanged;
}
