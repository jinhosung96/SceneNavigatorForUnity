using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SceneNavigator.Editor
{
    [CustomEditor(typeof(BaseSceneNode), editorForChildClasses: true)]
    public sealed class BaseSceneNodeEditor : UnityEditor.Editor
    {
        private SerializedProperty _startup;
        private SerializedProperty _defaultEffect;
        private SerializedProperty _overlayRoot;

        private void OnEnable()
        {
            _startup = serializedObject.FindProperty("startupMainTypeAQN");
            _defaultEffect = serializedObject.FindProperty("defaultEffect");
            _overlayRoot = serializedObject.FindProperty("overlayRoot");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawCatalogPopup(_startup, SceneNodeKind.Main, "Startup Main");
            EditorGUILayout.PropertyField(_defaultEffect, new GUIContent("Default Effect"), true);
            EditorGUILayout.PropertyField(_overlayRoot,    new GUIContent("Overlay Root (optional)"));

            serializedObject.ApplyModifiedProperties();

            EditorGUILayout.Space(10);
            EditorGUILayout.HelpBox(
                "Overlay Root: leave empty to auto-create a Screen-Space-Overlay Canvas at runtime.\n" +
                "Default Effect: used when callers pass null to Transition. Falls back to TransitionEffects.None.",
                MessageType.Info);
        }

        internal static void DrawCatalogPopup(SerializedProperty stringProp, SceneNodeKind filter, string label)
        {
            var catalog = SceneCatalogScanner.LoadOrCreateCatalog();
            var labels = new List<string> { "<None>" };
            var values = new List<string> { string.Empty };
            foreach (var e in catalog.Entries)
            {
                if (e.kind != filter) continue;
                if (string.IsNullOrEmpty(e.typeAssemblyQualifiedName)) continue;
                var typeName = e.typeAssemblyQualifiedName.Split(',')[0];
                labels.Add($"{typeName}  ({System.IO.Path.GetFileNameWithoutExtension(e.scenePath)})");
                values.Add(e.typeAssemblyQualifiedName);
            }

            int currentIndex = values.IndexOf(stringProp.stringValue);
            if (currentIndex < 0) currentIndex = 0;

            int newIndex = EditorGUILayout.Popup(label, currentIndex, labels.ToArray());
            if (newIndex != currentIndex) stringProp.stringValue = values[newIndex];
        }
    }
}
