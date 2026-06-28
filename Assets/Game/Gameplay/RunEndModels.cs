using System;
using Game.Gameplay.Pickups;
using UnityEngine;

namespace Game.Gameplay
{
    public enum RunEndReason
    {
        Finished = 0,
        ObstacleHit = 1,
        OutOfBounds = 2,
        LostMomentum = 3
    }

    public readonly struct RunEndCandidate
    {
        public RunEndReason Reason { get; }

        public RunEndCandidate(RunEndReason reason)
        {
            Reason = reason;
        }
    }

    public readonly struct RunResult
    {
        public RunEndReason Reason { get; }
        public bool IsSuccess { get; }
        public float ElapsedTime { get; }
        public float DistanceTravelled { get; }
        public Vector3 FinalPosition { get; }
        public float FinalSpeed { get; }
        public RunResourceSnapshot ResourceSnapshot { get; }

        public RunResult(
            RunEndReason reason,
            float elapsedTime,
            float distanceTravelled,
            Vector3 finalPosition,
            float finalSpeed,
            RunResourceSnapshot resourceSnapshot)
        {
            Reason = reason;
            IsSuccess = reason == RunEndReason.Finished;
            ElapsedTime = Mathf.Max(0f, elapsedTime);
            DistanceTravelled = Mathf.Max(0f, distanceTravelled);
            FinalPosition = finalPosition;
            FinalSpeed = Mathf.Max(0f, finalSpeed);
            ResourceSnapshot = resourceSnapshot ?? throw new ArgumentNullException(nameof(resourceSnapshot));
        }

        public override string ToString()
        {
            return "Run Result: "
                   + $"Reason={Reason}, "
                   + $"IsSuccess={IsSuccess}, "
                   + $"ElapsedTime={ElapsedTime:0.###}, "
                   + $"DistanceTravelled={DistanceTravelled:0.###}, "
                   + $"FinalPosition={FinalPosition}, "
                   + $"FinalSpeed={FinalSpeed:0.###}";
        }
    }
}
