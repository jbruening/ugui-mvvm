using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using uguimvvm;
using System.Collections.Generic;

[CustomPropertyDrawer(typeof(ComponentReferenceAttribute))]
class ComponentReferenceDrawer : PropertyDrawer
{
    private class DropDownItem
    {
        public string Label { get; set; }
        public Action Command { get; set; }
    }

    public static Component clipboard;

    [MenuItem("CONTEXT/Component/Copy Component Reference")]
    public static void CopyControlReference(MenuCommand command)
    {
        var control = command.context as Component;
        clipboard = control;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        PropertyField(position, property);
    }

    public static void PropertyField(Rect position, SerializedProperty property)
    {
        const float DropDownWidthFraction = 0.4f;
        float dropDownWidth = (position.width - EditorGUIUtility.labelWidth) * DropDownWidthFraction;

        position.width -= dropDownWidth;
        EditorGUI.PropertyField(position, property);

        var dropDownPosition = new Rect(position.xMax, position.y, dropDownWidth, EditorGUIUtility.singleLineHeight);

        var dropDownItems = new List<DropDownItem>();
        int selectedItem = -1;

        if (clipboard != null)
        {
            dropDownItems.Add(new DropDownItem
            {
                Label = $"Paste reference to {clipboard.name} - {clipboard.GetType()}",
                Command = () => { property.objectReferenceValue = clipboard; },
            });
        }
        else
        {
            dropDownItems.Add(new DropDownItem { Label = "Nothing to paste" });
        }

        var component = property.objectReferenceValue as Component;
        if (component != null && component.gameObject != null)
        {
            dropDownItems.Add(new DropDownItem { Label = null });

            var siblingComponents = component.gameObject.GetComponents<Component>();

            foreach (var siblingComponent in siblingComponents)
            {
                bool currentlySelected = siblingComponent == component;

                if (currentlySelected)
                {
                    // This component is currently selected.  Mark it as such.
                    selectedItem = dropDownItems.Count;
                }

                dropDownItems.Add(new DropDownItem
                {
                    Label = $"{siblingComponent.GetType().Name}",
                    Command = () => { property.objectReferenceValue = siblingComponent; },
                });
            }
        }

        var dropDownContent = new GUIContent[dropDownItems.Count];
        for (int i = 0; i < dropDownContent.Length; i++)
        {
            var item = dropDownItems[i];

            if (item.Label == null)
            {
                dropDownContent[i] = new GUIContent();
            }
            else
            {
                dropDownContent[i] = new GUIContent(item.Label);
            }
        }

        var clickedIndex = EditorGUI.Popup(dropDownPosition, selectedItem, dropDownContent);

        if (clickedIndex != selectedItem)
        {
            dropDownItems[clickedIndex].Command?.Invoke();
        }
    }
}