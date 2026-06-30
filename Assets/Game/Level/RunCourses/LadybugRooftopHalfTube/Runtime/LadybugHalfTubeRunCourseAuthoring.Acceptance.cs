using UnityEngine;

namespace Game.Level.RunCourses.LadybugRooftopHalfTube
{
    public sealed partial class LadybugHalfTubeRunCourseAuthoring
    {
        private void EnsureAcceptanceProfile()
        {
            var acceptanceProfile = GetComponent<LadybugHalfTubeRunCourseAcceptanceProfile>();

            if (acceptanceProfile == null)
                acceptanceProfile = gameObject.AddComponent<LadybugHalfTubeRunCourseAcceptanceProfile>();

            acceptanceProfile.ApplyLadybugDefaultsForCourseAuthoring();
        }
    }
}
