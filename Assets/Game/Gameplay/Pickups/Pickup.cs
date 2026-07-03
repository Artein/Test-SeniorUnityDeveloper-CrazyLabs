using System;
using Game.Foundation.Physics;
using Game.Utils.Invocation;
using UnityEngine;

namespace Game.Gameplay.Pickups
{
    public sealed partial class Pickup : MonoBehaviour
    {
        [SerializeField] private PickupDefinition _definition;
        [SerializeField] private TriggerNotifier _triggerNotifier;

        public PickupDefinition Definition => _definition;
        public Vector3 Position => transform.position;

        internal TriggerNotifier TriggerNotifierForValidation => _triggerNotifier;

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

            if (_triggerNotifier == null)
                throw new InvalidOperationException($"Pickup '{name}' requires a Trigger Notifier reference.");
        }

        private void OnEnable()
        {
            _triggerNotifier.TriggerEntered += OnTriggerNotifierEntered;
        }

        private void OnDisable()
        {
            _triggerNotifier.TriggerEntered -= OnTriggerNotifierEntered;
        }

        private void OnTriggerNotifierEntered(Collider other)
        {
            HandleTriggerEntered(other);
        }

        private void HandleTriggerEntered(Collider other)
        {
            if (other == null || !isActiveAndEnabled)
                return;

            TriggerEntered?.InvokeSafely(this, other);
        }
    }
}
