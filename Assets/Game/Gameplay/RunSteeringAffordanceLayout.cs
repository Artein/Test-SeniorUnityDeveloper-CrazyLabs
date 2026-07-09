using UnityEngine;

namespace Game.Gameplay
{
    internal interface IRunSteeringAffordanceLayout
    {
        RunSteeringAffordancePresentationState Create(RunSteeringAffordanceSnapshot snapshot);
    }

    internal sealed class RunSteeringAffordanceLayout : IRunSteeringAffordanceLayout
    {
        public RunSteeringAffordancePresentationState Create(RunSteeringAffordanceSnapshot snapshot)
        {
            if (!snapshot.IsActive)
                return new RunSteeringAffordancePresentationState(false, Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, 0f);

            var origin = snapshot.OriginScreenPosition;
            var range = Mathf.Max(0f, snapshot.CapturedRangePixels);
            var deadzoneFraction = Mathf.Clamp(snapshot.CapturedDeadzoneFraction, 0f, 0.95f);
            var horizontalOffset = Mathf.Clamp(snapshot.CurrentScreenPosition.x - origin.x, -range, range);
            var knobPosition = new Vector2(origin.x + horizontalOffset, origin.y);
            var leftRangeEndPosition = new Vector2(origin.x - range, origin.y);
            var rightRangeEndPosition = new Vector2(origin.x + range, origin.y);
            var deadzoneDiameterPixels = range * deadzoneFraction * 2f;

            return new RunSteeringAffordancePresentationState(
                true,
                origin,
                knobPosition,
                leftRangeEndPosition,
                rightRangeEndPosition,
                deadzoneDiameterPixels);
        }
    }
}
