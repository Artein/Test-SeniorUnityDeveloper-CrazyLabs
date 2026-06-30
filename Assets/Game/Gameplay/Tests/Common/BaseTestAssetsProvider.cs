using System;
using System.Linq;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Game.Gameplay.Tests.Common
{
    public abstract class BaseTestAssetsProvider<TConcrete> : ScriptableObject
        where TConcrete : BaseTestAssetsProvider<TConcrete>
    {
        public static TConcrete LoadSingleFromAssetDatabase()
        {
#if UNITY_EDITOR
            var assetPaths = AssetDatabase.FindAssets($"t:{typeof(TConcrete).Name}")
                .Where(guid => !string.IsNullOrWhiteSpace(guid))
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(path => !string.IsNullOrWhiteSpace(path))
                .Distinct(StringComparer.Ordinal)
                .OrderBy(path => path, StringComparer.Ordinal)
                .ToArray();

            if (assetPaths.Length == 0)
                throw new InvalidOperationException($"Missing {typeof(TConcrete).Name} asset in {nameof(AssetDatabase)}.");

            if (assetPaths.Length > 1)
                throw new InvalidOperationException(
                    $"Duplicate {typeof(TConcrete).Name} assets in {nameof(AssetDatabase)}: `{string.Join("`, `", assetPaths)}`.");

            var asset = AssetDatabase.LoadAssetAtPath<TConcrete>(assetPaths[0]);

            if (asset == null)
                throw new InvalidOperationException($"Failed loading {typeof(TConcrete).Name} asset at `{assetPaths[0]}`.");

            return asset;
#else
            throw new NotSupportedException("AssetDatabase-backed test assets are editor-only.");
#endif
        }
    }
}
