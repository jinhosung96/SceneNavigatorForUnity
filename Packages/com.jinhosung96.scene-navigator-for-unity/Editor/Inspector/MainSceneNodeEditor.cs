using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SceneNavigator.Editor
{
    [CustomEditor(typeof(MainSceneNode), editorForChildClasses: true)]
    public sealed class MainSceneNodeEditor : UnityEditor.Editor
    {
        private SerializedProperty _subList;

        private void OnEnable()
        {
            _subList = serializedObject.FindProperty("subSceneNodeTypeAQNs");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.LabelField("Sub Scene Nodes", EditorStyles.boldLabel);

            for (int i = 0; i < _subList.arraySize; i++)
            {
                EditorGUILayout.BeginHorizontal();
                var prop = _subList.GetArrayElementAtIndex(i);
                BaseSceneNodeEditor.DrawCatalogPopup(prop, SceneNodeKind.Sub, $"Sub {i}");
                if (GUILayout.Button("✕", GUILayout.Width(22)))
                {
                    _subList.DeleteArrayElementAtIndex(i);
                    EditorGUILayout.EndHorizontal();
                    break;
                }
                EditorGUILayout.EndHorizontal();
            }
            if (GUILayout.Button("Add Sub"))
            {
                _subList.arraySize++;
                _subList.GetArrayElementAtIndex(_subList.arraySize - 1).stringValue = string.Empty;
            }

            serializedObject.ApplyModifiedProperties();

            EditorGUILayout.Space(10);
            EditorGUILayout.HelpBox(
                "Subs registered here are loaded Additive together with this Main scene. " +
                "Sub categories shared with the next Main are reused or recreated based on each SubSceneNode's ReusePolicy.",
                MessageType.Info);
        }
    }
}
