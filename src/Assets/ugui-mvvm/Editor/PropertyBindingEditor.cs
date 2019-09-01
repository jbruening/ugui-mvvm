﻿using System.Linq;
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
        FigureViewBindings();
    }

#if UNITY_2017_2_5_OR_NEWER
    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        // adding this workaround because a lot of the bindings don't get cleaned up by the editor after quitting the scene 
        if (state == PlayModeStateChange.ExitingPlayMode)
        {
            foreach (var binding in cachedBindings)
            {
                if (binding.Mode != BindingMode.OneWayToView)
                {
                    RemoveViewBinding(binding);
                }
            }
            cachedBindings.Clear();
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        }
    }
#endif

    static void RemoveViewBinding(PropertyBinding binding)
    {
        var sobj = new SerializedObject(binding);
        var vevValue = GetViewEventValue(sobj);
        if (vevValue != null)
        {
            var eventCount = vevValue.GetPersistentEventCount();
            for (var idx = 0; idx < eventCount; idx++)
            {
                var perTarget = vevValue.GetPersistentTarget(idx);
                // this was a binding we added, let's remove it
                if (perTarget == binding)
                {
                    UnityEditor.Events.UnityEventTools.RemovePersistentListener(vevValue, idx);
                    eventCount--;
                    sobj.ApplyModifiedProperties();
                }
            }
        }
    }

    private static UnityEventBase GetViewEventValue(SerializedObject sobj)
    {
        var viewEvProp = sobj.FindProperty("_targetEvent");
        if (string.IsNullOrEmpty(viewEvProp.stringValue))
            return null;

        var viewProp = sobj.FindProperty("_target");
        var vcprop = viewProp.FindPropertyRelative("Component");

        var vcomp = vcprop.objectReferenceValue as Component;
        if (vcomp == null)
            return null;

        return GetEvent(vcomp, viewEvProp);
    }

    private static void FigureViewBindings()
    {
        var objects = Resources.FindObjectsOfTypeAll<GameObject>();
        foreach (var obj in objects)
        {
            var bindings = obj.GetComponents<PropertyBinding>();
            foreach (var binding in bindings)
                FigureViewBinding(binding);
        }
    }

    static void FigureViewBinding(PropertyBinding binding)
    {
        // Don't register for the target event that indicates the property has changed if the value from the target never flows back to the source.
        if (!binding.Mode.IsSourceBoundToTarget())
        {
            return;
        }

        var sobj = new SerializedObject(binding);
        var vevValue = GetViewEventValue(sobj);
        if (vevValue != null)
        {
            cachedBindings.Add(binding);
            var eventCount = vevValue.GetPersistentEventCount();

            for (var idx = 0; idx < eventCount; idx++)
            {
                var perTarget = vevValue.GetPersistentTarget(idx);
                // if we find a duplicate event skip over adding it
                if (perTarget == binding)
                {
                    return;
                }
            }

            UnityEditor.Events.UnityEventTools.AddVoidPersistentListener(vevValue, binding.ApplyVToVM);
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

        var evProp = type.GetProperty(eventName, typeof (UnityEventBase));
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
        return typeof (UnityEventBase).IsAssignableFrom(type);
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

        var vprop = serializedObject.FindProperty("_target");
        var vmprop = serializedObject.FindProperty("_source");
        var veprop = serializedObject.FindProperty("_targetEvent");
        var cprop = serializedObject.FindProperty("_converter");
        var mprop = serializedObject.FindProperty("_mode");

        EditorGUILayout.PropertyField(vprop, new GUIContent(vprop.displayName, "Typically, the Target would be a View"));

        // If the value never flows back from the target to the source, then there is no reason to pay attention to value change events on the target.
        int epropcount = -1;
        if (((PropertyBinding)target).Mode.IsSourceBoundToTarget())
        {
            epropcount = DrawCrefEvents(vprop, veprop);
        }

        EditorGUILayout.PropertyField(vmprop, new GUIContent(vmprop.displayName, "Typically, the Source would be a ViewModel"));

        EditorGUILayout.PropertyField(mprop, false);

        if (epropcount == 0)
        {
            if (mprop.enumValueIndex > 1)
            {
                EditorUtility.DisplayDialog("Error", string.Format("Cannot change {0} to {1}, as only {2} and {3} are valid for no event",
                    mprop.displayName, (BindingMode)mprop.enumValueIndex, BindingMode.OneTime, BindingMode.OneWayToTarget), "Okay");
                mprop.enumValueIndex = 1;
            }
        }

        var position = EditorGUILayout.GetControlRect(true);
        ComponentReferenceDrawer.PropertyField(position, cprop);

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
        var cprop = crefProperty.FindPropertyRelative("Component");
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
        if (component.objectReferenceValue != null)
        {
            FieldInfo[] unityEventFields = GetAllFields(component.objectReferenceValue.GetType())
                .Where(
                    field =>
                        typeof (UnityEventBase).IsAssignableFrom(field.FieldType) && (field.IsPublic ||
                        field.GetCustomAttributes(typeof (SerializeField), false).Length > 0)).ToArray();

            var currentSelectedIndex = Array.FindIndex(unityEventFields, p => p.Name == eventProperty.stringValue);

            var unityEventsDropDown = new DropDownMenu();
            for (int i = 0; i < unityEventFields.Length; i++)
            {
                FieldInfo unityEventField = unityEventFields[i];
                unityEventsDropDown.Add(new DropDownItem
                {
                    Label = ObjectNames.NicifyVariableName(unityEventField.Name),
                    IsSelected = currentSelectedIndex == i,
                    Command = () => eventProperty.stringValue = unityEventField.Name,
                });
            }

            if (unityEventFields.Length > 0)
            {
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

            return unityEventFields.Length;
        }

        return 0;
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
}
