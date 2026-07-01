using UnityEngine;

namespace Game.Gameplay
{
    public enum RunContactCategory
    {
        Surface = 0,
        Obstacle = 1,
        SafetyNet = 2,
        Finish = 3
    }

    public sealed partial class RunContact : MonoBehaviour
    {
        [SerializeField] private RunContactCategory _category;

        public RunContactCategory Category => _category;

        internal void SetCategoryForCourseAuthoring(RunContactCategory category)
        {
            _category = category;
        }

        private void OnValidate()
        {
            if (GetComponent<Collider>() == null)
                Debug.LogWarning("RunContact metadata should be placed on the same GameObject as its Collider.", this);
        }
    }
}
