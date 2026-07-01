using UnityEngine;

namespace Game.Level.RunCourses.LadybugRooftopHalfTube
{
    [DisallowMultipleComponent]
    public sealed class LadybugHalfTubeRunCourseAcceptanceProfile : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float _courseLengthMeters = 420f;
        [SerializeField, Min(0f)] private float _requiredRampStartProgress = 170f;
        [SerializeField, Min(0f)] private float _requiredRampEndProgress = 200f;
        [SerializeField, Min(0f)] private float _optionalRampStartProgress = 310f;
        [SerializeField, Min(0f)] private float _optionalRampEndProgress = 350f;
        [SerializeField, Min(0f)] private float _finishProgress = 416f;
        [SerializeField, Min(0f)] private float _upgradedCompletionSecondsMin = 55f;
        [SerializeField, Min(0f)] private float _upgradedCompletionSecondsMax = 65f;
        [SerializeField, Min(0f)] private float _firstUsefulFailureProgressMin = 85f;
        [SerializeField, Min(0f)] private float _firstUsefulFailureProgressMax = 125f;
        [SerializeField, Min(1)] private int _firstCompletionRunCountMin = 5;
        [SerializeField, Min(1)] private int _firstCompletionRunCountMax = 10;
        [SerializeField, Range(0f, 1f)] private float _safePickupRatioMin = 0.65f;
        [SerializeField, Range(0f, 1f)] private float _safePickupRatioMax = 0.75f;
        [SerializeField, Range(0f, 1f)] private float _riskRewardPickupRatioMin = 0.25f;
        [SerializeField, Range(0f, 1f)] private float _riskRewardPickupRatioMax = 0.35f;

        [SerializeField] private string _manualQaReportPath =
            "Assets/Game/Level/RunCourses/LadybugRooftopHalfTube/Design/GRAYBOX_ACCEPTANCE_QA.md";

        [SerializeField] private bool _movingObstaclesAllowed;
        [SerializeField] private bool _routeBranchingAllowed;
        [SerializeField] private bool _hiddenContainmentAllowed;

        public float CourseLengthMeters => _courseLengthMeters;
        public float RequiredRampStartProgress => _requiredRampStartProgress;
        public float RequiredRampEndProgress => _requiredRampEndProgress;
        public float OptionalRampStartProgress => _optionalRampStartProgress;
        public float OptionalRampEndProgress => _optionalRampEndProgress;
        public float FinishProgress => _finishProgress;
        public float UpgradedCompletionSecondsMin => _upgradedCompletionSecondsMin;
        public float UpgradedCompletionSecondsMax => _upgradedCompletionSecondsMax;
        public float FirstUsefulFailureProgressMin => _firstUsefulFailureProgressMin;
        public float FirstUsefulFailureProgressMax => _firstUsefulFailureProgressMax;
        public int FirstCompletionRunCountMin => _firstCompletionRunCountMin;
        public int FirstCompletionRunCountMax => _firstCompletionRunCountMax;
        public float SafePickupRatioMin => _safePickupRatioMin;
        public float SafePickupRatioMax => _safePickupRatioMax;
        public float RiskRewardPickupRatioMin => _riskRewardPickupRatioMin;
        public float RiskRewardPickupRatioMax => _riskRewardPickupRatioMax;
        public string ManualQaReportPath => _manualQaReportPath;
        public bool MovingObstaclesAllowed => _movingObstaclesAllowed;
        public bool RouteBranchingAllowed => _routeBranchingAllowed;
        public bool HiddenContainmentAllowed => _hiddenContainmentAllowed;

        internal void ApplyLadybugDefaultsForCourseAuthoring()
        {
            _courseLengthMeters = 420f;
            _requiredRampStartProgress = 170f;
            _requiredRampEndProgress = 200f;
            _optionalRampStartProgress = 310f;
            _optionalRampEndProgress = 350f;
            _finishProgress = 416f;
            _upgradedCompletionSecondsMin = 55f;
            _upgradedCompletionSecondsMax = 65f;
            _firstUsefulFailureProgressMin = 85f;
            _firstUsefulFailureProgressMax = 125f;
            _firstCompletionRunCountMin = 5;
            _firstCompletionRunCountMax = 10;
            _safePickupRatioMin = 0.65f;
            _safePickupRatioMax = 0.75f;
            _riskRewardPickupRatioMin = 0.25f;
            _riskRewardPickupRatioMax = 0.35f;
            _manualQaReportPath = "Assets/Game/Level/RunCourses/LadybugRooftopHalfTube/Design/GRAYBOX_ACCEPTANCE_QA.md";
            _movingObstaclesAllowed = false;
            _routeBranchingAllowed = false;
            _hiddenContainmentAllowed = false;
        }
    }
}
