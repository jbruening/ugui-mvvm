using System;
using System.Linq;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(DataContext))]
class DataContextEditor : Editor
{
    bool _searching;
    string _searchString;
    private Vector2 _scrollPos;
    private Type _tval;
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        var tprop = serializedObject.FindProperty("_type");

        if (_tval == null && !string.IsNullOrEmpty(tprop.stringValue))
        {
            _tval = Type.GetType(tprop.stringValue);
            if (_tval == null) //invalid type name. Clear it so we don't keep looking for an invalid type.
                tprop.stringValue = null;
        }

        if (_tval != null)
        {
            _searchString = EditorGUILayout.TextField(_tval.FullName);
            if (_searchString != _tval.FullName)
            {
                tprop.stringValue = null;
                _tval = null;
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
            foreach(var type in types)
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
    }
}