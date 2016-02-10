using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.UI;

namespace uguimvvm.Editor
{
    [CustomEditor(typeof(TabItem))]
    class TabItemEditor : ButtonEditor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            serializedObject.Update();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_active"));
            serializedObject.ApplyModifiedProperties();
        }
    }
}
