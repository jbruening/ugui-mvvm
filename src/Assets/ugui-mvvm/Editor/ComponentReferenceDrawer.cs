using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using uguimvvm;
using System.Collections.Generic;

[CustomPropertyDrawer(typeof(ComponentReferenceAttribute))]
class ComponentReferenceDrawer : PropertyDrawer
{
    private class DropdownItem
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
        const float dropDownWidth = 20.0f;

        position.width -= dropDownWidth;
        EditorGUI.PropertyField(position, property);

        var dropdownPosition = new Rect(position.xMax, position.y, dropDownWidth, EditorGUIUtility.singleLineHeight);

        //if (EditorGUI.DropdownButton(dropdownPosition, new GUIContent(), FocusType.Keyboard))
        {
            //GenericMenu dropdownMenu = new GenericMenu();
            var dropdownItems = new List<DropdownItem>();
            int selectedItem = -1;

            if (clipboard != null)
            {
                //dropdownMenu.AddItem(
                //    new GUIContent($"Paste reference to {clipboard.name} - {clipboard.GetType()}"),
                //    false,
                //    () => { property.objectReferenceValue = clipboard; });
                dropdownItems.Add(new DropdownItem
                {
                    Label = $"Paste reference to {clipboard.name} - {clipboard.GetType()}",
                    Command = () => { property.objectReferenceValue = clipboard; },
                });
            }
            else
            {
                //dropdownMenu.AddDisabledItem(new GUIContent("No component reference to paste."));
                dropdownItems.Add(new DropdownItem { Label = "Nothing to paste.  Right click on a component and select \"Copy Component Reference\"" });
            }

            var component = property.objectReferenceValue as Component;
            if (component != null && component.gameObject != null)
            {
                dropdownItems.Add(new DropdownItem { Label = null });
                //dropdownItems.Add(new DropdownItem { Label = "-------------------" });
                //dropdownMenu.AddSeparator("");

                var siblingComponents = component.gameObject.GetComponents<Component>();

                foreach (var siblingComponent in siblingComponents)
                {
                    bool currentlySelected = siblingComponent == component;

                    //dropdownMenu.AddItem(
                    //    new GUIContent($"{siblingComponent.name} - {siblingComponent.GetType()}"),
                    //    currentlySelected,
                    //    () => {
                    //        property.objectReferenceValue = siblingComponent;
                    //        Debug.Log($"select - {siblingComponent.name} - {siblingComponent.GetType()}");
                    //    });

                    if (currentlySelected)
                    {
                        // This component is currently selected.  Mark it as such.
                        selectedItem = dropdownItems.Count;
                    }

                    dropdownItems.Add(new DropdownItem
                    {
                        Label = $"Paste reference to {siblingComponent.name} - {siblingComponent.GetType()}",
                        Command = () => { property.objectReferenceValue = siblingComponent; },
                    });
                }
            }

            var dropdownContent = new GUIContent[dropdownItems.Count];
            for (int i = 0; i < dropdownContent.Length; i++)
            {
                var item = dropdownItems[i];

                if (item.Label == null)
                {
                    dropdownContent[i] = new GUIContent();
                }
                else
                {
                    dropdownContent[i] = new GUIContent(item.Label);
                }
            }

            var clickedIndex = EditorGUI.Popup(dropdownPosition, selectedItem, dropdownContent);

            if (clickedIndex != selectedItem)
            {
                dropdownItems[clickedIndex].Command?.Invoke();
            }

            //Debug.Log("before dropdown");
            //dropdownMenu.DropDown(dropdownPosition);
            //Debug.Log("after dropdown");
        }

        //Debug.Log($"{property.propertyPath} - {property.serializedObject} - {property.exposedReferenceValue} - {property.objectReferenceInstanceIDValue} - {property.objectReferenceValue}");

        //if (clipboard != null && GUI.Button(new Rect(position.xMax, position.y, 10, 10), new GUIContent("+", "paste reference to " + clipboard.name + "-" + clipboard.GetType())))
        //{
        //    property.objectReferenceValue = clipboard;
        //}
    }
}