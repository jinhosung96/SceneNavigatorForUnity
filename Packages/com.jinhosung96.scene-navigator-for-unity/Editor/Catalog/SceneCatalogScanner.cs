using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SceneNavigator.Editor
{
    /// <summary>
    /// Scans every .unity asset in the project, extracts attached SceneNode subclasses, and
    /// writes the result into the SceneCatalog asset. Idempotent.
    /// </summary>
    public static class SceneCatalogScanner
    {
        public static SceneCatalog LoadOrCreateCatalog()
        {
            var settings = SceneNavigatorSettings.GetOrDefault();
            var path = settings.CatalogAssetPath;
            var asset = AssetDatabase.LoadAssetAtPath<SceneCatalog>(path);
            if (asset != null) return asset;

            var dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
                AssetDatabase.Refresh();
            }
            asset = ScriptableObject.CreateInstance<SceneCatalog>();
            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();
            Debug.Log($"[SceneNavigator] Created SceneCatalog at {path}.");
            return asset;
        }

        public static void RebuildAll()
        {
            var catalog = LoadOrCreateCatalog();
            var entries = new List<SceneCatalog.Entry>();
            var seenTypes = new HashSet<string>();

            // Persist user dirty state and reload after scanning.
            var activeScenePath = EditorSceneManager.GetActiveScene().path;

            var sceneGuids = AssetDatabase.FindAssets("t:Scene");
            foreach (var guid in sceneGuids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (string.IsNullOrEmpty(path) || !path.StartsWith("Assets/")) continue;
                var info = ExtractSceneNodeInfo(path);
                if (info == null) continue;

                if (seenTypes.Contains(info.typeAQN))
                {
                    Debug.LogError(
                        $"[SceneNavigator] SceneNode type '{info.typeAQN}' is registered in multiple scenes. " +
                        $"Skipping '{path}'. (1 type = 1 scene)");
                    continue;
                }
                seenTypes.Add(info.typeAQN);
                entries.Add(new SceneCatalog.Entry
                {
                    typeAssemblyQualifiedName = info.typeAQN,
                    kind = info.kind,
                    scenePath = path,
                    sceneGuid = guid,
                });
            }

            ApplyEntries(catalog, entries);

            // Restore the previously active scene (if it still exists)
            if (!string.IsNullOrEmpty(activeScenePath) && File.Exists(activeScenePath))
            {
                try { EditorSceneManager.OpenScene(activeScenePath, OpenSceneMode.Single); }
                catch { /* user might have moved it; harmless */ }
            }

            EditorUtility.SetDirty(catalog);
            AssetDatabase.SaveAssets();
            Debug.Log($"[SceneNavigator] SceneCatalog rebuild complete: {entries.Count} entries.");
        }

        public static void SyncSingleScene(string scenePath)
        {
            if (string.IsNullOrEmpty(scenePath)) return;
            var catalog = LoadOrCreateCatalog();
            var info = ExtractSceneNodeInfo(scenePath);

            var list = catalog.EditorEntries;
            // Remove any existing entry that points to this scene path
            list.RemoveAll(e => e.scenePath == scenePath);

            if (info != null)
            {
                // Detect type duplicates (different scene with same type)
                var conflict = list.FirstOrDefault(e => e.typeAssemblyQualifiedName == info.typeAQN);
                if (conflict != null)
                {
                    Debug.LogError(
                        $"[SceneNavigator] SceneNode type '{info.typeAQN}' is already registered for " +
                        $"scene '{conflict.scenePath}'. Ignoring '{scenePath}'.");
                }
                else
                {
                    list.Add(new SceneCatalog.Entry
                    {
                        typeAssemblyQualifiedName = info.typeAQN,
                        kind = info.kind,
                        scenePath = scenePath,
                        sceneGuid = AssetDatabase.AssetPathToGUID(scenePath),
                    });
                }
            }
            EditorUtility.SetDirty(catalog);
        }

        public static void RemoveByPath(string scenePath)
        {
            if (string.IsNullOrEmpty(scenePath)) return;
            var catalog = LoadOrCreateCatalog();
            var removed = catalog.EditorEntries.RemoveAll(e => e.scenePath == scenePath);
            if (removed > 0) EditorUtility.SetDirty(catalog);
        }

        public static void RenamePath(string oldPath, string newPath)
        {
            if (string.IsNullOrEmpty(oldPath) || string.IsNullOrEmpty(newPath)) return;
            var catalog = LoadOrCreateCatalog();
            bool changed = false;
            foreach (var e in catalog.EditorEntries)
            {
                if (e.scenePath == oldPath) { e.scenePath = newPath; changed = true; }
            }
            if (changed) EditorUtility.SetDirty(catalog);
        }

        private static void ApplyEntries(SceneCatalog catalog, List<SceneCatalog.Entry> entries)
        {
            var list = catalog.EditorEntries;
            list.Clear();
            list.AddRange(entries.OrderBy(e => e.kind).ThenBy(e => e.scenePath));
        }

        // Lightweight per-scene scan: open additively, inspect roots, close.
        // Done synchronously inside editor; small project overhead is acceptable for OnWillSaveAssets / Rebuild.
        internal sealed class SceneNodeInfo
        {
            public string typeAQN;
            public SceneNodeKind kind;
            public string componentGoName;
        }

        internal static SceneNodeInfo ExtractSceneNodeInfo(string scenePath)
        {
            // Avoid touching the active scene if it matches; instead read via temporary additive open.
            var openedHere = false;
            Scene scene = default;
            try
            {
                // If the scene is already loaded, reuse it.
                for (int i = 0; i < SceneManager.sceneCount; i++)
                {
                    var s = SceneManager.GetSceneAt(i);
                    if (s.path == scenePath && s.isLoaded) { scene = s; break; }
                }
                if (!scene.IsValid())
                {
                    scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
                    openedHere = true;
                }

                var roots = scene.GetRootGameObjects();
                SceneNode found = null;
                int count = 0;
                foreach (var root in roots)
                {
                    var nodes = root.GetComponentsInChildren<SceneNode>(includeInactive: true);
                    foreach (var n in nodes)
                    {
                        if (n == null) continue;
                        count++;
                        if (found == null) found = n;
                    }
                }
                if (found == null) return null;
                if (count > 1)
                {
                    Debug.LogError(
                        $"[SceneNavigator] Scene '{scenePath}' has {count} SceneNode components. " +
                        $"Only the first ('{found.GetType().Name}') is registered.");
                }

                var kind = ClassifyKind(found);
                return new SceneNodeInfo
                {
                    typeAQN = found.GetType().AssemblyQualifiedName,
                    kind = kind,
                    componentGoName = found.gameObject.name,
                };
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return null;
            }
            finally
            {
                if (openedHere && scene.IsValid())
                {
                    EditorSceneManager.CloseScene(scene, removeScene: true);
                }
            }
        }

        public static SceneNodeKind ClassifyKind(SceneNode node)
        {
            if (node is BaseSceneNode)     return SceneNodeKind.Base;
            if (node is MainSceneNode)     return SceneNodeKind.Main;
            if (node is SubSceneNode)      return SceneNodeKind.Sub;
            if (node is InstanceSceneNode) return SceneNodeKind.Instance;
            // Defensive default — direct subclassing of SceneNode is not officially supported.
            return SceneNodeKind.Instance;
        }
    }
}
