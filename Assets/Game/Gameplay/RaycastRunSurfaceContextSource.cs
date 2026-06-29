using UnityEngine;

namespace Game.Gameplay
{
    public interface IRunSurfaceContextSource
    {
        RunSurfaceContext Current { get; }
    }

    public sealed class RaycastRunSurfaceContextSource : MonoBehaviour, IRunSurfaceContextSource
    {
        [SerializeField] private Transform _probeOrigin;
        [SerializeField] private RunProgressFrameSource _runProgressFrameSource;
        [SerializeField] private float _probeDistance = 1.5f;
        [SerializeField] private LayerMask _surfaceMask = Physics.DefaultRaycastLayers;

        private readonly IRunSurfaceSlopeCalculator _slopeCalculator = new RunSurfaceSlopeCalculator();

        public RunSurfaceContext Current { get; private set; } = new(false, Vector3.up, 0f);

        private void Reset()
        {
            _probeOrigin = transform;
            _runProgressFrameSource = GetComponentInParent<RunProgressFrameSource>();
        }

        private void FixedUpdate()
        {
            Sample();
        }

        private void Sample()
        {
            if (_probeOrigin == null || _runProgressFrameSource == null)
            {
                Current = new RunSurfaceContext(false, Vector3.up, 0f);
                return;
            }

            if (!_runProgressFrameSource.TryCreateSnapshot(_probeOrigin.position, out var frame, out _))
            {
                Current = new RunSurfaceContext(false, Vector3.up, 0f);
                return;
            }

            var ray = new Ray(_probeOrigin.position, -frame.UpDirection);

            if (!Physics.Raycast(ray, out var hit, Mathf.Max(0f, _probeDistance), _surfaceMask, QueryTriggerInteraction.Ignore))
            {
                Current = new RunSurfaceContext(false, Vector3.up, 0f);
                return;
            }

            var downhillDegrees = _slopeCalculator.CalculateForwardDownhillDegrees(hit.normal, frame);
            Current = new RunSurfaceContext(true, hit.normal, downhillDegrees);
        }
    }
}
