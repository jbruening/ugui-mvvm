using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace uguimvvm
{
    public class DropDownItem
    {
        public string Label { get; set; }
        public Action Command { get; set; }
        public bool IsSelected { get; set; }
    }

    public class DropDownMenu
    {
        private List<DropDownItem> _dropDownItems = new List<DropDownItem>();
        private int _selectedItem = -1;

        public int ItemCount => _dropDownItems.Count;

        public void Add(DropDownItem item)
        {
            if (item.IsSelected)
            {
                _selectedItem = _dropDownItems.Count;
            }

            _dropDownItems.Add(item);
        }

        /// <summary>
        /// Renders the dropdown menu.  If an item is selected, syncronously executes the action.
        /// </summary>
        /// <param name="position"></param>
        public void OnGUI(Rect position)
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

            var clickedIndex = EditorGUI.Popup(position, _selectedItem, dropDownContent);

            if (clickedIndex != _selectedItem)
            {
                _dropDownItems[clickedIndex].Command?.Invoke();
            }
        }
    }
}
