using System;
using System.Linq;
using UnityEngine;
using UnityEditor;
using uguimvvm;

[CustomEditor(typeof(DataContext))]
class DataContextEditor : Editor
{
    bool _searching;
    string _searchString;
    private Vector2 _scrollPos;
    private Type _tval;
    private bool _cvis;
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        var tprop = serializedObject.FindProperty("_type");
        var iprop = serializedObject.FindProperty("_instantiateOnAwake");
        var bprop = serializedObject.FindProperty("_propertyBinding");

        if (_tval == null && !string.IsNullOrEmpty(tprop.stringValue))
        {
            _tval = Type.GetType(tprop.stringValue);
            if (_tval == null) //invalid type name. Clear it so we don't keep looking for an invalid type.
            {
                tprop.stringValue = null;
                iprop.boolValue = false;
            }
        }

        if (_tval != null)
        {
            if (typeof(UnityEngine.Object).IsAssignableFrom(_tval))
                GUILayout.Label("Auto-instantiation not possible with UnityEngine.Object types");
            else
            {
                EditorGUILayout.PropertyField(iprop);

                INPCBindingEditor.DrawCRefProp(serializedObject.targetObject.GetInstanceID(), bprop, GUIContent.none);
            }
        }

        if (_tval != null)
        {
            _searchString = EditorGUILayout.TextField(_tval.FullName);
            if (_searchString != _tval.FullName)
            {
                tprop.stringValue = null;
                iprop.boolValue = false;
                _tval = null;
            }
            else
            {
                //_cvis = EditorGUILayout.Foldout(_cvis, "Commands");
                //if (_cvis)
                //{
                //    EditorGUI.indentLevel++;
                //    var cprops = _tval.GetProperties().Where(p => p.PropertyType == typeof(ICommand));
                //    foreach (var prop in cprops)
                //    {
                //        GUILayout.Label(prop.Name);
                //    }
                //    EditorGUI.indentLevel--;
                //}
            }
        }
        else
        {
            _searchString = EditorGUILayout.TextField(_searchString);
        }

        if (_tval == null && !string.IsNullOrEmpty(_searchString))
        {
            var types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes()).Where(t => t.Name.IndexOf(_searchString, StringComparison.OrdinalIgnoreCase) >= 0);
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos, GUILayout.Height(100));
            foreach (var type in types)
            {
                if (GUILayout.Button(type.FullName))
                {
                    _tval = type;
                    tprop.stringValue = _tval.AssemblyQualifiedName;
                }
            }
            EditorGUILayout.EndScrollView();
        }



        serializedObject.ApplyModifiedProperties();

        var dc = target as DataContext;
        if (dc != null)
        {
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.Toggle("Value?", dc.Value != null);
            EditorGUI.EndDisabledGroup();
        }
    }
}