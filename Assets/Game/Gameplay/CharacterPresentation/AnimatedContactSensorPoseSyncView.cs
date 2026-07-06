using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Gameplay.CharacterPresentation
{
    public interface IAnimatedContactSensorPoseSyncView
    {
        Rigidbody RootRigidbody { get; }
        IReadOnlyList<AnimatedContactSensorPoseBinding> Bindings { get; }
    }

    [Serializable]
    public struct AnimatedContactSensorPoseBinding
    {
        [SerializeField] private Transform _source;
        [SerializeField] private Transform _target;

        public Transform Source => _source;
        public Transform Target => _target;

        public AnimatedContactSensorPoseBinding(Transform source, Transform target)
        {
            _source = source;
            _target = target;
        }
    }

    public sealed partial class AnimatedContactSensorPoseSyncView : MonoBehaviour, IAnimatedContactSensorPoseSyncView
    {
        [SerializeField] private Rigidbody _rootRigidbody;
        [SerializeField] private AnimatedContactSensorPoseBinding[] _bindings = Array.Empty<AnimatedContactSensorPoseBinding>();

        public Rigidbody RootRigidbody => _rootRigidbody;
        public IReadOnlyList<AnimatedContactSensorPoseBinding> Bindings => _bindings ?? Array.Empty<AnimatedContactSensorPoseBinding>();
    }
}
