using Unity.Mathematics;

namespace Game.Utils.Mathematics
{
    public static class FloatExtensions
    {
        public static bool IsFiniteNegative(this float value)
        {
            return math.isfinite(value) && value < 0f;
        }

        public static bool IsFinitePositive(this float value)
        {
            return math.isfinite(value) && !(value < 0f);
        }
    }
}
