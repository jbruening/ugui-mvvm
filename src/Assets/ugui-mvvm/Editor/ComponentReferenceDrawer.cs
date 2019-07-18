using uguimvvm;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(ComponentReferenceAttribute))]
class ComponentReferenceDrawer : PropertyDrawer
{
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

        var menu = new DropDownMenu();

        bool clipboardEntryAdded = false;
        if (clipboard != null)
        {
            menu.Add(new DropDownItem
            {
                Label = $"Paste component: {clipboard.name} ({clipboard.GetType().Name})",
                Command = () => { property.objectReferenceValue = clipboard; },
            });

            clipboardEntryAdded = true;
        }

        var component = property.objectReferenceValue as Component;
        if (component != null && component.gameObject != null)
        {
            if (clipboardEntryAdded)
            {
                menu.Add(new DropDownItem { Label = null });
            }

            var siblingComponents = component.gameObject.GetComponents<Component>();

            foreach (var siblingComponent in siblingComponents)
            {
                bool currentlySelected = siblingComponent == component;

                menu.Add(new DropDownItem
                {
                    Label = $"{siblingComponent.GetType().Name}",
                    Command = () => { property.objectReferenceValue = siblingComponent; },
                    IsSelected = currentlySelected,
                });
            }
        }

        // Render the menu.  If the user selects an item, it will execute that item's command.
        menu.OnGUI(dropDownPosition);
    }
}