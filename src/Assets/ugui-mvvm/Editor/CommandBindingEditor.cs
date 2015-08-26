using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using uguimvvm;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.Events;

[CustomEditor(typeof (CommandBinding))]
class CommandBindingEditor : Editor
{
    private SerializedProperty _parmprop;
    private SerializedProperty _vprop;
    private SerializedProperty _vmprop;
    private SerializedProperty _veprop;

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
            var bindings = obj.GetComponents<CommandBinding>();
            foreach (var binding in bindings)
                FigureViewBinding(binding);
        }
    }

    static void FigureViewBinding(CommandBinding binding)
    {
        var sobj = new SerializedObject(binding);
        var vprop = sobj.FindProperty("_view");
        var veprop = sobj.FindProperty("_viewEvent");
        if (string.IsNullOrEmpty(veprop.stringValue))
            return;

        var vcprop = vprop.FindPropertyRelative("Component");

        var vcomp = vcprop.objectReferenceValue as Component;
        if (vcomp == null)
            return;

        var vctype = vcomp.GetType();
        var vevprop = vctype.GetProperty(veprop.stringValue);

        if (vevprop == null)
        {
            Debug.LogWarningFormat("Could not find member {0} on {1}", veprop.stringValue, vctype);
            return;
        }

        if (!typeof(UnityEventBase).IsAssignableFrom(vevprop.PropertyType))
        {
            Debug.LogWarningFormat("Type {0} is not a UnityEventBase", vevprop.Name);
        }

        var vevValue = vevprop.GetValue(vcomp, null) as UnityEventBase;

        UnityEditor.Events.UnityEventTools.AddVoidPersistentListener(vevValue, binding.ExecuteCommand);
    }
    #endregion

    void OnEnable()
    {
        _vprop = serializedObject.FindProperty("_view");
        _vmprop = serializedObject.FindProperty("_viewModel");
        _veprop = serializedObject.FindProperty("_viewEvent");
        _parmprop = serializedObject.FindProperty("_parameter");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        var vrect = EditorGUILayout.GetControlRect(true, INPCBindingEditor.GetCRefHeight(_vprop));
        INPCBindingEditor.DrawCRefProp(vrect, _vprop, new GUIContent());
        INPCBindingEditor.DrawCrefEvents(_vprop, _veprop);
    }
}
