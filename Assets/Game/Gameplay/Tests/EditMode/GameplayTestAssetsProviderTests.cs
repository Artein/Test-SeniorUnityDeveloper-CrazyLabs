using Game.Gameplay.Tests.Common;
using NUnit.Framework;

// ReSharper disable once CheckNamespace
public sealed class GameplayTestAssetsProviderTests
{
    [Test]
    public void LoadSingleFromAssetDatabase_WhenProviderExists_ReturnsSerializedGameplayTestAssets()
    {
        var provider = GameplayTestAssetsProvider.LoadSingleFromAssetDatabase();

        Assert.That(provider.GameplaySceneRef.Path, Is.Not.Empty);
        Assert.That(provider.GameplaySceneRef.BuildIndex, Is.GreaterThanOrEqualTo(0));
        Assert.That(provider.InputActionsAsset, Is.Not.Null);
        Assert.That(provider.RunSurfaceLayerMask.value, Is.Not.EqualTo(0));
        Assert.That(provider.CameraTerrainLayerMask.value, Is.Not.EqualTo(0));
        Assert.That(provider.CameraObstacleLayerMask.value, Is.Not.EqualTo(0));
        Assert.That(provider.CoinPickupPrefabs, Is.Not.Null.And.Not.Empty);
        Assert.That(provider.CoinPickupPrefabs, Has.None.Null);
        Assert.That(provider.CoinPickupMesh, Is.Not.Null);
        Assert.That(provider.CoinPickupMaterial, Is.Not.Null);
    }
}
