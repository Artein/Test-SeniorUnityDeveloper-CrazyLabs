using UnityEngine;

namespace Game.Gameplay
{
    public interface IRunMotionSource
    {
        Vector3 LinearVelocity { get; }

        /// <summary>
        ///     Gets the authoritative physics pose position for gameplay sampling, independent of render interpolation.
        /// </summary>
        Vector3 Position { get; }
    }
}
