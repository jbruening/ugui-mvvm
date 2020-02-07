using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using uguimvvm;
using UnityEditor;
using UnityEditor.Callbacks;
#if UNITY_5_3_OR_NEWER
using UnityEditor.SceneManagement;
#endif
using UnityEngine;

/// <summary>
/// Functionality for generating accessors for known properties at compile-time to avoid their lookup being required at runtime.
/// </summary>
public class PropertyPathAccessorGenerator
{
    private static string _filePath;
    private static StringBuilder _contents;
    private readonly static HashSet<PropertyInfo[]> _paths = new HashSet<PropertyInfo[]>(PropertyPathAccessors.Comparer);

    /// <summary>
    /// The directory in which generated code is placed.
    /// </summary>
    public static string Dir { get; set; }

    /// <summary>
    /// Generates code that registers all the property accessors of all enabled scenes and prefabs.
    /// </summary>
    [MenuItem("Assets/Generate PropertyPathGen")]
    public static void Generate()
    {
        if (Application.isPlaying) return;

        const string title = "Generating property accessors";
        EditorUtility.DisplayProgressBar(title, "", 0);

#if UNITY_5_3_OR_NEWER
        var currentLevel = EditorSceneManager.GetActiveScene();
#else
        var currentLevel = EditorApplication.currentScene;
#endif
        _contents = new StringBuilder();
        _paths.Clear();
        EditorUtility.DisplayProgressBar(title, "Creating header", 0);
        CreateHeader(_contents);

        var scenes = SelectedScenes;
        float total = scenes.Length + 1;

        for (int i = 0; i < scenes.Length; i++)
        {
            var level = scenes[i];
            EditorUtility.DisplayProgressBar(title, "Opening scene " + level, i / total);
#if UNITY_5_3_OR_NEWER
            var scene = EditorSceneManager.OpenScene(level);
            EditorSceneManager.SetActiveScene(scene);
#else
            EditorApplication.OpenScene(level);
#endif
            EditorUtility.DisplayProgressBar(title, "Adding paths in " + level, i / total);
            BuildPathsInScene(_contents);
#if UNITY_5_3_OR_NEWER
            EditorSceneManager.CloseScene(scene, true);
#endif
        }

        EditorUtility.DisplayProgressBar(title, "Generating for prefabs", total - 0.5f / total);
        GenForPrefabs(_contents);

        FinishContents(_contents);

        EditorUtility.DisplayProgressBar(title, "Reopening original", 1);
#if UNITY_5_3_OR_NEWER
        EditorSceneManager.SetActiveScene(currentLevel);
#endif
        var contents = _contents.ToString();
        if (!ValidateContents(contents))
        {
            var sw = new StreamWriter(CreateFile());
            sw.Write(contents);
            FinishFile(sw);

            AssetDatabase.ImportAsset(_filePath.Replace(Application.dataPath, "Assets"));
        }

        ClearState();

        EditorUtility.ClearProgressBar();
    }

    private static FileStream CreateFile()
    {
        ValidateDirectory();
        ValidateFile();

        var file = File.Create(_filePath);
        return file;
    }

    private static void FinishFile(StreamWriter sw)
    {
        sw.Close();
    }

    private static void FinishContents(StringBuilder sw)
    {
        sw.Append("  ppa.Initialize();\r\n}\r\n}\r\n#endregion");
    }

    private static void CreateHeader(StringBuilder sw)
    {
        sw.AppendLine(@"#region GENERATED. Regenerate by menu item Assets/Generate PropertyPathGen
using PropertyBinding = uguimvvm.PropertyBinding;
using ppa = uguimvvm.PropertyPathAccessors;
#if UNITY_WSA || !NET_LEGACY
using System.Collections.ObjectModel;
#else
using uguimvvm;
#endif

class PropertyPathGen
{
[UnityEngine.RuntimeInitializeOnLoadMethod]
static void Register()
{");
    }

    /// <summary>
    /// Cleans up state from work done as part of the <see cref="Generate"/> function.
    /// Invoked by Unity just after building the player.
    /// </summary>
    /// <param name="target"></param>
    /// <param name="pathToBuiltProject"></param>
    [PostProcessBuild(0)]
    public static void FinishedBuilding(BuildTarget target, string pathToBuiltProject)
    {
        if (Application.isPlaying) return;

        ClearState();

        Debug.Log("Build complete!");
    }

    static void ClearState()
    {
        _paths.Clear();
        _contents = null;
        _sceneIndex = 0;
        _currentScenePath = null;
        _scenePaths.Clear();
    }

    #region Private Properties
    private static string[] SelectedSceneNames
    {
        get
        {
            return EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => Path.GetFileName(s.path)).ToArray();
        }
    }

    private static string[] SelectedScenes
    {
        get { return EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray(); }
    }
    #endregion

    private static int _sceneIndex;
    private static string _currentScenePath;
    private static List<string> _scenePaths = new List<string>();

    private static bool GetSceneName()
    {
        if (_sceneIndex >= SelectedSceneNames.Length)
        {
            _currentScenePath = "Unknown";
            return false;
        }
        else
        {
            _currentScenePath = SelectedSceneNames[_sceneIndex++];
            _scenePaths.Add(_currentScenePath);
            return true;
        }
    }

    private static bool ValidateContents(string contents)
    {
        ValidateDirectory();
        ValidateFile();

        return File.ReadAllText(_filePath) == contents;
    }

    private static void ValidateFile()
    {
        _filePath = Path.Combine(Dir, "PropertyPathGen.cs");
    }

    private static void ValidateDirectory()
    {
        Dir = string.Format("{1}{0}{2}{0}{3}", Path.DirectorySeparatorChar, Application.dataPath, "Scripts", "Gen");
        Directory.CreateDirectory(Dir);
    }

    private static void BuildPathsInScene(StringBuilder sw)
    {
        var objects = Resources.FindObjectsOfTypeAll<GameObject>();
        foreach (var obj in objects)
        {
            var bindings = obj.GetComponents<PropertyBinding>();
            foreach (var binding in bindings)
                BuildPaths(binding, sw);
        }
    }

    private static void BuildPaths(PropertyBinding binding, StringBuilder sw)
    {
        var sobj = new SerializedObject(binding);

        BuildPath(sobj.FindProperty("_target"), sw);
        BuildPath(sobj.FindProperty("_source"), sw);
    }

    private static void BuildPath(SerializedProperty prop, StringBuilder sw)
    {
        SerializedProperty cprop;
        SerializedProperty pprop;
        ComponentPathDrawer.GetCPathProperties(prop, out cprop, out pprop);

        var oval = cprop.objectReferenceValue;
        if (oval == null) return;

        var ppath = new PropertyBinding.PropertyPath(pprop.stringValue, oval.GetType());

        if (!ppath.IsValid) return;

        if (_paths.Contains(ppath.PPath))
            return;
        _paths.Add(ppath.PPath);

        sw.AppendFormat(@"  ppa.Register(
    new[]
    {{
{0}
    }},
    obj => 
    {{
        if(obj == null) return null; 
        var v0 = (({1})obj).{2};
{3}
    }},
    (obj, value) =>
    {{
{4}
    }});",
            string.Join(",\r\n", ppath.PPath.Select(p =>
                string.Format("        PropertyBinding.PropertyPath.GetProperty(typeof({0}), \"{1}\")",
                    GetFullName(p.DeclaringType),
                    p.Name))
                    .ToArray()),
            GetFullName(ppath.PPath[0].DeclaringType),
            ppath.Parts[0],
            BuildGetterEnd(ppath),
            BuildSetterEnd(ppath))
            .AppendLine().AppendLine();
    }

    static string BuildGetterEnd(PropertyBinding.PropertyPath ppath)
    {
        if (ppath.Parts.Length == 1)
            return string.Format("        return v0;");

        var sb = new StringBuilder();
        var i = 1;
        for (; i < ppath.Parts.Length - 1; i++)
        {
            sb.AppendFormat("        var v{0} = v{1}.{2}", i, i - 1, ppath.Parts[i])
                .AppendLine()
                .AppendFormat("        if (v{0} == null) return null;", i)
                .AppendLine();
        }

        sb.AppendFormat("        return v{0}.{1};", i - 1, ppath.Parts[i]);

        return sb.ToString();
    }

    static string BuildSetterEnd(PropertyBinding.PropertyPath ppath)
    {
        if (ppath.Parts.Length == 1)
        {
            return string.Format("        (({0})obj).{1} = ({2})value;",
                GetFullName(ppath.PPath[0].DeclaringType),
                ppath.Parts[0],
                GetFullName(ppath.PPath[0].PropertyType));
        }

        var sb = new StringBuilder();
        sb.AppendFormat("        var v0 = (({0})obj).{1};", GetFullName(ppath.PPath[0].DeclaringType), ppath.Parts[0])
            .AppendLine();

        var i = 1;
        for (; i < ppath.Parts.Length - 1; i++)
        {
            sb.AppendFormat("        var v{0} = v{1}.{2}", i, i - 1, ppath.Parts[i])
                .AppendLine()
                .AppendFormat("        if (v{0} == null) return;", i)
                .AppendLine();
        }

        sb.AppendFormat("        v{0}.{1} = ({2})value;", i - 1, ppath.Parts[i], GetFullName(ppath.PPath.Last().PropertyType));

        return sb.ToString();
    }

    static string GetFullName(Type t)
    {
        if (!t.IsGenericType)
            return t.FullName;
        var sb = new StringBuilder();

        sb.Append(t.FullName.Substring(0, t.FullName.LastIndexOf("`", StringComparison.Ordinal)));
        sb.Append(t.GetGenericArguments()
            .Aggregate("<", (aggregate, type) => aggregate + (aggregate == "<" ? "" : ",") + GetFullName(type)));
        sb.Append(">");

        return sb.ToString();
    }

    private static void GenForPrefabs(StringBuilder sw)
    {

    }
}