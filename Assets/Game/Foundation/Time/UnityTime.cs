using JetBrains.Annotations;

namespace Game.Foundation.Time
{
    public interface ITime
    {
        float DeltaTime { get; }
    }

    [UsedImplicitly]
    public sealed class UnityTime : ITime
    {
        public float DeltaTime => UnityEngine.Time.deltaTime;
    }
}
