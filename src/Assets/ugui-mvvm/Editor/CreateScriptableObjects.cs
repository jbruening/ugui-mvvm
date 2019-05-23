using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

class CreateScriptableObjects : ScriptableWizard
{
    private static Type[] _scriptTypes;
    private static string[] _names;
    private int _idx;

    [MenuItem("Assets/Create/Scriptable object")]
    public static void CreateWizard()
    {
        if (_scriptTypes == null)
        {
            var ueda = typeof(ScriptableWizard).Assembly;
            var editorTypes = ueda.GetTypes().Where(t => typeof(ScriptableObject).IsAssignableFrom(t));

            var validAssemblies = AppDomain.CurrentDomain.GetAssemblies().Where(a => !Equals(a, ueda) && !a.FullName.Contains("UnityEditor"));
            //all types deriving from ScriptableObject, that aren't editor-based classes.
            _scriptTypes = validAssemblies
                   .SelectMany(a =>
                   {
                       // some assemblies do not allow querying their type and can result in exceptions
                       try
                       {
                           return a.GetTypes();
                       }
                       catch
                       {
                           return Enumerable.Empty<Type>();
                       }
                   })
                   .Where(t => typeof(ScriptableObject).IsAssignableFrom(t) && !editorTypes.Any(e => e.IsAssignableFrom(t)))
                   .ToArray();
            _names = _scriptTypes.Select(t => t.FullName).ToArray();
        }

        var wizard = DisplayWizard<CreateScriptableObjects>("Create scriptable object");
        wizard._idx = 0;
    }

    protected override bool DrawWizardGUI()
    {
        var idx = EditorGUILayout.Popup(_idx, _names);
        if (idx == _idx) return false;
        _idx = idx;
        return true;
    }

    void OnWizardCreate()
    {
        var type = _scriptTypes[_idx];
        var asset = ScriptableObject.CreateInstance(type);
        if (asset == null)
        {
            EditorUtility.DisplayDialog("Error", string.Format("Cannot create an asset for type {0}", type), "Okay");
            return;
        }
        ProjectWindowUtil.CreateAsset(asset, "New " + type.Name + ".asset");
    }
}