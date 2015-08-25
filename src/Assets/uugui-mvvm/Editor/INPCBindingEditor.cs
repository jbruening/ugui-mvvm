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
    [PostProcessScene(1)]
    public static void OnPostProcessScene()
    {
        FigureViewBindings();
    }

    private static void FigureViewBindings()
    {
        var objects = (GameObject[])UnityEngine.Object.FindObjectsOfType(typeof(GameObject));
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
        var vmProp = sobj.FindProperty("_viewModel");
        var viewEvProp = sobj.FindProperty("_viewEvent");
        if (string.IsNullOrEmpty(viewEvProp.stringValue))
            return;

        var vcprop = viewProp.FindPropertyRelative("Component");
        var vmcprop = vmProp.FindPropertyRelative("Component");

        var vpprop = viewProp.FindPropertyRelative("Property");
        var vmpprop = vmProp.FindPropertyRelative("Property");

        var vcomp = vcprop.objectReferenceValue as Component;
        if (vcomp == null)
            return;

        var vctype = vcomp.GetType();
        var scomp = new SerializedObject(vcomp);
        var vevprop = vctype.GetProperty(viewEvProp.stringValue);

        UnityEventBase vevValue = null;
        if (vevprop != null && typeof(UnityEventBase).IsAssignableFrom(vevprop.PropertyType))
        {
            vevValue = vevprop.GetValue(vcomp, null) as UnityEventBase;
        }

        else
        {
            SerializedProperty evProp = null;
            var it = scomp.GetIterator();
            //necessary to actually iterate over the properties of it.
            it.Next(true);
            evProp = FirstOrDefault(it, p => p.displayName == viewEvProp.stringValue);
            viewEvProp.stringValue = evProp.name;

            //if for some reason the reflection stops working, switching for the serialized properties should.
            #region serialized properties
            //var m_PersistantCalls = evProp.FindPropertyRelative("m_PersistentCalls");
            //var m_calls = m_PersistantCalls.FindPropertyRelative("m_Calls");
            ////add to the end.
            //m_calls.InsertArrayElementAtIndex(m_calls.arraySize);
            //var m_idx = m_calls.GetArrayElementAtIndex(m_calls.arraySize - 1);

            //var m_Target = m_idx.FindPropertyRelative("m_Target");
            //var m_MethodName = m_idx.FindPropertyRelative("m_MethodName");
            //var m_Mode = m_idx.FindPropertyRelative("m_Mode");
            //var m_CallState = m_idx.FindPropertyRelative("m_CallState");

            //m_Target.objectReferenceValue = binding;
            //var methodName = new Action(binding.ApplyVToVM).Method.Name;
            //m_MethodName.stringValue = methodName;
            //m_Mode.enumValueIndex = Array.IndexOf(Enum.GetValues(typeof(PersistentListenerMode)), PersistentListenerMode.Void);
            //m_CallState.enumValueIndex = Array.IndexOf(Enum.GetValues(typeof(UnityEventCallState)), UnityEventCallState.RuntimeOnly);
            #endregion

            #region reflection
            var vevProp = vctype.GetField(evProp.name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (vevProp == null)
            {
                Debug.LogErrorFormat("Could not get property {0} on {1}", evProp.name, vctype);
                return;
            }

            vevValue = vevProp.GetValue(vcomp) as UnityEventBase;
        }

        if (vevValue == null)
        {
            Debug.LogErrorFormat("Could not get ", viewEvProp.stringValue);
            return;
        }

        UnityEditor.Events.UnityEventTools.AddVoidPersistentListener(vevValue, binding.ApplyVToVM);
            #endregion

        scomp.ApplyModifiedProperties();
        sobj.ApplyModifiedProperties();
    }

    private static void ListChildProperties(SerializedProperty prop)
    {
        prop = prop.Copy();
        while (prop.Next(true))
        {
            Debug.LogFormat("{0} - {1}", prop.propertyPath, prop.type);
        }
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

        int epropcount = 0;
        if (vprop.FindPropertyRelative("Component").objectReferenceValue != null)
        {
            var eprops =
                vprop.FindPropertyRelative("Component")
                    .objectReferenceValue.GetType()
                    .GetProperties()
                    .Where(p => typeof(UnityEventBase).IsAssignableFrom(p.PropertyType))
                    .Select(p => p.Name).ToArray();
            epropcount = eprops.Length;

            if (eprops.Length > 0)
            {
                EditorGUI.indentLevel++;
                var fedx = Array.FindIndex(eprops, p => p == veprop.stringValue);
                var edx = fedx < 0 ? 0 : fedx;
                var nedx = EditorGUILayout.Popup("Event", edx, eprops);
                if (nedx != fedx && nedx >= 0 && nedx < eprops.Length)
                    veprop.stringValue = eprops[nedx];
                EditorGUI.indentLevel--;
            }
            else
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.LabelField("Event", "No available events");
                EditorGUI.indentLevel--;
            }
        }

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

    float GetCRefHeight(SerializedProperty property)
    {
        var cprop = property.FindPropertyRelative("Component");
        if (cprop.objectReferenceValue != null)
            return EditorGUIUtility.singleLineHeight * 3;
        return EditorGUIUtility.singleLineHeight * 2;
    }

    void DrawCRefProp(Rect position, SerializedProperty property, GUIContent label)
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
                var props = ortype.GetProperties().Select(p => p.Name).ToArray();
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