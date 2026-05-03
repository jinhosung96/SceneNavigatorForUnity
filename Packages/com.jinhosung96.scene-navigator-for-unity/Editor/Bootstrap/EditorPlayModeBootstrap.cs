using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SceneNavigator.Editor
{
    /// <summary>
    /// When entering Play Mode from a non-Base scene, switch to the Base scene first so the
    /// build-time bootstrap path is reused. The originally active scene's Main type, if any,
    /// will be picked up by SceneNavigatorImpl.ResolveStartupMain via the runtime probe.
    /// </summary>
    [InitializeOnLoad]
    internal static class EditorPlayModeBootstrap
    {
        static EditorPlayModeBootstrap()
        {
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
        }

        private static void OnPlayModeChanged(PlayModeStateChange change)
        {
            if (change != PlayModeStateChange.ExitingEditMode) return;

            var catalog = SceneCatalogScanner.LoadOrCreateCatalog();
            if (catalog == null || catalog.Entries.Count == 0) return;

            // Find the Base scene path
            string basePath = null;
            foreach (var e in catalog.Entries)
            {
                if (e.kind == SceneNodeKind.Base) { basePath = e.scenePath; break; }
            }
            if (string.IsNullOrEmpty(basePath))
            {
                Debug.LogWarning(
                    "[SceneNavigator] No Base scene registered in catalog. " +
                    "Add a BaseSceneNode subclass to a scene and rebuild the catalog.");
                return;
            }

            var active = EditorSceneManager.GetActiveScene();
            if (active.path == basePath) return;

            // Save dirty scenes so user does not lose work
            if (active.isDirty)
            {
                if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                {
                    EditorApplication.isPlaying = false;
                    return;
                }
            }

            // Open Base scene as Single. The runtime bootstrap will look at the previously active
            // scene path to decide the startup Main; we provide that hint via SessionState.
            SessionState.SetString(LastActiveScenePathKey, active.path);
            EditorSceneManager.OpenScene(basePath, OpenSceneMode.Single);
        }

        public const string LastActiveScenePathKey = "SceneNavigator.LastActiveScenePathBeforePlay";
    }
}
