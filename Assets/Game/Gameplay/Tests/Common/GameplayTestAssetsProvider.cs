using Eflatun.SceneReference;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Gameplay.Tests.Common
{
    public sealed class GameplayTestAssetsProvider : BaseTestAssetsProvider<GameplayTestAssetsProvider>
    {
        [field: SerializeField] public SceneReference GameplaySceneRef { get; private set; }
        [field: SerializeField] public InputActionAsset InputActionsAsset { get; private set; }
        [field: SerializeField] public LayerMask RunSurfaceLayerMask { get; private set; }
        [field: SerializeField] public LayerMask CameraTerrainLayerMask { get; private set; }
        [field: SerializeField] public LayerMask CameraObstacleLayerMask { get; private set; }
    }
}
