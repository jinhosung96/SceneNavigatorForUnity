using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SceneNavigator.Editor
{
    /// <summary>
    /// Hooks scene save:
    ///  - syncs catalog entry for the saved scene
    ///  - corrects GameObject name of the SceneNode component to match `SceneNode - {Kind}`
    /// </summary>
    public sealed class SceneSaveProcessor : UnityEditor.AssetModificationProcessor
    {
        public static string[] OnWillSaveAssets(string[] paths)
        {
            for (int i = 0; i < paths.Length; i++)
            {
                var p = paths[i];
                if (!p.EndsWith(".unity")) continue;
                NormalizeNodeNameInOpenScene(p);
                SceneCatalogScanner.SyncSingleScene(p);
            }
            return paths;
        }

        private static void NormalizeNodeNameInOpenScene(string scenePath)
        {
            // Find the loaded Scene matching this path; if not loaded, skip (Sync will handle catalog later via additive open).
            Scene scene = default;
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                var s = SceneManager.GetSceneAt(i);
                if (s.path == scenePath && s.isLoaded) { scene = s; break; }
            }
            if (!scene.IsValid()) return;

            var roots = scene.GetRootGameObjects();
            foreach (var root in roots)
            {
                var node = root.GetComponentInChildren<SceneNode>(includeInactive: true);
                if (node == null) continue;
                var kind = SceneCatalogScanner.ClassifyKind(node);
                var expected = SceneNameNormalizer.Normalize(kind);
                if (node.gameObject.name != expected)
                {
                    Undo.RecordObject(node.gameObject, "Normalize SceneNode name");
                    node.gameObject.name = expected;
                    EditorSceneManager.MarkSceneDirty(scene);
                }
                break;
            }
        }
    }
}
