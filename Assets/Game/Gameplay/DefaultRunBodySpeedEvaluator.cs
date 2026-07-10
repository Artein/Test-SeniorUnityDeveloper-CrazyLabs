using System;
using Game.Utils.Mathematics;
using UnityEngine;

namespace Game.Gameplay
{
    [Flags]
    internal enum RunBodySpeedDecisionContributors
    {
        None = 0,
        DownhillAcceleration = 1 << 0,
        SurfaceSlowdown = 1 << 1,
        AboveEnvelopeResistance = 1 << 2,
        LowSpeedAssist = 1 << 3,
    }

    internal readonly struct RunBodySpeedContext
    {
        public Vector3 CurrentVelocity { get; }
        public bool HasValidGroundedRunSurface { get; }
        public Vector3 SurfaceNormal { get; }
        public float ForwardDownhillDegrees { get; }
        public float CourseForwardAlignment { get; }
        public float ResolvedSoftMaximumSpeed { get; }

        public RunBodySpeedContext(
            Vector3 currentVelocity,
            bool hasValidGroundedRunSurface,
            Vector3 surfaceNormal,
            float forwardDownhillDegrees,
            float courseForwardAlignment,
            float resolvedSoftMaximumSpeed)
        {
            CurrentVelocity = currentVelocity;
            HasValidGroundedRunSurface = hasValidGroundedRunSurface;
            SurfaceNormal = surfaceNormal;
            ForwardDownhillDegrees = forwardDownhillDegrees;
            CourseForwardAlignment = courseForwardAlignment;
            ResolvedSoftMaximumSpeed = resolvedSoftMaximumSpeed;
        }
    }

    internal readonly struct RunBodySpeedDecision
    {
        public float TangentAcceleration { get; }
        public float TangentDrag { get; }
        public float LowSpeedAssistTargetSpeed { get; }
        public float LowSpeedAssistAcceleration { get; }
        public float SoftMaximumSpeed { get; }
        public RunBodySpeedDecisionContributors Contributors { get; }

        public RunBodySpeedDecision(
            float tangentAcceleration,
            float tangentDrag,
            float lowSpeedAssistTargetSpeed,
            float lowSpeedAssistAcceleration,
            float softMaximumSpeed,
            RunBodySpeedDecisionContributors contributors)
        {
            TangentAcceleration = tangentAcceleration;
            TangentDrag = tangentDrag;
            LowSpeedAssistTargetSpeed = lowSpeedAssistTargetSpeed;
            LowSpeedAssistAcceleration = lowSpeedAssistAcceleration;
            SoftMaximumSpeed = softMaximumSpeed;
            Contributors = contributors;
        }
    }

    internal interface IRunBodySpeedEvaluator
    {
        RunBodySpeedDecision Evaluate(RunBodySpeedContext context);
    }

    internal sealed class DefaultRunBodySpeedEvaluator : IRunBodySpeedEvaluator
    {
        private readonly IRunBodySpeedConfig _config;

        public DefaultRunBodySpeedEvaluator(IRunBodySpeedConfig config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        public RunBodySpeedDecision Evaluate(RunBodySpeedContext context)
        {
            if (!context.HasValidGroundedRunSurface
                || !context.CurrentVelocity.IsFinite()
                || !TryNormalize(context.SurfaceNormal, out var surfaceNormal)
                || !float.IsFinite(context.ForwardDownhillDegrees)
                || !float.IsFinite(context.CourseForwardAlignment)
                || !float.IsFinite(context.ResolvedSoftMaximumSpeed)
                || context.ResolvedSoftMaximumSpeed <= 0f)
            {
                return CreateNeutralDecision(context.ResolvedSoftMaximumSpeed);
            }

            var tangentSpeed = Vector3.ProjectOnPlane(context.CurrentVelocity, surfaceNormal).magnitude;

            if (!float.IsFinite(tangentSpeed))
                return CreateNeutralDecision(context.ResolvedSoftMaximumSpeed);

            var positiveDownhillDegrees = Mathf.Clamp(context.ForwardDownhillDegrees, 0f, 90f);
            var positiveCourseAlignment = Mathf.Clamp01(context.CourseForwardAlignment);
            var tangentAcceleration = 0f;
            var tangentDrag = _config.SurfaceSlowdown;
            var lowSpeedAssistTargetSpeed = 0f;
            var lowSpeedAssistAcceleration = 0f;

            var contributors = tangentDrag > 0f
                ? RunBodySpeedDecisionContributors.SurfaceSlowdown
                : RunBodySpeedDecisionContributors.None;

            if (tangentSpeed > context.ResolvedSoftMaximumSpeed)
            {
                var normalizedExcess = Mathf.Clamp01(
                    (tangentSpeed - context.ResolvedSoftMaximumSpeed)
                    / context.ResolvedSoftMaximumSpeed);
                var aboveEnvelopeResistance = _config.AboveMaximumSpeedResistance * normalizedExcess;
                tangentDrag += aboveEnvelopeResistance;

                if (aboveEnvelopeResistance > 0f)
                    contributors |= RunBodySpeedDecisionContributors.AboveEnvelopeResistance;
            }

            if (tangentSpeed < context.ResolvedSoftMaximumSpeed
                && positiveDownhillDegrees > 0f
                && positiveCourseAlignment > 0f)
            {
                tangentAcceleration = _config.DownhillAcceleration
                                      * Mathf.Sin(positiveDownhillDegrees * Mathf.Deg2Rad)
                                      * positiveCourseAlignment;

                if (tangentAcceleration > 0f)
                    contributors |= RunBodySpeedDecisionContributors.DownhillAcceleration;
            }

            var effectiveLowSpeedAssistTarget = Mathf.Min(
                _config.LowSpeedAssistTargetSpeed,
                context.ResolvedSoftMaximumSpeed);

            if (tangentSpeed > 0.001f
                && tangentSpeed < effectiveLowSpeedAssistTarget
                && positiveCourseAlignment > 0f)
            {
                lowSpeedAssistTargetSpeed = effectiveLowSpeedAssistTarget;
                lowSpeedAssistAcceleration = _config.LowSpeedAssistAcceleration * positiveCourseAlignment;

                if (lowSpeedAssistAcceleration > 0f)
                    contributors |= RunBodySpeedDecisionContributors.LowSpeedAssist;
            }

            return new RunBodySpeedDecision(
                tangentAcceleration,
                tangentDrag,
                lowSpeedAssistTargetSpeed,
                lowSpeedAssistAcceleration,
                context.ResolvedSoftMaximumSpeed,
                contributors);
        }

        private RunBodySpeedDecision CreateNeutralDecision(float softMaximumSpeed)
        {
            return new RunBodySpeedDecision(
                0f,
                0f,
                0f,
                0f,
                softMaximumSpeed,
                RunBodySpeedDecisionContributors.None);
        }

        private bool TryNormalize(Vector3 value, out Vector3 normalized)
        {
            normalized = Vector3.zero;

            if (!value.IsFinite() || value.sqrMagnitude <= 0.000001f)
                return false;

            normalized = value.normalized;
            return normalized.IsFinite();
        }
    }
}
