using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Game.Gameplay.Pickups.Editor
{
    [CustomEditor(typeof(GameplayLifetimeScope))]
    internal sealed class GameplayLifetimeScopeEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            if (GUILayout.Button("Refresh Pickup References From Scene"))
                RefreshPickupReferences((GameplayLifetimeScope)target);
        }

        private void RefreshPickupReferences(GameplayLifetimeScope scope)
        {
            var scene = scope.gameObject.scene;

            var pickups = scene.GetRootGameObjects()
                .SelectMany(rootGameObject => rootGameObject.GetComponentsInChildren<Pickup>(true))
                .Where(pickup => pickup.gameObject.scene == scene)
                .OrderBy(pickup => GetHierarchyPath(pickup.transform), StringComparer.Ordinal)
                .ThenBy(pickup => pickup.GetInstanceID())
                .ToArray();
            var pickupsProperty = serializedObject.FindProperty("_levelPickups");

            Undo.RecordObject(scope, "Refresh Pickup References From Scene");
            serializedObject.Update();
            pickupsProperty.arraySize = pickups.Length;

            for (var pickupIndex = 0; pickupIndex < pickups.Length; pickupIndex += 1)
            {
                pickupsProperty.GetArrayElementAtIndex(pickupIndex).objectReferenceValue = pickups[pickupIndex];
            }

            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(scope);
        }

        private static string GetHierarchyPath(Transform transform)
        {
            var path = transform.name;
            var parent = transform.parent;

            while (parent != null)
            {
                path = parent.name + "/" + path;
                parent = parent.parent;
            }

            return path;
        }
    }
}
