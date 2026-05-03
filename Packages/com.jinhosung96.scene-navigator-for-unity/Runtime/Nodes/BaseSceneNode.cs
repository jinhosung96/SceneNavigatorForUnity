using System;
using UnityEngine;
using UnityEngine.UI;

namespace SceneNavigator
{
    /// <summary>
    /// Root node living in the Base scene. Owns the SceneNavigator instance, the effect overlay,
    /// the default transition effect, and the startup Main scene reference.
    /// Exactly one Base scene exists in a project; it is loaded Single, never unloaded.
    /// </summary>
    public abstract class BaseSceneNode : SceneNode
    {
        [Tooltip("Assembly-qualified name of the MainSceneNode subclass to load on startup.")]
        [SerializeField] private string startupMainTypeAQN;

        [Tooltip("Default transition effect used when callers pass null. Falls back to TransitionEffects.None when this is null.")]
        [SerializeReference] private ITransitionEffect defaultEffect;

        [Tooltip("Optional. Transform to host transition effect UI. If empty, an overlay Canvas is auto-created at runtime.")]
        [SerializeField] private Transform overlayRoot;

        private Transform _autoOverlayRoot;

        public Type StartupMain
        {
            get
            {
                if (string.IsNullOrEmpty(startupMainTypeAQN)) return null;
                return Type.GetType(startupMainTypeAQN, throwOnError: false);
            }
        }

        public ITransitionEffect DefaultEffect => defaultEffect;

        public Transform ResolveOverlayRoot()
        {
            if (overlayRoot != null) return overlayRoot;
            if (_autoOverlayRoot != null) return _autoOverlayRoot;

            var go = new GameObject("[Effect Overlay]",
                typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            go.transform.SetParent(transform, false);

            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = short.MaxValue - 1;

            _autoOverlayRoot = go.transform;
            return _autoOverlayRoot;
        }

        protected virtual void Awake()
        {
            if (SceneNavigator.Instance == null)
            {
                SceneNavigatorImpl.BootstrapFrom(this);
            }
        }

#if UNITY_EDITOR
        // Editor-only: surface the AQN field so a custom inspector or scanner can write to it.
        internal string EditorStartupMainAQN
        {
            get => startupMainTypeAQN;
            set => startupMainTypeAQN = value;
        }
#endif
    }
}
