using System;
using System.IO;
using System.Linq;
using uguimvvm;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

#pragma warning disable CS0618 // Type or member is obsolete
[CustomEditor(typeof(INPCBinding))]
class INPCBindingToPropertyBindingUpgradeEditor : PropertyBindingEditor
{
#if UNITY_2018_3_OR_NEWER
    private MonoScript _oldMonoScript;
    private MonoScript _newMonoScript;
#endif

    public override void OnInspectorGUI()
    {
        if (GUILayout.Button("Upgrade to PropertyBinding"))
        {
            if (EditorUtility.DisplayDialog(
                "Upgrade all INPCBindings in the Unity project?",
                "This will change all INPCBindings in your prefabs and scenes to PropertyBindings.\n\n" +
                    "This action cannot be undone.\n\n" +
                    "You are strongly encouraged to commit unsaved changes to source control before proceeding.",
                "Upgrade now",
                "Cancel"))
            {
                bool okToProceed = true;
#if UNITY_2018_3_OR_NEWER

                if (EditorSettings.serializationMode != SerializationMode.ForceText)
                {
                    okToProceed = false;
                    EditorUtility.DisplayDialog(
                        "Text base assets required",
                        "The upgrade requires that you enable text based assets\n\n" +
                        "Edit > Project Settings > Editor > Asset Serialization > Mode > Force Text\n\n" +
                        "Please set this setting, then save and commit all, then attempt the upgrade again.  You can revert to binary assets after upgrading if you wish.",
                        "Ok");
                }
                else
                {
                    var inpcBinding = this.serializedObject.targetObject as INPCBinding;
                    _oldMonoScript = MonoScript.FromMonoBehaviour(inpcBinding);
                    var propertyBinding = inpcBinding.gameObject.AddComponent<PropertyBinding>();
                    _newMonoScript = MonoScript.FromMonoBehaviour(propertyBinding);
                    DestroyImmediate(propertyBinding);
                }
#endif

                if (okToProceed)
                {
                    // Schedule the upgrade to occur on the next Editor update tick.
                    EditorApplication.update += UpgradeMonoScriptsOnEditorUpdate;
                }
            }
        }

        EditorGUILayout.GetControlRect();

        base.OnInspectorGUI();
    }

    private void UpgradeMonoScriptsOnEditorUpdate()
    {
        try
        {
#if UNITY_2018_3_OR_NEWER
            UpgradeMonoScriptInAllAssets(_oldMonoScript, _newMonoScript);
#else
            UpgradeAllInpcAssetsAndScenes();
#endif
        }
        finally
        {
            EditorApplication.update -= UpgradeMonoScriptsOnEditorUpdate;
        }
    }

#if UNITY_2018_3_OR_NEWER
    private static void UpgradeMonoScriptInAllAssets(MonoScript oldMonoScript, MonoScript newMonoScript)
    {
        string oldGuid;
        long oldFileId;
        if (!AssetDatabase.TryGetGUIDAndLocalFileIdentifier(oldMonoScript, out oldGuid, out oldFileId))
        {
            Debug.LogError($"Failed to get guid for old monoscript - {oldMonoScript}");
            return;
        }

        string newGuid;
        long newFileId;
        if (!AssetDatabase.TryGetGUIDAndLocalFileIdentifier(newMonoScript, out newGuid, out newFileId))
        {
            Debug.LogError($"Failed to get guid for new monoscript - {newMonoScript}");
            return;
        }

        var dataFullPath = Path.GetFullPath(Application.dataPath);
        var prefabGuids = AssetDatabase.FindAssets("t:Prefab");
        var sceneGuids = AssetDatabase.FindAssets("t:Scene");
        var assetPaths = prefabGuids.Concat(sceneGuids)
            .Select((assetGuid) => AssetDatabase.GUIDToAssetPath(assetGuid))
            .Where((path) => Path.GetFullPath(path).StartsWith(dataFullPath));

        foreach (var assetPath in assetPaths)
        {
            // In Unity 2018.3, they added the new prefab workflow (variants and nested prefabs).
            // So in that version and later, we need to update the YAML of the prefabs and scenes by hand
            // to ensure references and overrides still function correctly after the upgrade.
            //
            // In older versions of Unity, TryGetGUIDAndLocalFileIdentifier does not exist, so we must fall back to
            // a simpler technique of just updating the gameObjects.

            try
            {
                var backupFile = assetPath + ".backup";
                File.Copy(assetPath, backupFile);

                bool fileModified = false;

                using (StreamReader reader = new StreamReader(backupFile))
                using (StreamWriter writer = new StreamWriter(assetPath))
                {
                    while (!reader.EndOfStream)
                    {
                        string line = reader.ReadLine();

                        if (line.Contains(oldGuid))
                        {
                            line = line.Replace(oldGuid, newGuid);
                            fileModified = true;
                        }

                        writer.WriteLine(line);
                    }

                    writer.Flush();
                }


                if (fileModified)
                {
                    Debug.Log($"Upgraded - {assetPath}");

                    // Force an asset re-import since we've modified the file.
                    AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
                }

                // If we succeeded without any exceptions, then remove the backup.
                File.Delete(backupFile);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        // Force reload the current scene
        var activeScene = EditorSceneManager.GetActiveScene();
        var activeScenePath = activeScene.path;
        EditorSceneManager.OpenScene(activeScenePath);
    }
#endif

    /// <summary>
    /// Go through the asset database and loaded scenes.  This only works correctly in Unity versions prior to 2018.3
    /// </summary>
    private static void UpgradeAllInpcAssetsAndScenes()
    {
        var assetGuids = AssetDatabase.FindAssets("t:Prefab");
        foreach (var assetGuid in assetGuids)
        {
            var assetPath = AssetDatabase.GUIDToAssetPath(assetGuid);
            var assetGameObject = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (ReplaceAllInpcWithProperty(assetGameObject))
            {
                Debug.Log($"Upgraded - {assetPath}");

                EditorUtility.SetDirty(assetGameObject);
            }
        }
        AssetDatabase.SaveAssets();

        for (int i = 0; i < EditorSceneManager.sceneCount; i++)
        {
            var scene = EditorSceneManager.GetSceneAt(i);
            bool sceneModified = false;
            foreach (var gameObject in scene.GetRootGameObjects())
            {
                if (ReplaceAllInpcWithProperty(gameObject))
                {
                    sceneModified = true;
                }
            }

            if (sceneModified)
            {
                Debug.Log($"Upgraded scene - {scene.name}");

                EditorSceneManager.MarkSceneDirty(scene);
            }
        }
        EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
    }

    private static bool ReplaceAllInpcWithProperty(GameObject gameObject)
    {
        bool anyReplaced = false;

        foreach (var inpcBinding in gameObject.GetComponentsInChildren<INPCBinding>(includeInactive: true))
        {
            anyReplaced = true;
            ReplaceInpcWithProperty(inpcBinding);
        }

        return anyReplaced;
    }

    private static void ReplaceInpcWithProperty(INPCBinding inpcBinding)
    {
        var propertyBinding = inpcBinding.gameObject.AddComponent<PropertyBinding>();
        propertyBinding.CopyFromExisting(inpcBinding);
        DestroyImmediate(inpcBinding, allowDestroyingAssets: true);
    }
}

#pragma warning restore CS0618 // Type or member is obsolete
