using System;
using System.Collections.Generic;
using UnityEngine;

namespace SceneNavigator
{
    /// <summary>
    /// Subject of scene transitions. Only one Main scene is active at a time. The associated Sub
    /// scenes are loaded Additive together with this Main and reconciled (reuse vs recreate)
    /// across Main switches based on the SubSceneNode subclass identity.
    /// </summary>
    public abstract class MainSceneNode : SceneNode
    {
        [Tooltip("Assembly-qualified names of SubSceneNode subclasses to load Additive together with this Main scene.")]
        [SerializeField] private List<string> subSceneNodeTypeAQNs = new List<string>();

        private List<Type> _resolvedSubTypesCache;

        public IReadOnlyList<Type> SubSceneNodeTypes
        {
            get
            {
                if (_resolvedSubTypesCache != null) return _resolvedSubTypesCache;
                _resolvedSubTypesCache = new List<Type>(subSceneNodeTypeAQNs.Count);
                for (int i = 0; i < subSceneNodeTypeAQNs.Count; i++)
                {
                    var aqn = subSceneNodeTypeAQNs[i];
                    if (string.IsNullOrEmpty(aqn)) continue;
                    var t = Type.GetType(aqn, throwOnError: false);
                    if (t != null) _resolvedSubTypesCache.Add(t);
                }
                return _resolvedSubTypesCache;
            }
        }

#if UNITY_EDITOR
        internal List<string> EditorSubSceneNodeTypeAQNs => subSceneNodeTypeAQNs;
        internal void EditorClearSubTypeCache() => _resolvedSubTypesCache = null;
#endif
    }
}
