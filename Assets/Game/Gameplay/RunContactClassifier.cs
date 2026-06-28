using System;
using Game.Utils.Mathematics;
using UnityEngine;

namespace Game.Gameplay
{
    public interface IRunContactClassifier
    {
        bool TryClassify(RigidbodyCollisionNotification notification, out RunEndCandidate candidate);
        bool TryClassify(RigidbodyTriggerNotification notification, out RunEndCandidate candidate);
    }

    internal sealed class RunContactClassifier : IRunContactClassifier
    {
        private readonly IRunEndConfig _config;

        public RunContactClassifier(IRunEndConfig config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        public bool TryClassify(RigidbodyCollisionNotification notification, out RunEndCandidate candidate)
        {
            candidate = default;

            if (notification == null || !TryGetContact(notification.OtherCollider, out var contact))
                return false;

            switch (contact.Category)
            {
                case RunContactCategory.Finish:
                    candidate = new RunEndCandidate(RunEndReason.Finished);
                    return true;
                case RunContactCategory.SafetyNet:
                    candidate = new RunEndCandidate(RunEndReason.OutOfBounds);
                    return true;
                case RunContactCategory.Obstacle:
                    if (!IsObstacleImpact(notification))
                        return false;

                    candidate = new RunEndCandidate(RunEndReason.ObstacleHit);
                    return true;
                case RunContactCategory.Surface:
                    return false;
                default:
                    return false;
            }
        }

        public bool TryClassify(RigidbodyTriggerNotification notification, out RunEndCandidate candidate)
        {
            candidate = default;

            if (notification == null || !TryGetContact(notification.OtherCollider, out var contact))
                return false;

            switch (contact.Category)
            {
                case RunContactCategory.Finish:
                    candidate = new RunEndCandidate(RunEndReason.Finished);
                    return true;
                case RunContactCategory.SafetyNet:
                    candidate = new RunEndCandidate(RunEndReason.OutOfBounds);
                    return true;
                default:
                    return false;
            }
        }

        private bool TryGetContact(Collider collider, out RunContact contact)
        {
            contact = null;
            return collider != null && collider.TryGetComponent(out contact);
        }

        private bool IsObstacleImpact(RigidbodyCollisionNotification notification)
        {
            var threshold = _config.ObstacleImpactSpeedThreshold;

            if (threshold <= 0f)
                return true;

            for (var contactIndex = 0; contactIndex < notification.ContactCount; contactIndex += 1)
            {
                var contact = notification.GetContact(contactIndex);

                if (!contact.Normal.IsFinite() || contact.Normal.sqrMagnitude <= 0.000001f)
                    continue;

                var normalImpactSpeed = Mathf.Abs(Vector3.Dot(
                    notification.RelativeVelocity,
                    contact.Normal.normalized));

                if (normalImpactSpeed >= threshold)
                    return true;
            }

            return false;
        }
    }
}
