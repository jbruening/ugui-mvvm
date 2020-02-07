namespace uguimvvm
{
    /// <summary>
    /// Describes how data updates in a binding are initiated.
    /// </summary>
    public enum BindingUpdateTrigger
    {
        /// <summary>Updates will not be prompted.</summary>
        None,
        /// <summary>Updates prompted by a <see cref="System.ComponentModel.INotifyPropertyChanged.PropertyChanged"/> event.</summary>
        PropertyChangedEvent,
        /// <summary>Updates prompted by a <see cref="UnityEngine.Events.UnityEvent"/>.</summary>
        UnityEvent,
    }
}
