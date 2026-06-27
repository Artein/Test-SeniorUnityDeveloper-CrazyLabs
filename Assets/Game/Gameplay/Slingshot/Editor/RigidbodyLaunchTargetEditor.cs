using UnityEditor;
using UnityEngine;

namespace Game.Gameplay.Slingshot.Editor
{
    [CustomEditor(typeof(RigidbodyLaunchTarget))]
    internal sealed class RigidbodyLaunchTargetEditor : UnityEditor.Editor
    {
        private readonly float _surfaceSkinOffset = 0.01f;
        private readonly float _minimumDirectionLength = 0.0001f;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            EditorGUILayout.Space();

            if (GUILayout.Button("Project Band Center To Collider Surface"))
                ProjectBandCenterToColliderSurface();
        }

        private void ProjectBandCenterToColliderSurface()
        {
            serializedObject.Update();

            var collider = serializedObject.FindProperty("_bandContactCollider").objectReferenceValue as Collider;
            var bandCenter = serializedObject.FindProperty("_bandCenter").objectReferenceValue as Transform;

            if (collider == null || bandCenter == null)
            {
                Debug.LogWarning("RigidbodyLaunchTarget requires both Band Contact Collider and Band Center before projection.", target);
                return;
            }

            if (!TryProject(collider, bandCenter, out var projectedPosition))
            {
                Debug.LogWarning(
                    "Could not project Band Center to the assigned collider surface. Move the marker away from the collider center and try again.",
                    target);
                return;
            }

            Undo.RecordObject(bandCenter, "Project Band Center To Collider Surface");
            bandCenter.position = projectedPosition;
            EditorUtility.SetDirty(bandCenter);
        }

        private bool TryProject(Collider collider, Transform bandCenter, out Vector3 projectedPosition)
        {
            var bounds = collider.bounds;
            var direction = bandCenter.position - bounds.center;

            if (direction.sqrMagnitude <= _minimumDirectionLength * _minimumDirectionLength)
            {
                projectedPosition = default;
                return false;
            }

            direction.Normalize();
            var rayDistance = bounds.extents.magnitude + _surfaceSkinOffset + 1f;
            var ray = new Ray(bounds.center, direction);

            if (collider.Raycast(ray, out var hit, rayDistance))
            {
                projectedPosition = hit.point + (hit.normal * _surfaceSkinOffset);
                return true;
            }

            var outsidePoint = bounds.center + (direction * rayDistance);
            var closestPoint = collider.ClosestPoint(outsidePoint);
            var fallbackNormal = outsidePoint - closestPoint;

            if (fallbackNormal.sqrMagnitude <= _minimumDirectionLength * _minimumDirectionLength)
            {
                projectedPosition = default;
                return false;
            }

            projectedPosition = closestPoint + (fallbackNormal.normalized * _surfaceSkinOffset);
            return true;
        }
    }
}
