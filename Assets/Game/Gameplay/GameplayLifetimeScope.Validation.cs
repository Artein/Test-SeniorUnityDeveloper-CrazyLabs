using System;
using System.Collections.Generic;
using System.Linq;
using Game.Gameplay.Pickups;
using UnityEngine;

namespace Game.Gameplay
{
    public sealed partial class GameplayLifetimeScope
    {
        private void OnValidate()
        {
            LogReferenceValidationWarnings();
        }

        private void LogReferenceValidationWarnings()
        {
            foreach (var error in GetReferenceValidationErrors())
            {
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

            if (_runPreparationStateId == null)
                yield return "GameplayLifetimeScope requires a Run Preparation State Id reference.";

            if (_preLaunchStateId == null)
                yield return "GameplayLifetimeScope requires a Pre-Launch State Id reference.";

            if (_runningStateId == null)
                yield return "GameplayLifetimeScope requires a Running State Id reference.";

            if (_runEndedStateId == null)
                yield return "GameplayLifetimeScope requires a Run Ended State Id reference.";

            if (_upgradeCatalog == null)
                yield return "GameplayLifetimeScope requires an Upgrade Catalog reference.";

            if (_slingshotLaunchPowerStatId == null)
                yield return "GameplayLifetimeScope requires a Slingshot Launch Power Stat Id reference.";

            if (_playerMaxSpeedStatId == null)
                yield return "GameplayLifetimeScope requires a Player Max Speed Stat Id reference.";

            if (_playerSteeringResponsivenessStatId == null)
                yield return "GameplayLifetimeScope requires a Player Steering Responsiveness Stat Id reference.";

            if (_coinCurrencyDefinition == null)
                yield return "GameplayLifetimeScope requires a Coin Currency Definition reference.";
            else if (string.IsNullOrWhiteSpace(_coinCurrencyDefinition.SaveId))
                yield return "GameplayLifetimeScope requires Coin Currency Definition to have a stable save id.";

            if (_coinPickupMultiplierStatId == null)
                yield return "GameplayLifetimeScope requires a Coin Pickup Multiplier Stat Id reference.";

            if (_slingshotConfig == null)
                yield return "GameplayLifetimeScope requires a Slingshot Config reference.";

            if (_gameplaySlingshotLaunchConfig == null)
                yield return "GameplayLifetimeScope requires a Gameplay Slingshot Launch Config reference.";

            if (_playerSteeringConfig == null)
                yield return "GameplayLifetimeScope requires a Player Steering Config reference.";

            if (_runCameraConfig == null)
                yield return "GameplayLifetimeScope requires a Run Camera Config reference.";

            if (_runEndConfig == null)
                yield return "GameplayLifetimeScope requires a Run End Config reference.";

            if (_playerSteeringTarget == null)
                yield return "GameplayLifetimeScope requires a Player Steering Target reference.";

            if (_runCameraSource == null)
                yield return "GameplayLifetimeScope requires a Run Camera Source reference.";

            if (_runProgressFrameSource == null)
                yield return "GameplayLifetimeScope requires a Run Progress Frame Source reference.";

            if (_runSurfaceContextSource == null)
                yield return "GameplayLifetimeScope requires a Run Surface Context Source reference.";

            if (_contactNotifier == null)
                yield return "GameplayLifetimeScope requires a Rigidbody Contact Notifier reference.";

            if (_runCameraAnchor == null)
                yield return "GameplayLifetimeScope requires a Run Camera Anchor reference.";

            if (_runCameraRig == null)
                yield return "GameplayLifetimeScope requires a Run Camera Rig reference.";

            if (_inputCamera == null)
                yield return "GameplayLifetimeScope requires an Input Camera reference.";

            if (_slingshotRig == null)
                yield return "GameplayLifetimeScope requires a Slingshot Rig Transform reference.";

            if (_preLaunchSlingshotRigPose == null)
                yield return "GameplayLifetimeScope requires a Pre-Launch Slingshot Rig Pose reference.";

            if (_preLaunchLaunchTargetPose == null)
                yield return "GameplayLifetimeScope requires a Pre-Launch Launch Target Pose reference.";

            if (_slingshotView == null)
                yield return "GameplayLifetimeScope requires a Slingshot View reference.";

            if (_pullHintView == null)
                yield return "GameplayLifetimeScope requires a Pull Hint View reference.";

            if (_runPreparationView == null)
                yield return "GameplayLifetimeScope requires a Run Preparation View reference.";

            if (_launchTarget == null)
                yield return "GameplayLifetimeScope requires a Launch Target reference.";

            if (_characterPresentationView == null)
                yield return "GameplayLifetimeScope requires a Character Presentation View reference.";

            foreach (var error in GetPickupSetupValidationErrors())
                yield return error;
        }

        private IEnumerable<string> GetPickupSetupValidationErrors()
        {
            var validator = new PickupSetupValidator();
            return validator.Validate(GetLevelPickups(), GetPlayerPickupContactColliders(), _playerTag, _playerLayerName, _pickupLayerName);
        }
    }
}
