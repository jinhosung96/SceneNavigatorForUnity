using UnityEngine;

namespace SceneNavigator
{
    [CreateAssetMenu(menuName = "Scene Navigator/Settings", fileName = "SceneNavigatorSettings")]
    public sealed class SceneNavigatorSettings : ScriptableObject
    {
        public const string DefaultResourcesPath = "SceneNavigator/SceneCatalog";
        public const string DefaultAssetPath = "Assets/Resources/SceneNavigator/SceneCatalog.asset";

        [Tooltip("Resources-relative path used at runtime via Resources.Load<SceneCatalog>(...). " +
                 "Default: 'SceneNavigator/SceneCatalog' (file at Assets/Resources/SceneNavigator/SceneCatalog.asset).")]
        [SerializeField] private string catalogResourcesPath = DefaultResourcesPath;

        [Tooltip("Asset path the editor uses to create/edit the catalog asset.")]
        [SerializeField] private string catalogAssetPath = DefaultAssetPath;

        public string CatalogResourcesPath =>
            string.IsNullOrEmpty(catalogResourcesPath) ? DefaultResourcesPath : catalogResourcesPath;

        public string CatalogAssetPath =>
            string.IsNullOrEmpty(catalogAssetPath) ? DefaultAssetPath : catalogAssetPath;

        private static SceneNavigatorSettings _runtimeInstance;

        public static SceneNavigatorSettings GetOrDefault()
        {
            if (_runtimeInstance != null) return _runtimeInstance;
            _runtimeInstance = Resources.Load<SceneNavigatorSettings>("SceneNavigator/SceneNavigatorSettings");
            if (_runtimeInstance == null)
            {
                _runtimeInstance = CreateInstance<SceneNavigatorSettings>();
            }
            return _runtimeInstance;
        }
    }
}
