using JetBrains.Annotations;

namespace Game.Foundation.Time
{
    public interface ITime
    {
        float DeltaTime { get; }
        float FixedDeltaTime { get; }
    }

    [UsedImplicitly]
    public sealed class UnityTime : ITime
    {
        public float DeltaTime => UnityEngine.Time.deltaTime;
        public float FixedDeltaTime => UnityEngine.Time.fixedDeltaTime;
    }
}
