using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using uguimvvm;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(BindingMode))]
class BindingModeDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        BindingMode mode = (BindingMode)property.intValue;
        Debug.Log($"{mode}");

        var enumType = typeof(BindingMode);
        var selectedItem = property.intValue;
        var names = Enum.GetNames(enumType);

        int newSelectedItem = selectedItem;

        DropDownMenu menu = new DropDownMenu();
        for (int i = 0; i < names.Length; i++)
        {
            var name = names[i];
            var enumValueMember = enumType.GetMember(name).First();
            var isObsolete = enumValueMember.GetCustomAttributes(typeof(ObsoleteAttribute), false).Any();

            if (!isObsolete)
            {
                menu.Add(new DropDownItem
                {
                    Label = name,
                    IsSelected = i == selectedItem,
                    Command = () => { newSelectedItem = i; },
                });
            }
        }
        menu.OnGUI(position);

        if (newSelectedItem != selectedItem)
        {
            property.intValue = newSelectedItem;
        }

        //base.OnGUI(position, property, label);
    }
}
