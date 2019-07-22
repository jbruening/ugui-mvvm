using AutoSuggest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;

namespace uguimvvm
{
    public class PropertyPathSuggestionProvider : ISuggestionProvider
    {
        private class PropertyPathException : Exception
        {
            public PropertyPathException(string message)
                : base(message)
            { }
        }

        private SerializedProperty _property;

        public event Action SuggestionsChanged;

        public PropertyPathSuggestionProvider(SerializedProperty property)
        {
            _property = property;
        }

        public IEnumerable<Suggestion> GetSuggestions(string currentValue, bool isFocused)
        {
            string currentValueToLastDot;
            string currentValueAfterLastDot;
            SplitOnLastDot(currentValue, out currentValueToLastDot, out currentValueAfterLastDot);

            try
            {
                var subProperties = GetSubProperties(_property, currentValue, !isFocused);

                if (!isFocused)
                {
                    // When not focused, return an empty list.  But still call GetSubProperties above to catch error cases and display them.
                    return Enumerable.Empty<Suggestion>();
                }

                var optionsWithInfo = subProperties.Select(p => new Suggestion(currentValueToLastDot + p, p, p.IndexOf(currentValueAfterLastDot, StringComparison.CurrentCultureIgnoreCase), currentValueAfterLastDot.Length))
                    .Where(opt => opt.DisplayTextMatchIndex >= 0);

                if (!optionsWithInfo.Any())
                {
                    // When focused, there should be at least one suggestion.
                    var errorMessage = string.Format("Error: \"{0}\" does not match any valid property path.",
                        currentValue);
                    throw new PropertyPathException(errorMessage);
                }

                return optionsWithInfo
                    .OrderBy(opt => opt.DisplayTextMatchIndex)
                    .ThenBy(opt => opt.Value.Length)
                    .ThenBy(opt => opt.Value);
            }
            catch (PropertyPathException ex)
            {
                var errorList = new List<Suggestion>();
                errorList.Add(new Suggestion(ex.Message));
                return errorList;
            }
        }

        public void FireSuggestionsChangedEvent()
        {
            SuggestionsChanged?.Invoke();
        }

        private static void SplitOnLastDot(string s, out string stringToLastDot, out string stringAfterLastDot)
        {
            var lastDotIndex = s.LastIndexOf('.');
            var indexAfterDot = lastDotIndex + 1;
            if (lastDotIndex == -1)
            {
                stringToLastDot = string.Empty;
                stringAfterLastDot = s;
            }
            else if (lastDotIndex >= s.Length - 1)
            {
                stringToLastDot = s;
                stringAfterLastDot = string.Empty;
            }
            else
            {
                stringToLastDot = s.Substring(0, indexAfterDot);
                stringAfterLastDot = s.Substring(indexAfterDot, s.Length - indexAfterDot);
            }
        }

        private static IEnumerable<string> GetSubProperties(SerializedProperty property, string currentPathString, bool throwOnInvalidPath)
        {
            SerializedProperty cprop, pprop;
            ComponentPathDrawer.GetCPathProperties(property, out cprop, out pprop);

            if (cprop.objectReferenceValue == null)
            {
                if (string.IsNullOrEmpty(currentPathString))
                {
                    return Enumerable.Empty<string>();
                }
                else
                {
                    var errorMessage = string.Format("Error: {0}/{1} is bound to property \"{2}\" of an invalid component object reference.",
                        property.displayName,
                        pprop.displayName,
                        pprop.stringValue);
                    throw new PropertyPathException(errorMessage);
                }
            }

            var objectReferenceType = GetTypeFromObjectReference(cprop.objectReferenceValue);

            if (objectReferenceType == null)
            {
                if (string.IsNullOrEmpty(currentPathString))
                {
                    return Enumerable.Empty<string>();
                }
                else
                {
                    var errorMessage = string.Format("Error: {0}/{1} is bound to property \"{2}\" of an invalid DataContext Type.",
                        property.displayName,
                        pprop.displayName,
                        pprop.stringValue);
                    throw new PropertyPathException(errorMessage);
                }
            }

            // pprop.stringValue can be one frame behind what is in currentPathString
            var path = new INPCBinding.PropertyPath(currentPathString, objectReferenceType);

            if (throwOnInvalidPath && !path.IsValid)
            {
                var errorMessage = string.Format("Error: {0}/{1} invalid property \"{2}\" of a valid DataContext.",
                        property.displayName,
                        pprop.displayName,
                        pprop.stringValue);
                throw new PropertyPathException(errorMessage);
            }

            var rtype = GetLastValidTypeBeforeTheDot(path.PPath, objectReferenceType);

            var props = rtype.GetProperties(BindingFlags.Instance | BindingFlags.Public);
            var propNames = props.Select(p => p.Name);
            return propNames;
        }

        private static Type GetTypeFromObjectReference(UnityEngine.Object o)
        {
            if (o is DataContext)
            {
                return (o as DataContext).Type;
            }
            else
            {
                return o.GetType();
            }
        }

        private static Type GetLastValidTypeBeforeTheDot(PropertyInfo[] propertyPath, Type fallbackValue)
        {
            // Start one entry before the last one.
            var lastEntry = propertyPath.Length - 1;
            for (int i = lastEntry - 1; i >= 0; i--)
            {
                if (propertyPath[i] != null)
                {
                    return propertyPath[i].PropertyType;
                }
            }

            return fallbackValue;
        }
    }
}
