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

    private string _searchString;
    private string _previousSearchString = String.Empty;
    private IEnumerable<Type> _types;
    private Vector2 _scrollPos;
    private Type _tval;
    private string _focusedControl;
    private bool _hasFocusedSearchControl = false;

    public override void OnInspectorGUI()
    {
        // Only update the focused element name during the Layout event, since all controls must be static between Layout & Repaint.
        if (Event.current.type == EventType.Layout)
        {
            _focusedControl = GUI.GetNameOfFocusedControl();
        }

        serializedObject.Update();

        var tprop = serializedObject.FindProperty("_type");
        var iprop = serializedObject.FindProperty("_instantiateOnAwake");
        var bprop = serializedObject.FindProperty("_propertyBinding");

        if (_tval == null && !string.IsNullOrEmpty(tprop.stringValue))
        {
            _tval = Type.GetType(tprop.stringValue);
            if (_tval == null) //invalid type name. Clear it so we don't keep looking for an invalid type.
            {
                // Handle invalid DataContext types
                var style = new GUIStyle(EditorStyles.textField);
                style.normal.textColor = Color.red;

                EditorGUILayout.TextField(string.Format("Error: Invalid type \"{0}\"",
                    tprop.stringValue),
                    style);
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

                INPCBindingEditor.DrawCRefProp(serializedObject.targetObject.GetInstanceID(), _focusedControl, bprop, GUIContent.none);
            }
        }

        if (_tval != null)
        {
            _hasFocusedSearchControl = false;
            _searchString = EditorGUILayout.TextField(SearchFieldLabel, _tval.FullName);
            if (_searchString != _tval.FullName)
            {
                tprop.stringValue = null;
                iprop.boolValue = false;
                _tval = null;
            }
        }
        else
        {
            GUI.SetNextControlName(SearchFieldControlName);
            _searchString = EditorGUILayout.TextField(SearchFieldLabel, _searchString);
        }

        if (_tval == null && !string.IsNullOrEmpty(_searchString))
        {
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
                    }
                    catch (Exception)
                    {
                        return new Type[] { };
                    }
                }).Where(t => t.AssemblyQualifiedName.IndexOf(_searchString, StringComparison.OrdinalIgnoreCase) >= 0);

                // Calling ToList forces the query to execute this one time, instead of executing every single time "types" is enumerated.
                _types = typeQuery.ToList();
            }

            if (_types != null)
            {
                using (var scrollViewScope = new EditorGUILayout.ScrollViewScope(_scrollPos, GUILayout.Height(100)))
                {
                    _scrollPos = scrollViewScope.scrollPosition;
                    foreach (var type in _types)
                    {
                        if (GUILayout.Button(type.FullName))
                        {
                            _tval = type;
                            tprop.stringValue = _tval.AssemblyQualifiedName;
                        }
                    }
                }
            }
        }

        serializedObject.ApplyModifiedProperties();

        var dc = target as DataContext;
        if (dc != null)
        {
            using (var disabledGroupScope = new EditorGUI.DisabledGroupScope(true))
            {
                EditorGUILayout.Toggle("Value is non-null?", dc.Value != null);
            }
        }

        // Only update the focused control at the very end of the draw call, and only update the focus once when the user starts a search.
        if (_tval == null && !string.IsNullOrEmpty(_searchString))
        {
            if (!_hasFocusedSearchControl)
            {
                EditorGUI.FocusTextInControl(SearchFieldControlName);
                _hasFocusedSearchControl = true;
            }
        }
    }
}
