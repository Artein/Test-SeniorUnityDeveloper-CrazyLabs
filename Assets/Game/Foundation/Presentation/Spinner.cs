using UnityEngine;

namespace Game.Foundation.Presentation
{
    public sealed partial class Spinner : MonoBehaviour
    {
        [SerializeField] private Transform _target;
        [SerializeField] private Vector3 _localAxis = Vector3.up;
        [SerializeField] private float _degreesPerSecond = 180f;
        [SerializeField] private bool _useDeterministicInitialPhase = true;
        [SerializeField, Range(0f, 360f)] private float _authoredInitialPhaseOffsetDegrees;

        private Transform _capturedTarget;
        private Quaternion _baseLocalRotation;
        private bool _hasBaseLocalRotation;

        private Transform RotationTarget => _target != null ? _target : transform;

        private void Awake()
        {
            ApplyInitialPhase();
        }

        private void OnEnable()
        {
            ApplyInitialPhase();
        }

        private void Update()
        {
            Tick(UnityEngine.Time.deltaTime);
        }

        private void Tick(float deltaTime)
        {
            if (Mathf.Approximately(deltaTime, 0f) || Mathf.Approximately(_degreesPerSecond, 0f))
                return;

            var target = RotationTarget;
            target.localRotation *= Quaternion.AngleAxis(_degreesPerSecond * deltaTime, ResolveLocalAxis());
        }

        private void ApplyInitialPhase()
        {
            CaptureBaseRotationIfNeeded();

            var target = RotationTarget;
            target.localRotation = _baseLocalRotation * Quaternion.AngleAxis(GetInitialPhaseDegrees(), ResolveLocalAxis());
        }

        private void CaptureBaseRotationIfNeeded()
        {
            var target = RotationTarget;

            if (_hasBaseLocalRotation && _capturedTarget == target)
                return;

            _capturedTarget = target;
            _baseLocalRotation = target.localRotation;
            _hasBaseLocalRotation = true;
        }

        private float GetInitialPhaseDegrees()
        {
            var phaseDegrees = _authoredInitialPhaseOffsetDegrees;

            if (_useDeterministicInitialPhase)
                phaseDegrees += GetDeterministicPhaseDegrees();

            return Mathf.Repeat(phaseDegrees, 360f);
        }

        private float GetDeterministicPhaseDegrees()
        {
            return GetStableTransformHash(RotationTarget) % 36000 / 100f;
        }

        private static uint GetStableTransformHash(Transform target)
        {
            var hash = 2166136261u;
            hash = AddStringToHash(hash, target.name);
            hash = AddVectorToHash(hash, target.position);
            hash = AddVectorToHash(hash, target.localPosition);
            return hash;
        }

        private static uint AddStringToHash(uint hash, string value)
        {
            unchecked
            {
                for (var characterIndex = 0; characterIndex < value.Length; characterIndex += 1)
                {
                    hash ^= value[characterIndex];
                    hash *= 16777619u;
                }

                return hash;
            }
        }

        private static uint AddVectorToHash(uint hash, Vector3 value)
        {
            hash = AddIntToHash(hash, Mathf.RoundToInt(value.x * 1000f));
            hash = AddIntToHash(hash, Mathf.RoundToInt(value.y * 1000f));
            hash = AddIntToHash(hash, Mathf.RoundToInt(value.z * 1000f));
            return hash;
        }

        private static uint AddIntToHash(uint hash, int value)
        {
            unchecked
            {
                hash ^= (uint)value;
                hash *= 16777619u;
                return hash;
            }
        }

        private Vector3 ResolveLocalAxis()
        {
            return _localAxis.sqrMagnitude <= 0.0001f ? Vector3.up : _localAxis.normalized;
        }
    }
}
