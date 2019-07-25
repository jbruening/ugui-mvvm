using AutoSuggest;
using System;
using uguimvvm;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(DataContext))]
class DataContextEditor : Editor
{
    private static readonly string SearchFieldLabel = "Type name";

    private string _searchString = null;
    private Type _tval;
    private TypeSuggestionProvider _suggestionProvider;
    private AutoSuggestField _autoSuggestField;

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        var tprop = serializedObject.FindProperty("_type");
        var iprop = serializedObject.FindProperty("_instantiateOnAwake");
        var bprop = serializedObject.FindProperty("_propertyBinding");

        if (_autoSuggestField == null)
        {
            _suggestionProvider = new TypeSuggestionProvider();
            _autoSuggestField = new AutoSuggestField(
                _suggestionProvider,
                new GUIContent(SearchFieldLabel),
                new AutoSuggestField.Options
                {
                    DisplayMode = DisplayMode.Inline,
                });
        }

        if (_searchString == null)
        {
            // On first frame, _searchString will be null.
            // After block line, it will be non-null.

            _tval = Type.GetType(tprop.stringValue);
            _searchString = _tval?.FullName ?? string.Empty;
        }

        _searchString = _autoSuggestField.OnGUI(_searchString);

        if (_suggestionProvider.SelectedTypeIsValid && Event.current.type == EventType.Layout)
        {
            _tval = _suggestionProvider.SelectedType;
            tprop.stringValue = _tval?.AssemblyQualifiedName;
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
                EditorGUILayout.PropertyField(bprop);
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
    }
}
