using UnityEngine.UI;

namespace uguimvvm
{
    class ListBoxItem : Selectable
    {
        public bool IsSelected()
        {
            return currentSelectionState == SelectionState.Highlighted || currentSelectionState == SelectionState.Pressed;
        }
    }
}
