namespace Game.Gameplay
{
    internal enum RunBodySpeedDiagnosticsState
    {
        Inactive = 0,
        Active = 1,
    }

    internal readonly struct RunBodySpeedDiagnosticsSnapshot
    {
        public RunBodySpeedDiagnosticsState State { get; }
        public bool IsRunSurfaceGrounded { get; }
        public bool HasValidGroundedRunSurface { get; }
        public bool HasUsableTangentDirection { get; }
        public float SampledTangentSpeed { get; }
        public float EffectiveSoftMaximumSpeed { get; }
        public float ForwardDownhillDegrees { get; }
        public float CourseForwardAlignment { get; }
        public RunBodySpeedDecisionContributors Contributors { get; }
        public float EffectiveLowSpeedAssistTargetSpeed { get; }
        public RunBodyLowSpeedAssistAttemptState LowSpeedAssistAttemptState { get; }
        public bool IsLowSpeedAssistEligible { get; }
        public float RemainingRequestedLowSpeedAssistVelocityBudget { get; }

        public RunBodySpeedDiagnosticsSnapshot(
            RunBodySpeedDiagnosticsState state,
            bool isRunSurfaceGrounded,
            bool hasValidGroundedRunSurface,
            bool hasUsableTangentDirection,
            float sampledTangentSpeed,
            float effectiveSoftMaximumSpeed,
            float forwardDownhillDegrees,
            float courseForwardAlignment,
            RunBodySpeedDecisionContributors contributors,
            float effectiveLowSpeedAssistTargetSpeed,
            RunBodyLowSpeedAssistAttemptState lowSpeedAssistAttemptState,
            bool isLowSpeedAssistEligible,
            float remainingRequestedLowSpeedAssistVelocityBudget)
        {
            State = state;
            IsRunSurfaceGrounded = isRunSurfaceGrounded;
            HasValidGroundedRunSurface = hasValidGroundedRunSurface;
            HasUsableTangentDirection = hasUsableTangentDirection;
            SampledTangentSpeed = sampledTangentSpeed;
            EffectiveSoftMaximumSpeed = effectiveSoftMaximumSpeed;
            ForwardDownhillDegrees = forwardDownhillDegrees;
            CourseForwardAlignment = courseForwardAlignment;
            Contributors = contributors;
            EffectiveLowSpeedAssistTargetSpeed = effectiveLowSpeedAssistTargetSpeed;
            LowSpeedAssistAttemptState = lowSpeedAssistAttemptState;
            IsLowSpeedAssistEligible = isLowSpeedAssistEligible;
            RemainingRequestedLowSpeedAssistVelocityBudget = remainingRequestedLowSpeedAssistVelocityBudget;
        }
    }

    internal interface IRunBodySpeedDiagnosticsSource
    {
        RunBodySpeedDiagnosticsSnapshot Current { get; }
    }

    internal interface IRunBodySpeedDiagnosticsSink
    {
        void Publish(RunBodySpeedDiagnosticsSnapshot snapshot);
        void Clear();
    }

    internal sealed class RunBodySpeedDiagnostics :
        IRunBodySpeedDiagnosticsSource,
        IRunBodySpeedDiagnosticsSink
    {
        public RunBodySpeedDiagnosticsSnapshot Current { get; private set; }

        public void Publish(RunBodySpeedDiagnosticsSnapshot snapshot)
        {
            Current = snapshot;
        }

        public void Clear()
        {
            Current = default;
        }
    }
}
