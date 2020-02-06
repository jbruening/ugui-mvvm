using System;
#if UNITY_WSA || !NET_LEGACY
using ICommand = System.Windows.Input.ICommand;
#endif
using uguimvvm;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

[CustomEditor(typeof(CommandBinding))]
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
        var objects = Resources.FindObjectsOfTypeAll<GameObject>();
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
        var vcprop = sobj.FindProperty("_target");
        var veprop = sobj.FindProperty("_targetEvent");
        if (string.IsNullOrEmpty(veprop.stringValue))
            return;

        var vcomp = vcprop.objectReferenceValue as Component;
        if (vcomp == null)
            return;

        var @event = PropertyBindingEditor.GetEvent(vcomp, veprop);
        if (@event != null)
        {
            // Fixing adding multiple command binding event handlers when using prefabs
            var eventCount = @event.GetPersistentEventCount();
            for (var idx = 0; idx < eventCount; idx++)
            {
                var perTarget = @event.GetPersistentTarget(idx);
                // if we find a duplicate event skip over adding it
                if (perTarget == binding)
                {
                    return;
                }
            }

            UnityEditor.Events.UnityEventTools.AddVoidPersistentListener(@event, binding.ExecuteCommand);
        }

        sobj.ApplyModifiedProperties();
    }
    #endregion

    void OnEnable()
    {
        _vprop = serializedObject.FindProperty("_target");
        _vmprop = serializedObject.FindProperty("_source");
        _veprop = serializedObject.FindProperty("_targetEvent");
        _parmprop = serializedObject.FindProperty("_parameter");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        ComponentReferenceDrawer.PropertyField(EditorGUILayout.GetControlRect(), _vprop);

        if (_vprop.objectReferenceValue != null)
            PropertyBindingEditor.DrawComponentEvents(_vprop, _veprop);

        EditorGUILayout.PropertyField(_vmprop);

        var rect = EditorGUILayout.GetControlRect(true, EditorGUIUtility.singleLineHeight);
        using (var propertyScope = new EditorGUI.PropertyScope(rect, null, _parmprop))
        {
            GUI.Label(rect, propertyScope.content);

            //parameter type
            var typeProp = _parmprop.FindPropertyRelative("Type");
            var trect = rect;
            trect.x += EditorGUIUtility.labelWidth;
            trect.width -= EditorGUIUtility.labelWidth;
            EditorGUI.PropertyField(trect, typeProp);

            //value field
            var typeValue = (BindingParameterType)Enum.GetValues(typeof(BindingParameterType)).GetValue(typeProp.enumValueIndex);
            SerializedProperty valueProp = null;
            switch (typeValue)
            {
                case BindingParameterType.None:
                    break;
                default:
                    valueProp = _parmprop.FindPropertyRelative(typeValue.ToString());
                    break;
            }
            if (valueProp != null)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(valueProp);
                EditorGUI.indentLevel--;
            }
        }

        serializedObject.ApplyModifiedProperties();
    }
}
