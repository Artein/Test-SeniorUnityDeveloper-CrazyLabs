using System;
using Game.Utils.Mathematics;
using UnityEngine;

namespace Game.Gameplay
{
    public readonly struct LaunchImpulse
    {
        public Vector3 VelocityChange { get; }
        public Vector3 LaunchDirection { get; }
        public Vector3 LaunchUpDirection { get; }
        public float ForwardImpulse { get; }
        public float UpwardImpulse { get; }
        
        public LaunchImpulse(
            Vector3 velocityChange,
            Vector3 launchDirection,
            Vector3 launchUpDirection,
            float forwardImpulse,
            float upwardImpulse)
        {
            if (!velocityChange.IsFinite())
                throw new ArgumentException("Launch impulse velocity change must be finite.", nameof(velocityChange));

            if (!launchDirection.IsFinite() || !launchDirection.IsApproximatelyUnit())
                throw new ArgumentException("Launch impulse direction must be a finite unit vector.", nameof(launchDirection));

            if (!launchUpDirection.IsFinite() || !launchUpDirection.IsApproximatelyUnit())
                throw new ArgumentException("Launch impulse up direction must be a finite unit vector.", nameof(launchUpDirection));

            if (float.IsNaN(forwardImpulse) || float.IsInfinity(forwardImpulse) || forwardImpulse < 0f)
                throw new ArgumentException("Forward launch impulse must be finite and non-negative.", nameof(forwardImpulse));

            if (float.IsNaN(upwardImpulse) || float.IsInfinity(upwardImpulse) || upwardImpulse < 0f)
                throw new ArgumentException("Upward launch impulse must be finite and non-negative.", nameof(upwardImpulse));

            VelocityChange = velocityChange;
            LaunchDirection = launchDirection;
            LaunchUpDirection = launchUpDirection;
            ForwardImpulse = forwardImpulse;
            UpwardImpulse = upwardImpulse;
        }
    }
}
