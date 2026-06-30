namespace Game.Level.RunCourses.LadybugRooftopHalfTube
{
    public sealed partial class LadybugHalfTubeRunCourseAuthoring
    {
        private readonly struct CourseSurfaceContext
        {
            public CourseSurfaceContext(
                LadybugHalfTubeSectionSpec[] preRampSections,
                LadybugHalfTubeSectionSpec[] postRampSections,
                float rampStartHeight,
                float postRampStartHeight)
            {
                PreRampSections = preRampSections;
                PostRampSections = postRampSections;
                RampStartHeight = rampStartHeight;
                PostRampStartHeight = postRampStartHeight;
            }

            public LadybugHalfTubeSectionSpec[] PreRampSections { get; }
            public LadybugHalfTubeSectionSpec[] PostRampSections { get; }
            public float RampStartHeight { get; }
            public float PostRampStartHeight { get; }
        }

        private readonly struct ObstacleSpec
        {
            public ObstacleSpec(
                string name,
                float lateralPosition,
                float progress,
                float width,
                float height,
                float depth,
                ObstacleVisualKind visualKind)
            {
                Name = name;
                LateralPosition = lateralPosition;
                Progress = progress;
                Width = width;
                Height = height;
                Depth = depth;
                VisualKind = visualKind;
            }

            public string Name { get; }
            public float LateralPosition { get; }
            public float Progress { get; }
            public float Width { get; }
            public float Height { get; }
            public float Depth { get; }
            public ObstacleVisualKind VisualKind { get; }
        }

        private enum ObstacleVisualKind
        {
            Ac1,
            Ac2,
            Sunroof,
            SolarPanels,
            Billboard
        }

        private enum CourseSurfaceFrictionProfile
        {
            EarlyReachPressure,
            CompletionGlide
        }
    }
}
