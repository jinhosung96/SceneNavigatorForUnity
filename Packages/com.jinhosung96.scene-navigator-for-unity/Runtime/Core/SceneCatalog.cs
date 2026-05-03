using System;
using System.Collections.Generic;
using UnityEngine;

namespace SceneNavigator
{
    [CreateAssetMenu(menuName = "Scene Navigator/Scene Catalog", fileName = "SceneCatalog")]
    public sealed class SceneCatalog : ScriptableObject
    {
        [Serializable]
        public sealed class Entry
        {
            public string typeAssemblyQualifiedName;
            public SceneNodeKind kind;
            public string scenePath;
            public string sceneGuid;
        }

        [SerializeField] private List<Entry> entries = new List<Entry>();

        public IReadOnlyList<Entry> Entries => entries;

        public bool TryResolve(Type nodeType, out Entry entry)
        {
            if (nodeType == null) { entry = null; return false; }
            var name = nodeType.AssemblyQualifiedName;
            for (int i = 0; i < entries.Count; i++)
            {
                if (entries[i].typeAssemblyQualifiedName == name)
                {
                    entry = entries[i];
                    return true;
                }
            }
            entry = null;
            return false;
        }

#if UNITY_EDITOR
        /// <summary>Editor-only mutable view of catalog entries. Used by SceneCatalogScanner.</summary>
        public List<Entry> EditorEntries => entries;
#endif
    }
}
