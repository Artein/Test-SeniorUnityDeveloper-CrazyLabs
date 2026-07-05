using Unity.Mathematics;
using UnityEngine;

namespace Game.Utils.Mathematics
{
    public static class Vector3Extensions
    {
        public static bool IsFinite(this Vector3 value)
        {
            return math.isfinite(value.x) && math.isfinite(value.y) && math.isfinite(value.z);
        }
        
        public static bool IsApproximatelyUnit(this Vector3 value)
        {
            return math.abs(value.magnitude - 1f) <= 0.01f;
        }
        
        public static Vector3 Abs(this Vector3 value)
        {
            return new Vector3(math.abs(value.x), math.abs(value.y), math.abs(value.z));
        }
    }
}
