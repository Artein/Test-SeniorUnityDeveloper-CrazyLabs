using System;
using Game.Utils.Invocation;
using UnityEngine;

namespace Game.Gameplay.Pickups
{
    public sealed partial class Pickup : MonoBehaviour
    {
        [SerializeField] private PickupDefinition _definition;

        public PickupDefinition Definition => _definition;
        public Vector3 Position => transform.position;

        public event Action<Pickup, Collider> TriggerEntered;

        public void SetAvailable(bool isAvailable)
        {
            gameObject.SetActive(isAvailable);
        }

        public void Validate()
        {
            if (_definition == null)
                throw new InvalidOperationException($"Pickup '{name}' requires a Pickup Definition reference.");

            _definition.Validate();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other == null || !isActiveAndEnabled)
                return;

            TriggerEntered?.InvokeSafely(this, other);
        }
    }
}
