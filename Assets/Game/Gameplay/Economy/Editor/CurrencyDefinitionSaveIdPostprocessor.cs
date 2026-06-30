using UnityEditor;

namespace Game.Gameplay.Economy.Editor
{
    public sealed class CurrencyDefinitionSaveIdPostprocessor : AssetPostprocessor
    {
        private static void OnPostprocessAllAssets(
            string[] importedAssets,
            string[] deletedAssets,
            string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            _ = deletedAssets;
            _ = movedFromAssetPaths;

            ReconcileAssetPaths(importedAssets);
            // Unity reports moved assets separately, so re-check moved definitions without a project-wide scan.
            ReconcileAssetPaths(movedAssets);
        }

        private static void ReconcileAssetPaths(string[] assetPaths)
        {
            foreach (var assetPath in assetPaths)
            {
                var definition = AssetDatabase.LoadAssetAtPath<CurrencyDefinition>(assetPath);

                if (definition == null)
                    continue;

                definition.ReconcileSaveIdWithAssetGuidForEditor();
            }
        }
    }
}
