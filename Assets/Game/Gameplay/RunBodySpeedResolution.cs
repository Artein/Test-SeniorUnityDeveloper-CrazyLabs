namespace Game.Gameplay
{
    internal readonly struct RunBodySpeedResolution
    {
        public float ResolvedTangentSpeed { get; }
        public RunBodySpeedDecisionContributors PolicyContributors { get; }
        public RunBodySpeedDecisionContributors RequestedContributors { get; }
        public float AccelerationVelocityDelta { get; }
        public float DragVelocityDelta { get; }
        public float RequestedLowSpeedAssistVelocityDelta { get; }
        public RunBodyLowSpeedAssistAttemptSnapshot LowSpeedAssistAttempt { get; }

        public RunBodySpeedResolution(
            float resolvedTangentSpeed,
            RunBodySpeedDecisionContributors policyContributors,
            float accelerationVelocityDelta,
            float dragVelocityDelta,
            float requestedLowSpeedAssistVelocityDelta,
            RunBodyLowSpeedAssistAttemptSnapshot lowSpeedAssistAttempt)
        {
            ResolvedTangentSpeed = resolvedTangentSpeed;
            PolicyContributors = policyContributors;
            AccelerationVelocityDelta = accelerationVelocityDelta;
            DragVelocityDelta = dragVelocityDelta;
            RequestedLowSpeedAssistVelocityDelta = requestedLowSpeedAssistVelocityDelta;
            LowSpeedAssistAttempt = lowSpeedAssistAttempt;

            var requestedContributors = RunBodySpeedDecisionContributors.None;

            if (accelerationVelocityDelta > 0f)
            {
                requestedContributors |= policyContributors
                                         & RunBodySpeedDecisionContributors.DownhillAcceleration;
            }

            if (dragVelocityDelta > 0f)
            {
                requestedContributors |= policyContributors
                                         & (RunBodySpeedDecisionContributors.SurfaceSlowdown
                                            | RunBodySpeedDecisionContributors.AboveEnvelopeResistance);
            }

            if (requestedLowSpeedAssistVelocityDelta > 0f)
                requestedContributors |= RunBodySpeedDecisionContributors.LowSpeedAssist;

            RequestedContributors = requestedContributors;
        }
    }
}
