using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace uguimvvm
{
    /// <summary>
    /// Represents the container for an item in a <see cref="DropDownMenu"/> control.
    /// </summary>
    public class DropDownItem
    {
        /// <summary>
        /// The string to use to represent this item to the user.
        /// </summary>
        public string Label { get; set; }

        /// <summary>
        /// The method to execute when this item is selected.
        /// </summary>
        public Action Command { get; set; }

        /// <summary>
        /// A flag indicating if this item is currently selected.
        /// </summary>
        public bool IsSelected { get; set; }
    }

    /// <summary>
    /// Represents a drop-down list box that allows users to select an item from a list.
    /// </summary>
    public class DropDownMenu
    {
        private readonly List<DropDownItem> _dropDownItems = new List<DropDownItem>();
        private int _selectedItem = -1;

        /// <summary>
        /// The number of items in the list.
        /// </summary>
        public int ItemCount => _dropDownItems.Count;

        /// <summary>
        /// The index of the currently selected item.
        /// </summary>
        public int SelectedIndex => _selectedItem;

        /// <summary>
        /// Adds an item to the end of the list.
        /// </summary>
        /// <param name="item">The item to append to the list.</param>
        public void Add(DropDownItem item)
        {
            if (item.IsSelected)
            {
                _selectedItem = _dropDownItems.Count;
            }

            _dropDownItems.Add(item);
        }

        /// <summary>
        /// Renders the dropdown menu.  If an item is selected, synchronously executes the action.
        /// </summary>
        /// <param name="position">The position at which to render the dropdown.</param>
        public void OnGUI(Rect position)
        {
            OnGUI(dropDownContent => EditorGUI.Popup(position, _selectedItem, dropDownContent));
        }

        /// <summary>
        ///  Renders the dropdown menu.  If an item is selected, synchronously executes the action.
        /// </summary>
        /// <param name="label">The label to render along with the dropdown.</param>
        public void OnGUI(string label)
        {
            OnGUI(dropDownContent => EditorGUILayout.Popup(new GUIContent(label), _selectedItem, dropDownContent));
        }

        private void OnGUI(Func<GUIContent[], int> showPopup)
        {
            var dropDownContent = new GUIContent[_dropDownItems.Count];
            for (int i = 0; i < dropDownContent.Length; i++)
            {
                var item = _dropDownItems[i];

                if (item.Label == null)
                {
                    dropDownContent[i] = new GUIContent();
                }
                else
                {
                    dropDownContent[i] = new GUIContent(item.Label);
                }
            }

            var clickedIndex = showPopup(dropDownContent);

            if (clickedIndex != _selectedItem)
            {
                _dropDownItems[clickedIndex].Command?.Invoke();
            }
        }
    }
}
