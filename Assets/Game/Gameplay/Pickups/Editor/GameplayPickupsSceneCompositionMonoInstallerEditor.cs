using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Game.Gameplay.Pickups.Editor
{
    [CustomEditor(typeof(GameplayPickupsSceneCompositionMonoInstaller))]
    internal sealed class GameplayPickupsSceneCompositionMonoInstallerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            if (GUILayout.Button("Refresh Pickup References From Scene"))
                RefreshPickupReferences((GameplayPickupsSceneCompositionMonoInstaller)target);
        }

        private void RefreshPickupReferences(GameplayPickupsSceneCompositionMonoInstaller installer)
        {
            var scene = installer.gameObject.scene;

            var pickups = scene.GetRootGameObjects()
                .SelectMany(rootGameObject => rootGameObject.GetComponentsInChildren<Pickup>(true))
                .Where(pickup => pickup.gameObject.scene == scene)
                .OrderBy(pickup => GetHierarchyPath(pickup.transform), StringComparer.Ordinal)
                .ThenBy(pickup => pickup.GetInstanceID())
                .ToArray();
            var pickupsProperty = serializedObject.FindProperty(GameplayPickupsSceneCompositionMonoInstaller.Serialization.LevelPickups);

            Undo.RecordObject(installer, "Refresh Pickup References From Scene");
            serializedObject.Update();
            pickupsProperty.arraySize = pickups.Length;

            for (var pickupIndex = 0; pickupIndex < pickups.Length; pickupIndex += 1)
            {
                pickupsProperty.GetArrayElementAtIndex(pickupIndex).objectReferenceValue = pickups[pickupIndex];
            }

            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(installer);
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
