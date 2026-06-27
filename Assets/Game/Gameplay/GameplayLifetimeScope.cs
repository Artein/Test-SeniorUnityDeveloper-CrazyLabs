using System;
using System.Collections.Generic;
using System.Linq;
using Game.Foundation.Screen;
using Game.Gameplay.GameplayState;
using Game.Gameplay.Slingshot;
using Game.Input.UnityInput;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.Gameplay
{
    public sealed partial class GameplayLifetimeScope : LifetimeScope
    {
        [SerializeField] private GameplayStateConfig _gameplayStateConfig;
        [SerializeField] private GameplayStateId _preLaunchStateId;
        [SerializeField] private GameplayStateId _runningStateId;
        [SerializeField] private SlingshotConfig _slingshotConfig;
        [SerializeField] private PlayerSteeringConfig _playerSteeringConfig;
        [SerializeField] private RigidbodyPlayerSteeringTarget _playerSteeringTarget;
        [SerializeField] private Camera _inputCamera;
        [SerializeField] private SlingshotView _slingshotView;
        [SerializeField] private RigidbodyLaunchTarget _launchTarget;

        protected override void Configure(IContainerBuilder builder)
        {
            if (builder is null)
                throw new ArgumentNullException(nameof(builder));

            ThrowIfInvalidReferences();
            InstallGameplay(builder);
        }

        private void InstallGameplay(IContainerBuilder builder)
        {
            new UnityInputInstaller().Install(builder);
            new GameplayStateInstaller(_gameplayStateConfig).Install(builder);

            builder.RegisterInstance(_launchTarget).As<ILaunchTarget, IHeldLaunchTarget, ILaunchTargetSilhouetteSource>();
            builder.RegisterInstance(_playerSteeringTarget).As<IPlayerSteeringTarget>();

            new SlingshotInstaller(_slingshotConfig, _preLaunchStateId, _slingshotView, _inputCamera).Install(builder);
            new GameplayFlowInstaller(_runningStateId).Install(builder);

            builder.RegisterInstance(_playerSteeringConfig).As<IPlayerSteeringConfig>();
            builder.Register<IScreen, UnityScreen>(Lifetime.Singleton);

            builder.RegisterEntryPoint<PlayerSteeringController>()
                .WithParameter(_runningStateId);
        }

        private void OnValidate()
        {
            LogReferenceValidationWarnings();
        }

        private void LogReferenceValidationWarnings()
        {
            foreach (var error in GetReferenceValidationErrors())
            {
                // TODO - AI Note: We should use Debug.LogXYZFormat() instead
                Debug.LogWarning(error, this);
            }
        }

        private void ThrowIfInvalidReferences()
        {
            var errors = GetReferenceValidationErrors().ToList();

            if (errors.Any())
                throw new InvalidOperationException(string.Join("\n", errors));
        }

        private IEnumerable<string> GetReferenceValidationErrors()
        {
            if (_gameplayStateConfig == null)
                yield return "GameplayLifetimeScope requires a Gameplay State Config reference.";

            if (_preLaunchStateId == null)
                yield return "GameplayLifetimeScope requires a Pre-Launch State Id reference.";

            if (_runningStateId == null)
                yield return "GameplayLifetimeScope requires a Running State Id reference.";

            if (_slingshotConfig == null)
                yield return "GameplayLifetimeScope requires a Slingshot Config reference.";

            if (_playerSteeringConfig == null)
                yield return "GameplayLifetimeScope requires a Player Steering Config reference.";

            if (_playerSteeringTarget == null)
                yield return "GameplayLifetimeScope requires a Player Steering Target reference.";

            if (_inputCamera == null)
                yield return "GameplayLifetimeScope requires an Input Camera reference.";

            if (_slingshotView == null)
                yield return "GameplayLifetimeScope requires a Slingshot View reference.";

            if (_launchTarget == null)
                yield return "GameplayLifetimeScope requires a Launch Target reference.";
        }
    }
}
