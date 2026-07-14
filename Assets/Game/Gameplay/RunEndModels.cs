using System;
using Game.Gameplay.Economy;
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

    internal enum RunEndFixedStepResult
    {
        ContinueRunSteps = 0,
        BlockRemainingRunSteps = 1
    }

    internal readonly struct RunEndObservation
    {
        public RunEndCandidate Candidate { get; }
        public float ElapsedTime { get; }
        public Vector3 Position { get; }
        public Vector3 LinearVelocity { get; }
        public RunProgressSample ProgressSample { get; }
        public float AirTimeSeconds { get; }

        public RunEndObservation(
            RunEndCandidate candidate,
            float elapsedTime,
            Vector3 position,
            Vector3 linearVelocity,
            RunProgressSample progressSample,
            float airTimeSeconds)
        {
            Candidate = candidate;
            ElapsedTime = Mathf.Max(0f, elapsedTime);
            Position = position;
            LinearVelocity = linearVelocity;
            ProgressSample = progressSample;
            AirTimeSeconds = Mathf.Max(0f, airTimeSeconds);
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
        public RunRewardBreakdown RewardBreakdown { get; }
        public RunCurrencySnapshot CurrencySnapshot => RewardBreakdown.CurrencySnapshot;

        public RunResult(
            RunEndReason reason,
            float elapsedTime,
            float distanceTravelled,
            Vector3 finalPosition,
            float finalSpeed,
            RunRewardBreakdown rewardBreakdown)
        {
            Reason = reason;
            IsSuccess = reason == RunEndReason.Finished;
            ElapsedTime = Mathf.Max(0f, elapsedTime);
            DistanceTravelled = Mathf.Max(0f, distanceTravelled);
            FinalPosition = finalPosition;
            FinalSpeed = Mathf.Max(0f, finalSpeed);
            RewardBreakdown = rewardBreakdown ?? throw new ArgumentNullException(nameof(rewardBreakdown));
        }

        public override string ToString()
        {
            return "Run Result: "
                   + $"Reason={Reason}, "
                   + $"IsSuccess={IsSuccess}, "
                   + $"ElapsedTime={ElapsedTime:0.###}, "
                   + $"DistanceTravelled={DistanceTravelled:0.###}, "
                   + $"FinalPosition={FinalPosition}, "
                   + $"FinalSpeed={FinalSpeed:0.###}, "
                   + $"CurrencySnapshot={CurrencySnapshot}";
        }
    }
}
