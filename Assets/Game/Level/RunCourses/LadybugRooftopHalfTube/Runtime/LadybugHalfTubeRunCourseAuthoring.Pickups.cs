using System;
using Game.Gameplay.Pickups;
using UnityEngine;

namespace Game.Level.RunCourses.LadybugRooftopHalfTube
{
    public sealed partial class LadybugHalfTubeRunCourseAuthoring
    {
        [SerializeField] private string _pickupLayerName = "Pickup";
        [SerializeField] private Pickup _regularCoinPickupPrefab;
        [SerializeField] private Pickup _bigCoinPickupPrefab;

        private void CreateCoursePickupLines(CourseSurfaceContext surfaceContext)
        {
            CreateBandOneCoinPickups(surfaceContext);
            CreateBandTwoCoinPickups(surfaceContext);
            CreateRequiredRampCoinArcCue(surfaceContext);
            CreateBandThreeCoinPickups(surfaceContext);
            CreateBandFourCoinPickups(surfaceContext);
            CreateBandFiveCoinPickups(surfaceContext);
        }

        private void CreateBandOneCoinPickups(CourseSurfaceContext surfaceContext)
        {
            CreateCoursePickupLine(
                "Band 1 Safe Center Coin Cue",
                "Band 1 Safe Center Coin",
                false,
                CreateCueLinePositions(surfaceContext, 0f, new[] { 8f, 14f, 20f, 26f, 32f, 38f, 44f, 50f }, 0.75f, 0.2f));

            CreateCoursePickupLine(
                "Band 1 Near-Safe Bank Return Coin Cue",
                "Band 1 Bank Return Coin",
                false,
                CreateFunnelCueLinePositions(surfaceContext, 0.8f, 3.1f, new[] { 52f, 56f, 60f, 64f, 68f, 70f }, 0.6f, 0.15f));
        }

        private void CreateBandTwoCoinPickups(CourseSurfaceContext surfaceContext)
        {
            CreateCoursePickupLine(
                "Band 2 Safe Split Coin Cue",
                "Band 2 Safe Split Coin",
                false,
                CreateCueLinePositions(
                    surfaceContext,
                    new[] { -3.2f, -3.6f, -3.4f, 3.4f, 2.8f, 1.2f, 0f, 3.6f, 3.4f, 2.8f, 1.2f, 0f },
                    new[] { 72f, 78f, 84f, 90f, 102f, 108f, 114f, 122f, 130f, 138f, 144f, 148f },
                    0.75f,
                    0.3f));

            CreateCoursePickupLine(
                "Band 2 Right Gap Starter Coin Cushion",
                "Band 2 Right Gap Coin",
                false,
                CreateCueLinePositions(
                    surfaceContext,
                    new[] { 2.25f, 2.55f, 2.85f, 2.95f, 3.05f, 3.35f, 3.3f, 3.2f, 3.12f, 3.1f, 3f, 2.98f, 2.95f, 2.95f, 2.9f, 2.7f },
                    new[] { 74f, 80f, 86f, 88.5f, 90f, 91f, 92.5f, 94f, 95.5f, 96f, 98f, 99f, 100f, 101f, 102f, 104f },
                    0.55f,
                    0.1f));

            CreateCoursePickupLine(
                "Band 2 Risk Bank Coin Cue",
                "Band 2 Risk Bank Coin",
                true,
                CreateCueLinePositions(
                    surfaceContext,
                    new[] { -4.2f, -4.4f, -4.2f, 4.2f, 4.4f, 4.2f, 4.35f },
                    new[] { 76f, 82f, 88f, 126f, 132f, 138f, 144f },
                    0.75f,
                    0.45f));
        }

        private void CreateBandThreeCoinPickups(CourseSurfaceContext surfaceContext)
        {
            CreateCoursePickupLine(
                "Band 3 Center Fallback Coin Cue",
                "Center Fallback Coin",
                false,
                CreateCueLinePositions(surfaceContext, 0f, new[] { 204f, 208f, 212f, 216f, 222f }, 0.75f, 0.25f));

            CreateCoursePickupLine(
                "Band 3 Bank Reward Coin Cue",
                "Bank Reward Coin",
                true,
                CreateCueLinePositions(surfaceContext, -4.1f, new[] { 228f, 231.5f, 235f, 238.5f, 242f, 245f, 248.5f }, 0.75f, 0.45f));
        }

        private void CreateBandFourCoinPickups(CourseSurfaceContext surfaceContext)
        {
            CreateCoursePickupLine(
                "Band 4 Center Fallback Coin Cue",
                "Band 4 Center Fallback Coin",
                false,
                CreateCueLinePositions(surfaceContext, 0f, new[] { 252f, 256f, 260f, 264f, 268f, 272f, 276f, 280f }, 0.75f, 0.2f));

            CreateCoursePickupLine(
                "Band 4 Side Transfer Reward Coin Cue",
                "Band 4 Side Transfer Coin",
                true,
                CreateCueLinePositions(surfaceContext, 4.1f, new[] { 254f, 259f, 264f, 269f, 274f, 279f }, 0.75f, 0.45f));

            CreateCoursePickupLine(
                "Band 4 Reach Pressure Sparse Coin Cue",
                "Band 4 Reach Glide Coin",
                false,
                CreateCueLinePositions(surfaceContext, 0f, new[] { 286f, 294f, 302f, 310f }, 0.75f, 0.15f));

            CreateCoursePickupLine(
                "Band 4 Optional Ramp Reward Coin Cue",
                "Band 4 Optional Ramp Coin",
                true,
                CreateOptionalRampCueLinePositions(surfaceContext));
        }

        private void CreateBandFiveCoinPickups(CourseSurfaceContext surfaceContext)
        {
            CreateCoursePickupLine(
                "Band 5 Finish Approach Coin Cue",
                "Band 5 Finish Approach Coin",
                false,
                CreateFunnelCueLinePositions(surfaceContext, 2.4f, 0f, new[] { 352f, 357f, 362f, 367f, 372f, 377f, 382f, 388f }, 0.75f, 0.25f));

            CreateCoursePickupLine(
                "Band 5 Final Funnel Coin Cue",
                "Band 5 Final Funnel Coin",
                false,
                CreateCueLinePositions(surfaceContext, 0f, new[] { 392f, 397f, 402f, 407f, 412f, 416f }, 0.85f, 0.2f));
        }

        private void CreateRequiredRampCoinArcCue(CourseSurfaceContext surfaceContext)
        {
            var cueProgresses = new[] { 158f, 162f, 166f, 170f, 174f, 178f, 182f, 186f };
            var cuePositions = new Vector3[cueProgresses.Length];

            for (var cueIndex = 0; cueIndex < cueProgresses.Length; cueIndex += 1)
            {
                var cueProgress = cueProgresses[cueIndex];
                var arcPosition = cueIndex / (cueProgresses.Length - 1f);
                var arcHeight = Mathf.Sin(arcPosition * Mathf.PI) * 1.4f;
                var surfaceHeight = GetRequiredRampCueSurfaceHeight(surfaceContext.RampStartHeight, cueProgress);

                cuePositions[cueIndex] = new Vector3(0f, surfaceHeight + 0.75f + arcHeight, cueProgress);
            }

            CreateCoursePickupLine(
                "Band 3 Required Ramp Coin Arc Cue",
                "Required Ramp Coin",
                false,
                cuePositions);
        }

        private Vector3[] CreateOptionalRampCueLinePositions(CourseSurfaceContext surfaceContext)
        {
            var progresses = new[] { 306f, 312f, 318f, 324f, 330f, 336f, 342f };
            var positions = new Vector3[progresses.Length];
            var lateralPosition = 4.1f;
            var bankLineStartHeight = GetSurfaceHeightAtPosition(surfaceContext, 4f, 310f);

            for (var cueIndex = 0; cueIndex < progresses.Length; cueIndex += 1)
            {
                var cueProgress = progresses[cueIndex];
                var arcPosition = cueIndex / (progresses.Length - 1f);
                var rampHeight = _meshBuilder.GetOptionalBankRampHeight(bankLineStartHeight, cueProgress);

                positions[cueIndex] = new Vector3(
                    lateralPosition,
                    rampHeight + 0.75f + Mathf.Sin(arcPosition * Mathf.PI) * 0.9f,
                    cueProgress);
            }

            return positions;
        }

        private Vector3[] CreateCueLinePositions(
            CourseSurfaceContext surfaceContext,
            float lateralPosition,
            float[] progresses,
            float baseLift,
            float arcHeight)
        {
            var lateralPositions = new float[progresses.Length];

            for (var cueIndex = 0; cueIndex < lateralPositions.Length; cueIndex += 1)
            {
                lateralPositions[cueIndex] = lateralPosition;
            }

            return CreateCueLinePositions(surfaceContext, lateralPositions, progresses, baseLift, arcHeight);
        }

        private Vector3[] CreateCueLinePositions(
            CourseSurfaceContext surfaceContext,
            float[] lateralPositions,
            float[] progresses,
            float baseLift,
            float arcHeight)
        {
            if (lateralPositions.Length != progresses.Length)
                throw new ArgumentException("Lateral position and progress arrays must have equal length.", nameof(lateralPositions));

            var positions = new Vector3[progresses.Length];

            for (var cueIndex = 0; cueIndex < progresses.Length; cueIndex += 1)
            {
                var cueProgress = progresses[cueIndex];
                var arcPosition = cueIndex / (progresses.Length - 1f);
                var lateralPosition = lateralPositions[cueIndex];
                var surfaceHeight = GetSurfaceHeightAtPosition(surfaceContext, lateralPosition, cueProgress);

                positions[cueIndex] = new Vector3(
                    lateralPosition,
                    surfaceHeight + baseLift + Mathf.Sin(arcPosition * Mathf.PI) * arcHeight,
                    cueProgress);
            }

            return positions;
        }

        private Vector3[] CreateFunnelCueLinePositions(
            CourseSurfaceContext surfaceContext,
            float startLateralPosition,
            float endLateralPosition,
            float[] progresses,
            float baseLift,
            float arcHeight)
        {
            var positions = new Vector3[progresses.Length];

            for (var cueIndex = 0; cueIndex < progresses.Length; cueIndex += 1)
            {
                var cueProgress = progresses[cueIndex];
                var arcPosition = cueIndex / (progresses.Length - 1f);
                var lateralPosition = Mathf.Lerp(startLateralPosition, endLateralPosition, arcPosition);
                var surfaceHeight = GetSurfaceHeightAtPosition(surfaceContext, lateralPosition, cueProgress);

                positions[cueIndex] = new Vector3(
                    lateralPosition,
                    surfaceHeight + baseLift + Mathf.Sin(arcPosition * Mathf.PI) * arcHeight,
                    cueProgress);
            }

            return positions;
        }

        private void CreateCoursePickupLine(string pickupRootName, string pickupNamePrefix, bool isRiskRewardLine, Vector3[] pickupPositions)
        {
            var pickupLayer = ResolvePickupLayer();
            var pickupPrefab = ResolvePickupPrefab(isRiskRewardLine);

            var pickupRoot = new GameObject(pickupRootName)
            {
                layer = pickupLayer
            };

            pickupRoot.transform.SetParent(transform, false);

            for (var pickupIndex = 0; pickupIndex < pickupPositions.Length; pickupIndex += 1)
            {
                var pickup = Instantiate(pickupPrefab);

                pickup.name = $"{pickupNamePrefix} {pickupIndex + 1:00}";
                pickup.transform.SetParent(pickupRoot.transform, false);
                pickup.transform.localPosition = pickupPositions[pickupIndex];
                pickup.transform.localRotation = Quaternion.identity;
                SetLayerRecursively(pickup.gameObject, pickupLayer);
            }
        }

        private Pickup ResolvePickupPrefab(bool isRiskRewardLine)
        {
            if (isRiskRewardLine)
            {
                if (_bigCoinPickupPrefab == null)
                    throw new InvalidOperationException("Ladybug half-tube risk/reward coin lines require a Big Coin Pickup prefab reference.");

                return _bigCoinPickupPrefab;
            }

            if (_regularCoinPickupPrefab == null)
                throw new InvalidOperationException("Ladybug half-tube safe coin lines require a Regular Coin Pickup prefab reference.");

            return _regularCoinPickupPrefab;
        }

        private int ResolvePickupLayer()
        {
            var pickupLayer = LayerMask.NameToLayer(_pickupLayerName);

            if (pickupLayer < 0)
                throw new InvalidOperationException($"Ladybug half-tube pickups require Unity layer '{_pickupLayerName}'.");

            return pickupLayer;
        }

        private void SetLayerRecursively(GameObject root, int layer)
        {
            root.layer = layer;

            foreach (Transform child in root.transform)
            {
                SetLayerRecursively(child.gameObject, layer);
            }
        }

        private float GetRequiredRampCueSurfaceHeight(float rampStartHeight, float progress)
        {
            if (progress >= 170f)
                return _meshBuilder.GetRequiredTutorialRampHeight(rampStartHeight, progress);

            return rampStartHeight + Mathf.Tan(9f * Mathf.Deg2Rad) * (170f - progress);
        }
    }
}
