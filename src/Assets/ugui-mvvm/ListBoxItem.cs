using UnityEngine.UI;

namespace uguimvvm
{
    /// <summary>
    /// Represent the container for an item in a <see cref="ListBox"/> control.
    /// </summary>
    public class ListBoxItem : Selectable
    {
        /// <summary>
        /// Gets a value indicating whether the item is in a selected state.
        /// </summary>
        public bool IsSelected()
        {
            return currentSelectionState == SelectionState.Highlighted || currentSelectionState == SelectionState.Pressed;
        }
    }
}
