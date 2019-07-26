using AutoSuggest;
using UnityEditor;
using UnityEngine;

namespace uguimvvm
{
    [CustomPropertyDrawer(typeof(PropertyBinding.ComponentPath))]
    class ComponentPathDrawer : PropertyDrawer
    {
        private PropertyPathSuggestionProvider _suggestionProvider;
        private AutoSuggestField _autoSuggestField;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.LabelField(position, label);
            EditorGUI.indentLevel++;

            SerializedProperty component, propertyPath;
            GetCPathProperties(property, out component, out propertyPath);

            using (var changeScope = new EditorGUI.ChangeCheckScope())
            {
                position = EditorGUILayout.GetControlRect(false);
                ComponentReferenceDrawer.PropertyField(position, component);

                if (changeScope.changed)
                {
                    // Notify the suggestion provider that any cached list of suggestions are now invalid.
                    _suggestionProvider.FireSuggestionsChangedEvent();
                }
            }

            if (_autoSuggestField == null)
            {
                _suggestionProvider = new PropertyPathSuggestionProvider(property);
                _autoSuggestField = new AutoSuggestField(
                    _suggestionProvider,
                    new GUIContent(propertyPath.displayName),
                    new AutoSuggestField.Options
                    {
                        DisplayMode = DisplayMode.Inline,
                    });
            }

            propertyPath.stringValue = _autoSuggestField.OnGUI(propertyPath.stringValue);

            EditorGUI.indentLevel--;
        }

        public static void GetCPathProperties(SerializedProperty property, out SerializedProperty component, out SerializedProperty path)
        {
            component = property.FindPropertyRelative("Component");
            path = property.FindPropertyRelative("Property");
        }
    }
}
