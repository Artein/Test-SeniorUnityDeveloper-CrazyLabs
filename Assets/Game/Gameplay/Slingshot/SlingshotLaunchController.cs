using System;
using Game.Gameplay.GameplayState;
using Game.Utils.Mathematics;
using Unity.Mathematics;
using UnityEngine;
using VContainer.Unity;

namespace Game.Gameplay.Slingshot
{
    public interface ISlingshotLauncher
    {
        void Launch(SlingshotLaunchRequest request);
    }

    public sealed class SlingshotLaunchController : IInitializable, IDisposable, ISlingshotLauncher
    {
        private readonly ILaunchTarget _launchTarget;
        private readonly IGameplayStateService _gameplayStateService;
        private readonly GameplayStateId _preLaunchStateId;

        private bool _isInitialized;
        private bool _isDisposed;

        public SlingshotLaunchController(ILaunchTarget launchTarget, IGameplayStateService gameplayStateService, GameplayStateId preLaunchStateId)
        {
            _launchTarget = launchTarget ?? throw new ArgumentNullException(nameof(launchTarget));
            _gameplayStateService = gameplayStateService ?? throw new ArgumentNullException(nameof(gameplayStateService));
            _preLaunchStateId = preLaunchStateId != null ? preLaunchStateId : throw new ArgumentNullException(nameof(preLaunchStateId));
        }

        void IInitializable.Initialize()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(nameof(SlingshotLaunchController));

            if (_isInitialized)
                return;

            _gameplayStateService.GameplayStateChanged += HandleGameplayStateChanged;
            _isInitialized = true;

            if (_gameplayStateService.IsCurrent(_preLaunchStateId))
                _launchTarget.Hold();
        }

        void IDisposable.Dispose()
        {
            if (_isDisposed)
                return;

            _isDisposed = true;

            _gameplayStateService.GameplayStateChanged -= HandleGameplayStateChanged;
        }

        public void Launch(SlingshotLaunchRequest request)
        {
            if (_isDisposed)
                throw new ObjectDisposedException(nameof(SlingshotLaunchController));

            if (!IsValidRequest(request))
            {
                Debug.LogWarning("Invalid Slingshot launch request. Launch skipped.");
                return;
            }

            var finalVelocity = (request.LaunchDirection * request.LaunchSpeed)
                                + (request.LaunchUpDirection * request.LaunchUpSpeed);

            if (!finalVelocity.IsFinite() || finalVelocity.sqrMagnitude <= 0.000001f)
            {
                Debug.LogWarning("Invalid Slingshot final velocity. Launch skipped.");
                return;
            }

            _launchTarget.Launch(finalVelocity);
        }

        private void HandleGameplayStateChanged(GameplayStateId nextStateId, GameplayStateId previousStateId)
        {
            if (ReferenceEquals(nextStateId, _preLaunchStateId))
                _launchTarget.Hold();
        }

        private bool IsValidRequest(SlingshotLaunchRequest request)
        {
            return request.LaunchDirection.IsFinite()
                   && request.LaunchDirection.IsApproximatelyUnit()
                   && request.LaunchUpDirection.IsFinite()
                   && request.LaunchUpDirection.IsApproximatelyUnit()
                   && math.isfinite(request.LaunchSpeed)
                   && request.LaunchSpeed >= 0f
                   && math.isfinite(request.LaunchUpSpeed)
                   && request.LaunchUpSpeed >= 0f;
        }
    }
}
