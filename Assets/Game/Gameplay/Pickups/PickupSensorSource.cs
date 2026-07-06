using System;
using System.Collections.Generic;
using Game.Foundation.Physics;
using Game.Utils.Invocation;
using UnityEngine;

namespace Game.Gameplay.Pickups
{
    public sealed partial class PickupSensorSource : MonoBehaviour, IPickupContactSource
    {
        [SerializeField] private TriggerNotifier[] _sensors = Array.Empty<TriggerNotifier>();

        private readonly Dictionary<TriggerNotifier, Action<Collider>> _handlersBySensor = new();

        public event Action<PickupContact> PickupContacted;

        public IEnumerable<string> GetReferenceValidationErrors(string playerBodyPartLayerName, string pickupLayerName)
        {
            var layerErrors = new List<string>();
            var playerBodyPartLayer = ResolveLayer(playerBodyPartLayerName, "Player Body Part Layer", layerErrors);
            _ = ResolveLayer(pickupLayerName, "Pickup Layer", layerErrors);
            var sensors = _sensors ?? Array.Empty<TriggerNotifier>();

            foreach (var error in layerErrors)
            {
                yield return error;
            }

            if (sensors.Length <= 0)
            {
                yield return $"Pickup Sensor Source '{name}' requires at least one Sensor Entry.";
                yield break;
            }

            var uniqueSensors = new HashSet<TriggerNotifier>(sensors.Length);

            for (var sensorIndex = 0; sensorIndex < sensors.Length; sensorIndex += 1)
            {
                var sensor = sensors[sensorIndex];

                if (sensor == null)
                {
                    yield return $"Pickup Sensor Source '{name}' Sensor Entry at index {sensorIndex} is missing.";
                    continue;
                }

                if (!uniqueSensors.Add(sensor))
                    yield return $"Pickup Sensor Source '{name}' contains duplicate Sensor Entry '{sensor.name}'.";

                foreach (var error in GetSensorValidationErrors(sensor, playerBodyPartLayer, playerBodyPartLayerName))
                {
                    yield return error;
                }
            }
        }

        private void OnEnable()
        {
            SubscribeSensors();
        }

        private void OnDisable()
        {
            UnsubscribeSensors();
        }

        private int ResolveLayer(string layerName, string label, ICollection<string> errors)
        {
            if (string.IsNullOrWhiteSpace(layerName))
            {
                errors.Add($"Pickup Sensor Source '{name}' requires a configured {label} name.");
                return -1;
            }

            var layer = LayerMask.NameToLayer(layerName);

            if (layer < 0)
                errors.Add($"Pickup Sensor Source '{name}' requires Unity layer '{layerName}' for {label}.");

            return layer;
        }

        private IEnumerable<string> GetSensorValidationErrors(
            TriggerNotifier sensor,
            int playerBodyPartLayer,
            string playerBodyPartLayerName)
        {
            var colliders = sensor.GetComponents<Collider>();

            if (colliders.Length <= 0)
            {
                yield return
                    $"Pickup Sensor Source '{name}' Sensor Entry '{sensor.name}' requires an enabled trigger Collider on the same GameObject.";
                yield break;
            }

            var hasEnabledTriggerCollider = false;

            foreach (var collider in colliders)
            {
                if (collider.enabled && collider.isTrigger)
                    hasEnabledTriggerCollider = true;

                if (collider.enabled && collider.isTrigger && playerBodyPartLayer >= 0 && collider.gameObject.layer != playerBodyPartLayer)
                {
                    yield return
                        $"Pickup Sensor Source '{name}' Sensor Entry '{sensor.name}' collider '{collider.name}' must be on Player Body Part Layer '{playerBodyPartLayerName}'.";
                }
            }

            if (!hasEnabledTriggerCollider)
            {
                yield return
                    $"Pickup Sensor Source '{name}' Sensor Entry '{sensor.name}' requires an enabled trigger Collider on the same GameObject.";
            }
        }

        private void SubscribeSensors()
        {
            UnsubscribeSensors();
            var sensors = _sensors ?? Array.Empty<TriggerNotifier>();
            var uniqueSensors = new HashSet<TriggerNotifier>();

            foreach (var sensor in sensors)
            {
                if (sensor == null || !uniqueSensors.Add(sensor))
                    continue;

                Action<Collider> handler = other => OnSensorTriggerEntered(sensor, other);
                _handlersBySensor.Add(sensor, handler);
                sensor.TriggerEntered += handler;
            }
        }

        private void UnsubscribeSensors()
        {
            foreach (var pair in _handlersBySensor)
            {
                if (pair.Key == null)
                    continue;

                pair.Key.TriggerEntered -= pair.Value;
            }

            _handlersBySensor.Clear();
        }

        private void OnSensorTriggerEntered(TriggerNotifier sensor, Collider other)
        {
            if (sensor == null || other == null || !isActiveAndEnabled)
                return;

            var pickup = other.GetComponentInParent<Pickup>();

            if (pickup == null)
                return;

            PickupContacted?.InvokeSafely(new PickupContact(pickup, other, sensor, GetSensorId(sensor)));
        }

        private string GetSensorId(TriggerNotifier sensor)
        {
            var sensorTransform = sensor.transform;
            var path = sensorTransform.name;
            var current = sensorTransform.parent;

            while (current != null)
            {
                path = $"{current.name}/{path}";

                if (current == transform)
                    break;

                current = current.parent;
            }

            return path;
        }
    }
}
