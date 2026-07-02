using JetBrains.Annotations;

namespace Game.Foundation.Screen
{
    public interface IScreen
    {
        int Width { get; }
        int Height { get; }
        float Dpi { get; }
    }

    [UsedImplicitly]
    public sealed class UnityScreen : IScreen
    {
        public int Width => UnityEngine.Screen.width;
        public int Height => UnityEngine.Screen.height;
        public float Dpi => UnityEngine.Screen.dpi;
    }
}
