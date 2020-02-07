using System;
using System.Linq;
using uguimvvm;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Concrete implementation of <see cref="ObsoleteAwareEnumDrawer{BindingMode}"/> to be used by the Editor.
/// </summary>
[CustomPropertyDrawer(typeof(BindingMode))]
public class BindingModeDrawer : ObsoleteAwareEnumDrawer<BindingMode>
{
    // Do nothing.  Just make a concrete type and the base will do the work.
}

/// <summary>
/// Equivalent to the default property drawer for an enum, but it hides obsolete enum values.
/// Known limitations: If an obsolete value was previously selected, it will not render that value, so it will appear in Editor that nothing is selected.
///                    If two or more non-obsolete enum entries have the same value and one of them is selected, the last one (alphabetically) will appear selected.
/// </summary>
/// <typeparam name="EnumT"></typeparam>
public class ObsoleteAwareEnumDrawer<EnumT> : PropertyDrawer
{
    /// <summary>
    /// Renders the given control.
    /// </summary>
    /// <param name="position">Rectangle on the screen to use for the render.</param>
    /// <param name="property">The <see cref="SerializedProperty"/> for which to render the custom GUI.</param>
    /// <param name="label">The label to render with the <paramref name="property"/>.</param>
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var dropDownPosition = EditorGUI.PrefixLabel(position, label);

        var enumType = typeof(EnumT);
        var names = Enum.GetNames(enumType);
        var values = Enum.GetValues(enumType).Cast<int>().ToList();

        var selectedItem = property.intValue;
        int newSelectedItem = selectedItem;

        DropDownMenu menu = new DropDownMenu();
        for (int i = 0; i < names.Length; i++)
        {
            var name = names[i];
            var value = values[i];
            var enumValueMember = enumType.GetMember(name).First();
            var isObsolete = enumValueMember.GetCustomAttributes(typeof(ObsoleteAttribute), false).Any();

            if (!isObsolete)
            {
                menu.Add(new DropDownItem
                {
                    Label = name,
                    IsSelected = property.intValue == value,
                    Command = () => { property.intValue = value; },
                });
            }
        }
        menu.OnGUI(dropDownPosition);

        if (newSelectedItem != selectedItem)
        {
            property.intValue = newSelectedItem;
        }
    }
}
