using UnityEditor;
using UnityEditor.UI;
using UnityEngine;
using UnityEngine.UI;

namespace uguimvvm.Editor
{
    [CustomEditor(typeof(TabItem)), CanEditMultipleObjects]
    class TabItemEditor : ButtonEditor
    {
        public override void OnInspectorGUI()
        {
            var tabitem = target as TabItem;
            base.OnInspectorGUI();

            serializedObject.Update();
            switch (tabitem.transition)
            {
                case Selectable.Transition.ColorTint:
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("_activeColor"));
                    break;
                default:
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("_active"));
                    break;
            }
            serializedObject.ApplyModifiedProperties();
        }
    }

    static class TabControlHelper
    {
        [MenuItem("GameObject/UI/Tabs/Tabs")]
        static void AddTabs(MenuCommand menuCommand)
        {
            var panel = DefaultControls.CreatePanel(GetStandardResources());
            panel.AddComponent<TabControl>();
            panel.AddComponent<HorizontalLayoutGroup>();

            var rect = panel.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 0.5f);
            rect.anchorMax = new Vector2(1, 0.5f);
            rect.anchoredPosition = Vector2.zero;

            var tab = CreateTab(GetStandardResources());
            SetParentAndAlign(tab, panel);


            PlaceUIElementRoot(panel, menuCommand.context as GameObject);
            if (panel == null) return; //destroyed
            rect.sizeDelta = new Vector2(0, 30f);
            //AnchorZero(panel);
        }

        [MenuItem("GameObject/UI/Tabs/Tab")]
        static void AddTab(MenuCommand menuCommand)
        {
            var context = menuCommand.context as GameObject;
            if (context == null) return;
            var tabs = context.GetComponent<TabControl>();
            if (tabs == null) return;

            var tab = CreateTab(GetStandardResources());
            SetParentAndAlign(tab, context);
            RenameAndUndoCreate(tab, context);
        }

        static GameObject CreateTab(DefaultControls.Resources resources)
        {
            var button = DefaultControls.CreateButton(GetStandardResources());
            Object.DestroyImmediate(button.GetComponent<Button>());
            button.AddComponent<TabItem>();
            button.name = "Tab";
            button.AddComponent<LayoutElement>();
            return button;
        }

        static DefaultControls.Resources s_StandardResources;
        static DefaultControls.Resources GetStandardResources()
        {
            if (s_StandardResources.standard == null)
            {
                s_StandardResources.standard = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
                s_StandardResources.background = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
                s_StandardResources.inputField = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/InputFieldBackground.psd");
                s_StandardResources.knob = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
                s_StandardResources.checkmark = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Checkmark.psd");
                s_StandardResources.dropdown = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/DropdownArrow.psd");
                s_StandardResources.mask = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UIMask.psd");
            }
            return s_StandardResources;
        }

        static void PlaceUIElementRoot(GameObject element, GameObject context)
        {
            if (context == null || context.GetComponentInParent<Canvas>() == null)
            {
                context = GetCanvasGameObject();
            }
            if (context == null)
            {
                Object.DestroyImmediate(element);
                return;
            }

            RenameAndUndoCreate(element, context);
            SetParentUndo(element, context);

            Selection.activeGameObject = element;
        }

        private static void AnchorZero(GameObject panel)
        {
            var rect = panel.GetComponent<RectTransform>();
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = Vector2.zero;
        }

        static void SetParentAndAlign(GameObject child, GameObject parent)
        {
            if (parent == null)
            {
                return;
            }
            child.transform.SetParent(parent.transform, false);
            SetLayerRecursively(child, parent.layer);
        }

        static void SetLayerRecursively(GameObject go, int layer)
        {
            go.layer = layer;
            Transform transform = go.transform;
            for (int i = 0; i < transform.childCount; i++)
            {
                SetLayerRecursively(transform.GetChild(i).gameObject, layer);
            }
        }

        static void RenameAndUndoCreate(GameObject element, GameObject parent)
        {
            var name = GameObjectUtility.GetUniqueNameForSibling(parent.transform, element.name);
            element.name = name;
            Undo.RegisterCreatedObjectUndo(element, "Create " + element.name);
        }

        static void SetParentUndo(GameObject element, GameObject parent)
        {
            Undo.SetTransformParent(element.transform, parent.transform, "Parent " + element.name);
            GameObjectUtility.SetParentAndAlign(element, parent);
        }

        public static GameObject GetCanvasGameObject()
        {
            GameObject activeGameObject = Selection.activeGameObject;
            Canvas canvas = (!(activeGameObject != null)) ? null : activeGameObject.GetComponentInParent<Canvas>();
            if (canvas != null && canvas.gameObject.activeInHierarchy)
            {
                return canvas.gameObject;
            }
            canvas = (UnityEngine.Object.FindObjectOfType(typeof(Canvas)) as Canvas);
            if (canvas != null && canvas.gameObject.activeInHierarchy)
            {
                return canvas.gameObject;
            }
            return null;
        }
    }
}
