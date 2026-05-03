using UnityEditor;

namespace SceneNavigator.Editor
{
    public static class SceneCatalogMenu
    {
        [MenuItem("Tools/Scene Navigator/Rebuild Catalog")]
        public static void RebuildCatalog()
        {
            SceneCatalogScanner.RebuildAll();
        }

        [MenuItem("Tools/Scene Navigator/Open Catalog Asset")]
        public static void OpenCatalogAsset()
        {
            var catalog = SceneCatalogScanner.LoadOrCreateCatalog();
            if (catalog != null) Selection.activeObject = catalog;
        }
    }
}
