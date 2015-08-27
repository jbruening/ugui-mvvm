using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEditor;
using UnityEditor.Callbacks;
using System.Reflection;
using System;
using uguimvvm;

[CustomEditor(typeof(INPCBinding))]
class INPCBindingEditor : Editor
{
    #region scene post processing
    [PostProcessScene(1)]
    public static void OnPostProcessScene()
    {
        FigureViewBindings();
    }

    private static void FigureViewBindings()
    {
        var objects = (GameObject[])FindObjectsOfType(typeof(GameObject));
        foreach (var obj in objects)
        {
            var bindings = obj.GetComponents<INPCBinding>();
            foreach (var binding in bindings)
                FigureViewBinding(binding);
        }
    }

    static void FigureViewBinding(INPCBinding binding)
    {
        var sobj = new SerializedObject(binding);
        var viewProp = sobj.FindProperty("_view");
        var viewEvProp = sobj.FindProperty("_viewEvent");
        if (string.IsNullOrEmpty(viewEvProp.stringValue))
            return;

        var vcprop = viewProp.FindPropertyRelative("Component");

        var vcomp = vcprop.objectReferenceValue as Component;
        if (vcomp == null)
            return;

        var vevValue = GetEvent(vcomp, viewEvProp);
        if (vevValue != null)
        {
            UnityEditor.Events.UnityEventTools.AddVoidPersistentListener(vevValue, binding.ApplyVToVM);
        }

        sobj.ApplyModifiedProperties();
    }

    public static UnityEventBase GetEvent(Component component, SerializedProperty eventProperty)
    {
        string eventName = eventProperty.stringValue;

        if (string.IsNullOrEmpty(eventName)) return null;

        var type = component.GetType();
        var evField = type.GetField(eventName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (evField != null && IsEventType(evField.FieldType))
        {
            return evField.GetValue(component) as UnityEventBase;
        }

        var evProp = type.GetProperty(eventName, typeof (UnityEventBase));
        if (evProp != null)
        {
            return evProp.GetValue(component, null) as UnityEventBase;
        }

        var scomp = new SerializedObject(component);
        var it = scomp.GetIterator();
        //necessary to actually iterate over the properties of it.
        it.Next(true);
        var sep = FirstOrDefault(it, p => p.displayName == eventName);
        
        if (sep == null)
        {
            Debug.LogErrorFormat("Could not get event {0} on {1}", eventName, type);
            return null;
        }

        evField = type.GetField(sep.name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (evField != null)
        {
            //need to update this, otherwise the things won't be able to bind
            eventProperty.stringValue = sep.name;

            return evField.GetValue(component) as UnityEventBase;
        }

        Debug.LogErrorFormat("Could not get event {0} on {1}", eventName, type);

        return null;
    }

    static bool IsEventType(Type type)
    {
        return typeof (UnityEventBase).IsAssignableFrom(type);
    }

    static SerializedProperty FirstOrDefault(SerializedProperty prop, Predicate<SerializedProperty> predicate, bool descendIntoChildren = false)
    {
        while (prop.Next(descendIntoChildren))
        {
            if (predicate(prop))
                return prop.Copy();
        }
        return null;
    }
    #endregion

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        var vprop = serializedObject.FindProperty("_view");
        var vmprop = serializedObject.FindProperty("_viewModel");
        var veprop = serializedObject.FindProperty("_viewEvent");
        var cprop = serializedObject.FindProperty("_converter");
        var mprop = serializedObject.FindProperty("_mode");

        var vrect = EditorGUILayout.GetControlRect(true, GetCRefHeight(vprop));
        DrawCRefProp(vrect, vprop, new GUIContent());

        var epropcount = DrawCrefEvents(vprop, veprop);

        var vmrect = EditorGUILayout.GetControlRect(true, GetCRefHeight(vmprop));
        DrawCRefProp(vmrect, vmprop, new GUIContent());

        EditorGUILayout.PropertyField(mprop, false);
        if (epropcount == 0)
        {
            if (mprop.enumValueIndex > 1)
            {
                EditorUtility.DisplayDialog("Error", string.Format("Cannot change {0} to {1}, as only {2} and {3} are valid for no event",
                    mprop.displayName, (BindingMode)mprop.enumValueIndex, BindingMode.OneTime, BindingMode.OneWayToView), "Okay");
                mprop.enumValueIndex = 1;
            }
        }

        EditorGUILayout.PropertyField(cprop, false);

        serializedObject.ApplyModifiedProperties();
    }

    /// <summary>
    /// Draw all UnityEventBase 
    /// </summary>
    /// <param name="crefProperty"></param>
    /// <param name="eventProperty"></param>
    /// <returns></returns>
    public static int DrawCrefEvents(SerializedProperty crefProperty, SerializedProperty eventProperty)
    {
        var cprop = crefProperty.FindPropertyRelative("Component");
        return DrawComponentEvents(cprop, eventProperty);
    }

    /// <summary>
    /// Draw all UnityEventBase fields
    /// </summary>
    /// <param name="component"></param>
    /// <param name="eventProperty"></param>
    /// <returns></returns>
    public static int DrawComponentEvents(SerializedProperty component, SerializedProperty eventProperty)
    {
        int epropcount = 0;
        if (component.objectReferenceValue != null)
        {
            var eprops = component
                .objectReferenceValue.GetType()
                .GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(
                    f =>
                        typeof (UnityEventBase).IsAssignableFrom(f.FieldType) && (f.IsPublic ||
                        f.GetCustomAttributes(typeof (SerializeField), false).Length > 0)).ToArray();
            var enames = eprops.Select(f => ObjectNames.NicifyVariableName(f.Name)).ToArray();
            epropcount = eprops.Length;

            if (eprops.Length > 0)
            {
                EditorGUI.indentLevel++;
                var fedx = Array.FindIndex(eprops, p => p.Name == eventProperty.stringValue);
                var edx = fedx < 0 ? 0 : fedx;
                var nedx = EditorGUILayout.Popup("Event", edx, enames);
                if (nedx != fedx && nedx >= 0 && nedx < eprops.Length)
                    eventProperty.stringValue = eprops[nedx].Name;
                EditorGUI.indentLevel--;
            }
            else
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.LabelField("Event", "No available events");
                eventProperty.stringValue = "";
                EditorGUI.indentLevel--;
            }
        }
        return epropcount;
    }

    /// <summary>
    /// get the height of an INPCBinding.ComponentPath property
    /// </summary>
    /// <param name="property"></param>
    /// <returns></returns>
    public static float GetCRefHeight(SerializedProperty property)
    {
        var cprop = property.FindPropertyRelative("Component");
        if (cprop.objectReferenceValue != null)
            return EditorGUIUtility.singleLineHeight * 3;
        return EditorGUIUtility.singleLineHeight * 2;
    }

    public static void DrawCRefProp(Rect position, SerializedProperty property, GUIContent label)
    {
        DrawCRefProp(position, property, label, typeof(object));
    }

    /// <summary>
    /// draw an INPCBinding.ComponentPath property
    /// </summary>
    /// <param name="position"></param>
    /// <param name="property"></param>
    /// <param name="label"></param>
    /// <param name="filter"></param>
    public static void DrawCRefProp(Rect position, SerializedProperty property, GUIContent label, Type filter)
    {
        EditorGUI.BeginProperty(position, label, property);
        EditorGUI.LabelField(position, property.displayName);

        position.y += EditorGUIUtility.singleLineHeight;
        EditorGUI.indentLevel++;
        position = EditorGUI.IndentedRect(position);
        EditorGUI.indentLevel--;
        position.height = EditorGUIUtility.singleLineHeight;
        var cprop = property.FindPropertyRelative("Component");
        ComponentReferenceDrawer.PropertyField(position, cprop);

        var pprop = property.FindPropertyRelative("Property");
        if (cprop.objectReferenceValue != null)
        {
            position.y += EditorGUIUtility.singleLineHeight;
            EditorGUI.LabelField(position, "Property");

            var orv = cprop.objectReferenceValue;
            Type ortype;
            if (orv is DataContext)
                ortype = (orv as DataContext).Type;
            else
                ortype = orv.GetType();
            if (ortype == null)
            {
                pprop.stringValue = null;
            }
            else
            {
                var props =
                    ortype.GetProperties()
                        .Where(p => filter.IsAssignableFrom(p.PropertyType))
                        .Select(p => p.Name)
                        .ToArray();
                var fidx = Array.FindIndex(props, p => p == pprop.stringValue);
                var idx = fidx < 0 ? 0 : fidx;

                position.x += EditorGUIUtility.labelWidth;
                position.xMax -= EditorGUIUtility.labelWidth;
                var nidx = EditorGUI.Popup(position, idx, props.ToArray());
                if (nidx != fidx)
                    pprop.stringValue = props[nidx];
            }
        }
        else
            pprop.stringValue = null;

        EditorGUI.EndProperty();
    }
}