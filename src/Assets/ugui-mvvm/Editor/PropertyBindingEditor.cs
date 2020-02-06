using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEditor;
using UnityEditor.Callbacks;
using System.Reflection;
using System;
using uguimvvm;
using System.Collections.Generic;
#if UNITY_5_3_OR_NEWER
using UnityEditor.SceneManagement;
#endif

[CustomEditor(typeof(PropertyBinding))]
class PropertyBindingEditor : Editor
{
    private static List<PropertyBinding> cachedBindings = new List<PropertyBinding>();

    #region scene post processing
    [PostProcessScene(1)]
    public static void OnPostProcessScene()
    {
#if UNITY_2017_2_5_OR_NEWER
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
#endif
        FigureUnityEventTriggeredBindings();
    }

#if UNITY_2017_2_5_OR_NEWER
    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        // adding this workaround because a lot of the bindings don't get cleaned up by the editor after quitting the scene 
        if (state == PlayModeStateChange.ExitingPlayMode)
        {
            foreach (var binding in cachedBindings)
            {
                if (binding.Mode != BindingMode.OneWayToTarget)
                {
                    RemoveViewBinding(binding, "_target", "_targetEvent");
                }

                if (binding.Mode != BindingMode.OneWayToSource)
                {
                    RemoveViewBinding(binding, "_source", "_sourceEvent");
                }
            }
            cachedBindings.Clear();
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        }
    }
#endif

    static void RemoveViewBinding(PropertyBinding binding, string componentPathPropertyName, string eventPropertyName)
    {
        var sobj = new SerializedObject(binding);
        var unityEvent = GetEventValue(sobj, componentPathPropertyName, eventPropertyName);
        if (unityEvent != null)
        {
            var eventCount = unityEvent.GetPersistentEventCount();
            for (var idx = 0; idx < eventCount; idx++)
            {
                var perTarget = unityEvent.GetPersistentTarget(idx);
                // this was a binding we added, let's remove it
                if (perTarget == binding)
                {
                    UnityEditor.Events.UnityEventTools.RemovePersistentListener(unityEvent, idx);
                    eventCount--;
                    sobj.ApplyModifiedProperties();
                }
            }
        }
    }

    private static UnityEventBase GetEventValue(SerializedObject sobj, string componentPathPropertyName, string eventPropertyName)
    {
        var eventProperty = sobj.FindProperty(eventPropertyName);
        if (string.IsNullOrEmpty(eventProperty.stringValue))
            return null;

        var componentPathProperty = sobj.FindProperty(componentPathPropertyName);
        var componentProperty = componentPathProperty.FindPropertyRelative(nameof(PropertyBinding.ComponentPath.Component));

        var component = componentProperty.objectReferenceValue as Component;
        if (component == null)
            return null;

        return GetEvent(component, eventProperty);
    }

    private static void FigureUnityEventTriggeredBindings()
    {
        var objects = Resources.FindObjectsOfTypeAll<GameObject>();
        foreach (var obj in objects)
        {
            var bindings = obj.GetComponents<PropertyBinding>();
            foreach (var binding in bindings)
            {
                FigureSourceBinding(binding);
                FigureTargetBinding(binding);
            }
        }
    }

    private static void FigureSourceBinding(PropertyBinding binding)
    {
        // Don't register for the target event that indicates the property has changed if the value from the target never flows back to the source.
        if (!binding.Mode.IsTargetBoundToSource())
        {
            return;
        }

        FigureBinding(binding, "_source", "_sourceEvent", binding.UpdateTarget);
    }

    private static void FigureTargetBinding(PropertyBinding binding)
    {
        // Don't register for the target event that indicates the property has changed if the value from the target never flows back to the source.
        if (!binding.Mode.IsSourceBoundToTarget())
        {
            return;
        }

        FigureBinding(binding, "_target", "_targetEvent", binding.UpdateSource);
    }

    private static void FigureBinding(PropertyBinding binding, string componentPathPropertyName, string eventPropertyName, UnityAction handler)
    {
        var sobj = new SerializedObject(binding);
        var unityEvent = GetEventValue(sobj, componentPathPropertyName, eventPropertyName);
        if (unityEvent != null)
        {
            cachedBindings.Add(binding);
            var eventCount = unityEvent.GetPersistentEventCount();

            for (var idx = 0; idx < eventCount; idx++)
            {
                var perTarget = unityEvent.GetPersistentTarget(idx);
                // if we find a duplicate event skip over adding it
                if (perTarget == binding)
                {
                    return;
                }
            }

            UnityEditor.Events.UnityEventTools.AddVoidPersistentListener(unityEvent, handler);
        }

        sobj.ApplyModifiedProperties();
    }

    public static UnityEventBase GetEvent(Component component, SerializedProperty eventProperty)
    {
        string eventName = eventProperty.stringValue;

        if (string.IsNullOrEmpty(eventName)) return null;

        var type = component.GetType();
        var evField = GetField(type, eventName);
        if (evField != null && IsEventType(evField.FieldType))
        {
            return evField.GetValue(component) as UnityEventBase;
        }

        var evProp = type.GetProperty(eventName, typeof(UnityEventBase));
        if (evProp != null)
        {
            return evProp.GetValue(component, null) as UnityEventBase;
        }

        var scomp = new SerializedObject(component);
        var it = scomp.GetIterator();
        //necessary to actually iterate over the properties of it.
        it.Next(true);
        var sep = FirstOrDefault(it, p => p.displayName == eventName);

        if (sep == null)
        {
            Debug.LogErrorFormat(component, "Could not get event {0} on {1}.  Something probably changed event names, so you need to fix it. {2}", eventName, type, PathTo(component));
            return null;
        }

        evField = GetField(type, sep.name);
        if (evField != null)
        {
            //need to update this, otherwise the things won't be able to bind
            eventProperty.stringValue = sep.name;

            return evField.GetValue(component) as UnityEventBase;
        }

        Debug.LogErrorFormat(component, "Could not get event {0} on {1}. Something probably changed event names, so you need to fix it. {2}", eventName, type, PathTo(component));

        return null;
    }

    private static string PathTo(Component component)
    {
        return PathTo(component.transform) + " Component " + component.name;
    }

    private static string PathTo(Transform transform)
    {
#if UNITY_5_3_OR_NEWER
        return (transform.parent != null ? PathTo(transform.parent) : "Scene " + transform.gameObject.scene.name) + "->" + transform.name;
#else
        return (transform.parent != null ? PathTo(transform.parent) : "Scene " + EditorApplication.currentScene) + "->" + transform.name;
#endif
    }

    static bool IsEventType(Type type)
    {
        return typeof(UnityEventBase).IsAssignableFrom(type);
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
    #endregion

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        var target = serializedObject.FindProperty("_target");
        var targetUpdateTrigger = serializedObject.FindProperty("_targetUpdateTrigger");
        var targetEvent = serializedObject.FindProperty("_targetEvent");

        var source = serializedObject.FindProperty("_source");
        var sourceUpdateTrigger = serializedObject.FindProperty("_sourceUpdateTrigger");
        var sourceEvent = serializedObject.FindProperty("_sourceEvent");

        var converter = serializedObject.FindProperty("_converter");
        var mode = serializedObject.FindProperty("_mode");

        int targetTriggerCount = DrawBindingComponent(target, "Typically, the Target would be a View", targetUpdateTrigger, targetEvent, ((BindingMode)mode.intValue).IsSourceBoundToTarget(), false);

        int sourceTriggerCount = DrawBindingComponent(source, "Typically, the Source would be a ViewModel", sourceUpdateTrigger, sourceEvent, ((BindingMode)mode.intValue).IsTargetBoundToSource(), true);

        EditorGUILayout.PropertyField(mode, false);

        if (targetTriggerCount == 0 && ((BindingMode)mode.intValue).IsSourceBoundToTarget())
        {
            EditorUtility.DisplayDialog("Error", string.Format("Cannot change {0} to {1}, as only {2} and {3} are valid for no target updated event",
                mode.displayName, mode.enumNames[mode.enumValueIndex], nameof(BindingMode.OneTime), nameof(BindingMode.OneWayToTarget)), "Okay");
            mode.intValue = (int)BindingMode.OneTime;
        }

        if (sourceTriggerCount == 0 && ((BindingMode)mode.intValue).IsTargetBoundToSource())
        {
            EditorUtility.DisplayDialog("Error", string.Format("Cannot change {0} to {1}, as only {2} and {3} are valid for no source updated event",
                mode.displayName, mode.enumNames[mode.enumValueIndex], nameof(BindingMode.OneTime), nameof(BindingMode.OneWayToSource)), "Okay");
            mode.intValue = (int)BindingMode.OneTime;
        }

        var position = EditorGUILayout.GetControlRect(true);
        ComponentReferenceDrawer.PropertyField(position, converter);

        serializedObject.ApplyModifiedProperties();
    }

    /// <summary>
    /// Draw all UnityEventBase
    /// </summary>
    /// <param name="crefProperty"></param>
    /// <param name="eventProperty"></param>
    /// <returns></returns>
    public static int DrawCrefEvents(SerializedProperty crefProperty, SerializedProperty eventProperty)
    {
        var cprop = crefProperty.FindPropertyRelative(nameof(PropertyBinding.ComponentPath.Component));
        return DrawComponentEvents(cprop, eventProperty);
    }

    /// <summary>
    /// Draw all UnityEventBase fields
    /// </summary>
    /// <param name="component"></param>
    /// <param name="eventProperty"></param>
    /// <returns></returns>
    public static int DrawComponentEvents(SerializedProperty component, SerializedProperty eventProperty)
    {
        List<DropDownItem> unityEvents = PropertyBindingEditor.GetDropUnityEventDownItems(component.objectReferenceValue?.GetType(), eventProperty.stringValue, eventName => eventProperty.stringValue = eventName).ToList();

        if (unityEvents.Count > 0)
        {
            var unityEventsDropDown = new DropDownMenu();
            unityEvents.ForEach(dropDownItem => unityEventsDropDown.Add(dropDownItem));

            EditorGUI.indentLevel++;
            unityEventsDropDown.OnGUI("Event");
            EditorGUI.indentLevel--;
        }
        else
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.LabelField("Event", "No available events");
            eventProperty.stringValue = "";
            EditorGUI.indentLevel--;
        }

        return unityEvents.Count;
    }

    public static IEnumerable<FieldInfo> GetAllFields(Type t)
    {
        if (t == null)
            return Enumerable.Empty<FieldInfo>();
        var flags = BindingFlags.Public | BindingFlags.NonPublic |
                    BindingFlags.Instance | BindingFlags.DeclaredOnly;
        return t.GetFields(flags).Concat(GetAllFields(t.BaseType));
    }

    public static FieldInfo GetField(Type t, string name)
    {
        if (t == null)
            return null;
        var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;
        return t.GetField(name, flags) ?? GetField(t.BaseType, name);
    }

    private static int DrawBindingComponent(SerializedProperty componentPathProperty, string componentDescription, SerializedProperty updateTriggerProperty, SerializedProperty unityEventProperty, bool enableUpdateTriggers, bool resolveDataContext)
    {
        using (var changeScope = new EditorGUI.ChangeCheckScope())
        {
            EditorGUILayout.PropertyField(componentPathProperty, new GUIContent(componentPathProperty.displayName, componentDescription));
            // If the binding target changes, reset the binding update trigger (since the type of the target will determine the available update triggers)
            if (changeScope.changed)
            {
                updateTriggerProperty.intValue = (int)BindingUpdateTrigger.None;
                unityEventProperty.stringValue = null;
            }
        }

        Type resolvedType = PropertyBinding.GetComponentType((Component)componentPathProperty.FindPropertyRelative(nameof(PropertyBinding.ComponentPath.Component)).objectReferenceValue, resolveDataContext);
        bool isINotifyPropertyChanged = typeof(System.ComponentModel.INotifyPropertyChanged).IsAssignableFrom(resolvedType);
        // Try to set the target update trigger to a reasonable default
        if (updateTriggerProperty.intValue == (int)BindingUpdateTrigger.None)
        {
            if (isINotifyPropertyChanged)
            {
                updateTriggerProperty.intValue = (int)BindingUpdateTrigger.PropertyChangedEvent;
            }
        }

        // If the value never flows back from the target to the source, then there is no reason to pay attention to value change events on the target.
        int updateTriggerCount = -1;
        if (enableUpdateTriggers)
        {
            var dropDownMenu = new DropDownMenu();

            if (isINotifyPropertyChanged)
            {
                dropDownMenu.Add(new DropDownItem
                {
                    Label = "Property Changed",
                    IsSelected = updateTriggerProperty.intValue == (int)BindingUpdateTrigger.PropertyChangedEvent,
                    Command = () =>
                    {
                        updateTriggerProperty.intValue = (int)BindingUpdateTrigger.PropertyChangedEvent;
                        unityEventProperty.stringValue = null;
                    }
                });
            }

            List<DropDownItem> unityEvents = PropertyBindingEditor.GetDropUnityEventDownItems(resolvedType, unityEventProperty.stringValue, unityEvent =>
            {
                unityEventProperty.stringValue = unityEvent;
                updateTriggerProperty.intValue = (int)BindingUpdateTrigger.UnityEvent;
            }).ToList();

            if (dropDownMenu.ItemCount > 0 && unityEvents.Any())
            {
                dropDownMenu.Add(new DropDownItem());
            }

            unityEvents.ForEach(dropDownItem => dropDownMenu.Add(dropDownItem));

            // Only show the update trigger dropdown if there is more than one choice
            if (dropDownMenu.ItemCount > 1)
            {
                using (new EditorGUI.IndentLevelScope())
                {
                    dropDownMenu.OnGUI("Event");
                }

                if (dropDownMenu.SelectedIndex < 0)
                {
                    EditorGUILayout.HelpBox($"Select an event that indicates the property has changed, or update the binding mode.", MessageType.Warning);
                }
            }

            updateTriggerCount = dropDownMenu.ItemCount;
        }

        return updateTriggerCount;
    }

    private static IEnumerable<DropDownItem> GetDropUnityEventDownItems(Type componentType, string currentEvent, Action<string> onEventSelected)
    {
        if (componentType != null)
        {
            FieldInfo[] unityEventFields = GetAllFields(componentType)
                .Where(
                    field =>
                        typeof(UnityEventBase).IsAssignableFrom(field.FieldType) && (field.IsPublic ||
                        field.GetCustomAttributes(typeof(SerializeField), false).Length > 0)).ToArray();

            var currentSelectedIndex = Array.FindIndex(unityEventFields, p => p.Name == currentEvent);

            for (int i = 0; i < unityEventFields.Length; i++)
            {
                FieldInfo unityEventField = unityEventFields[i];
                yield return new DropDownItem
                {
                    Label = ObjectNames.NicifyVariableName(unityEventField.Name),
                    IsSelected = currentSelectedIndex == i,
                    Command = () => onEventSelected(unityEventField.Name),
                };
            }
        }
    }
}
