using UnityEditor;

namespace SceneNavigator.Editor
{
    public sealed class AssetMovePostprocessor : AssetPostprocessor
    {
        private static void OnPostprocessAllAssets(
            string[] importedAssets,
            string[] deletedAssets,
            string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            bool any = false;
            foreach (var d in deletedAssets)
            {
                if (d.EndsWith(".unity"))
                {
                    SceneCatalogScanner.RemoveByPath(d);
                    any = true;
                }
            }
            for (int i = 0; i < movedAssets.Length; i++)
            {
                var to = movedAssets[i];
                var from = movedFromAssetPaths[i];
                if (to.EndsWith(".unity"))
                {
                    SceneCatalogScanner.RenamePath(from, to);
                    any = true;
                }
            }
            if (any) AssetDatabase.SaveAssets();
        }
    }
}
