using System;
using Game.Foundation.Physics;
using UnityEngine;

namespace Game.Gameplay.Pickups
{
    public readonly struct PickupContact
    {
        public Pickup Pickup { get; }
        public Collider ContactCollider { get; }
        public TriggerNotifier Sensor { get; }
        public string SensorId { get; }

        public PickupContact(Pickup pickup, Collider contactCollider, TriggerNotifier sensor, string sensorId)
        {
            Pickup = pickup != null ? pickup : throw new ArgumentNullException(nameof(pickup));
            ContactCollider = contactCollider;
            Sensor = sensor;

            if (string.IsNullOrWhiteSpace(sensorId))
                throw new ArgumentException("Pickup Contact requires a non-empty Sensor Id.", nameof(sensorId));

            SensorId = sensorId;
        }
    }

    public interface IPickupContactSource
    {
        event Action<PickupContact> PickupContacted;
    }

    public sealed class EmptyPickupContactSource : IPickupContactSource
    {
        public event Action<PickupContact> PickupContacted
        {
            add { }
            remove { }
        }
    }
}
