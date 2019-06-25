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

[CustomEditor(typeof(INPCBinding))]
class INPCBindingEditor : Editor
{
    private static List<INPCBinding> cachedBindings = new List<INPCBinding>();
    private string _focusedControl = "";

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

    static void RemoveViewBinding(INPCBinding binding)
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
        var viewEvProp = sobj.FindProperty("_viewEvent");
        if (string.IsNullOrEmpty(viewEvProp.stringValue))
            return null;

        var viewProp = sobj.FindProperty("_view");
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
            var bindings = obj.GetComponents<INPCBinding>();
            foreach (var binding in bindings)
                FigureViewBinding(binding);
        }
    }

    static void FigureViewBinding(INPCBinding binding)
    {
        if (binding.Mode == BindingMode.OneWayToView)
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
        // Only update the focused element name during the Layout event, since all controls must be static between Layout & Repaint.
        if (Event.current.type == EventType.Layout)
        {
            _focusedControl = GUI.GetNameOfFocusedControl();
        }

        serializedObject.Update();

        var vprop = serializedObject.FindProperty("_view");
        var vmprop = serializedObject.FindProperty("_viewModel");
        var veprop = serializedObject.FindProperty("_viewEvent");
        var cprop = serializedObject.FindProperty("_converter");
        var mprop = serializedObject.FindProperty("_mode");

        DrawCRefProp(serializedObject.targetObject.GetInstanceID(), _focusedControl, vprop, new GUIContent());

        var epropcount = DrawCrefEvents(vprop, veprop);

        DrawCRefProp(serializedObject.targetObject.GetInstanceID(), _focusedControl, vmprop, new GUIContent());

        EditorGUILayout.PropertyField(mprop, false);
        if (epropcount == 0)
        {
            if (mprop.enumValueIndex > 1)
            {
                EditorUtility.DisplayDialog("Error", string.Format("Cannot change {0} to {1}, as only {2} and {3} are valid for no event",
                    mprop.displayName, (BindingMode)mprop.enumValueIndex, BindingMode.OneTime, BindingMode.OneWayToView), "Okay");
                mprop.enumValueIndex = 1;
            }
        }

        EditorGUILayout.PropertyField(cprop, false);

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
        int epropcount = 0;
        if (component.objectReferenceValue != null)
        {
            var eprops = GetAllFields(component.objectReferenceValue.GetType())
                .Where(
                    f =>
                        typeof (UnityEventBase).IsAssignableFrom(f.FieldType) && (f.IsPublic ||
                        f.GetCustomAttributes(typeof (SerializeField), false).Length > 0)).ToArray();
            var enames = eprops.Select(f => ObjectNames.NicifyVariableName(f.Name)).ToArray();
            epropcount = eprops.Length;

            if (eprops.Length > 0)
            {
                EditorGUI.indentLevel++;
                var fedx = Array.FindIndex(eprops, p => p.Name == eventProperty.stringValue);
                var edx = fedx < 0 ? 0 : fedx;
                var nedx = EditorGUILayout.Popup("Event", edx, enames);
                if (nedx != fedx && nedx >= 0 && nedx < eprops.Length)
                    eventProperty.stringValue = eprops[nedx].Name;
                EditorGUI.indentLevel--;
            }
            else
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.LabelField("Event", "No available events");
                eventProperty.stringValue = "";
                EditorGUI.indentLevel--;
            }
        }
        return epropcount;
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

    public static void DrawCRefProp(int targetId, string focusedControl, SerializedProperty property, GUIContent label, bool resolveDataContext = true)
    {
        DrawCRefProp(targetId, focusedControl, property, label, typeof(object), resolveDataContext);
    }

    public static void GetCPathProperties(SerializedProperty property, out SerializedProperty component, out SerializedProperty path)
    {
        component = property.FindPropertyRelative("Component");
        path = property.FindPropertyRelative("Property");
    }

    /// <summary>
    /// draw an INPCBinding.ComponentPath property
    /// </summary>
    /// <param name="targetId"></param>
    /// <param name="property"></param>
    /// <param name="label"></param>
    /// <param name="filter"></param>
    /// <param name="resolveDataContext"></param>
    public static void DrawCRefProp(int targetId, string focusedControl, SerializedProperty property, GUIContent label, Type filter, bool resolveDataContext = true)
    {
        EditorGUILayout.LabelField(property.displayName);

        SerializedProperty cprop, pprop;
        GetCPathProperties(property, out cprop, out pprop);

        EditorGUI.indentLevel++;
        var position = EditorGUILayout.GetControlRect(true);
        ComponentReferenceDrawer.PropertyField(position, cprop);

        if (cprop.objectReferenceValue != null)
        {
            var name = "prop_" + property.propertyPath + "_" + targetId;
            GUI.SetNextControlName(name);
            EditorGUILayout.PropertyField(pprop);

            var orv = cprop.objectReferenceValue;
            Type ortype;
            if (orv is DataContext && resolveDataContext)
                ortype = (orv as DataContext).Type;
            else
                ortype = orv.GetType();

            if (ortype == null)
            {
                // Handle invalid DataContext types
                if (!string.IsNullOrEmpty(pprop.stringValue))
                {
                    var style = new GUIStyle(EditorStyles.textField);
                    style.normal.textColor = Color.red;

                    EditorGUILayout.TextField(string.Format("Error: {0}/{1} is bound to property \"{2}\" of an invalid DataContext Type.",
                        property.displayName,
                        pprop.displayName,
                        pprop.stringValue),
                        style);
                }
            }
            else
            {
                var path = new INPCBinding.PropertyPath(pprop.stringValue, ortype);
                Type rtype;
                var idx = Array.FindIndex(path.PPath, p => p == null);
                if (path.IsValid)
                {
                    rtype = path.PropertyType;
                }
                else
                {
                    rtype = idx - 1 < 0 ? ortype : path.PPath[idx - 1].PropertyType;
                    // Improve handling of invalid DataContext types
                    var style = new GUIStyle(EditorStyles.textField);
                    style.normal.textColor = Color.red;
                    EditorGUILayout.TextField(string.Format("Error: {0}/{1} invalid property \"{2}\" of a valid DataContext.",
                        property.displayName,
                        pprop.displayName,
                        pprop.stringValue),
                        style);
                }

                var lrect = GUILayoutUtility.GetLastRect();
                var props = rtype.GetProperties(BindingFlags.Instance | BindingFlags.Public);
                if (focusedControl == name)
                {
                    var propNames = props.Select(p => p.Name)
                        .OrderByDescending(
                            s => s.IndexOf(path.Parts.LastOrDefault() ?? "", StringComparison.OrdinalIgnoreCase) == 0)
                        .ToArray();

                    var propstring = string.Join("\n",propNames);
                    EditorGUILayout.HelpBox(propstring, MessageType.None);
                }
            }
        }
        else
        {
            // Improve handling of invalid DataContext types
            if (!string.IsNullOrEmpty(pprop.stringValue))
            {
                var style = new GUIStyle(EditorStyles.textField);
                style.normal.textColor = Color.red;

                EditorGUILayout.TextField(string.Format("Error: {0}/{1} is bound to property \"{2}\" of an invalid component object reference.",
                        property.displayName,
                        pprop.displayName,
                        pprop.stringValue),
                        style);
            }
        }

        EditorGUI.indentLevel--;
    }
}
