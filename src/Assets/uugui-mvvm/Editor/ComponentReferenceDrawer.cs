using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using uguimvvm;

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
        position.width -= 10;
        EditorGUI.PropertyField(position, property);
        if (clipboard != null && GUI.Button(new Rect(position.xMax, position.y, 10, 10), new GUIContent("+", "paste reference to " + clipboard.name + "-" + clipboard.GetType())))
        {
            property.objectReferenceValue = clipboard;
        }
    }
}