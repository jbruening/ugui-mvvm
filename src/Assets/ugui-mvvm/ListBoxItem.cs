using UnityEngine.UI;

namespace uguimvvm
{
    public class ListBoxItem : Selectable
    {
        public bool IsSelected()
        {
            return currentSelectionState == SelectionState.Highlighted || currentSelectionState == SelectionState.Pressed;
        }
    }
}
