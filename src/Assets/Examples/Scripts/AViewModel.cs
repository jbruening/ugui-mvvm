using System.Collections.Generic;
using System.ComponentModel;

class AViewModel : INotifyPropertyChanged
{
    /// <summary>
    /// Set the specified property and raise the PropertyChanged event if its different
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="propertyName"></param>
    /// <param name="backingStore"></param>
    /// <param name="value"></param>
    /// <returns>true, if the property changed. Otherwise false</returns>
    protected bool SetProperty<T>(string propertyName, ref T backingStore, T value)
    {
        if (EqualityComparer<T>.Default.Equals(backingStore, value)) return false;
        backingStore = value;
        RaisePropertyChanged(propertyName);
        return true;
    }

    protected void RaisePropertyChanged(string path)
    {
        if (PropertyChanged == null) return;
        PropertyChanged(this, new PropertyChangedEventArgs(path));
    }

    public event PropertyChangedEventHandler PropertyChanged;
}
