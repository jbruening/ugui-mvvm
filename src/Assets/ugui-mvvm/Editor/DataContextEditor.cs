using System;
using System.Linq;
using UnityEngine;
using UnityEditor;
using uguimvvm;
using System.Collections.Generic;

[CustomEditor(typeof(DataContext))]
class DataContextEditor : Editor
{
    private static readonly string SearchFieldLabel = "Type name";
    private static readonly string SearchFieldControlName = "SearchField";

    private bool _searching;
    private string _searchString;
    // MRMW_CHANGE - BEGIN: Improve searching
    private string _previousSearchString = String.Empty;
    private IEnumerable<Type> _types;
    // MRMW_CHANGE - END: Improve searching
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
                // MRMW_CHANGE - BEGIN: Improve handling of invalid DataContext types
                var style = new GUIStyle(EditorStyles.textField);
                style.normal.textColor = Color.red;

                EditorGUILayout.TextField(string.Format("Error: Invalid type \"{0}\"",
                    tprop.stringValue),
                    style);
                // MRMW_CHANGE - END: Improve handling of invalid DataContext types
            }
        }

        if (_tval != null)
        {
            if (typeof(UnityEngine.Object).IsAssignableFrom(_tval))
            {
                GUILayout.Label("Auto-instantiation not possible with UnityEngine.Object types");
            }
            else
            {
                EditorGUILayout.PropertyField(iprop);

                INPCBindingEditor.DrawCRefProp(serializedObject.targetObject.GetInstanceID(), bprop, GUIContent.none);
            }
        }

        if (_tval != null)
        {
            _searchString = EditorGUILayout.TextField(SearchFieldLabel, _tval.FullName);
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

            GUI.SetNextControlName(SearchFieldControlName);
            _searchString = EditorGUILayout.TextField(SearchFieldLabel, _searchString);
        }

        if (_tval == null && !string.IsNullOrEmpty(_searchString))
        {
            // MRMW_CHANGE - BEGIN: Improve searching
            if (!_previousSearchString.Equals(_searchString))
            {
                _previousSearchString = _searchString;
                _types = null;
            }

            if (_types == null && GUILayout.Button("Search"))
            {
                var typeQuery = AppDomain.CurrentDomain.GetAssemblies().SelectMany(a =>
                {
                    try
                    {
                        return a.GetTypes();
                    } catch (Exception)
                    {
                        return new Type[] { };
                    }
                 }).Where(t => t.AssemblyQualifiedName.IndexOf(_searchString, StringComparison.OrdinalIgnoreCase) >= 0);

                // Calling ToList forces the query to execute this one time, instead of executing every single time "types" is enumerated.
                _types = typeQuery.ToList();
            }

            EditorGUI.FocusTextInControl(SearchFieldControlName);

            if (_types != null)
            {
                _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos, GUILayout.Height(100));
                foreach (var type in _types)
                {
                    if (GUILayout.Button(type.FullName))
                    {
                        _tval = type;
                        tprop.stringValue = _tval.AssemblyQualifiedName;
                    }
                }
                EditorGUILayout.EndScrollView();
            }
            // MRMW_CHANGE - END: Improve searching
        }

        serializedObject.ApplyModifiedProperties();

        var dc = target as DataContext;
        if (dc != null)
        {
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.Toggle("Value is non-null?", dc.Value != null);
            EditorGUI.EndDisabledGroup();
        }
    }
}
