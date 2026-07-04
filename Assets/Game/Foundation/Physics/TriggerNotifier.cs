using System;
using Game.Utils.Invocation;
using UnityEngine;

namespace Game.Foundation.Physics
{
    public sealed class TriggerNotifier : MonoBehaviour
    {
        public event Action<Collider> TriggerEntered;

        private void OnTriggerEnter(Collider other)
        {
            NotifyTriggerEntered(other);
        }

        private void NotifyTriggerEntered(Collider other)
        {
            if (other == null)
                return;

            TriggerEntered?.InvokeSafely(other);
        }

#if UNITY_INCLUDE_TESTS
        internal void NotifyTriggerEnteredForTests(Collider other)
        {
            NotifyTriggerEntered(other);
        }
#endif // UNITY_INCLUDE_TESTS
    }
}
