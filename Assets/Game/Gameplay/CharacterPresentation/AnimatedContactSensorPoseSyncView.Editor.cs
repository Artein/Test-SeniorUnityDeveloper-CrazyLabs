#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using SaintsField.Playa;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Game.Gameplay.CharacterPresentation
{
    public partial class AnimatedContactSensorPoseSyncView
    {
        [Button("Sync Sensors From Current Bone Pose", hideReturnValue: true)]
        internal int SyncSourcePosesToTargetsForEditor()
        {
            var targets = GetValidBindingTargetsForEditor();

            if (targets.Length <= 0)
                return 0;

            Undo.RecordObjects(targets, "Sync Animated Contact Sensors From Bone Pose");
            var copiedCount = new AnimatedContactSensorPoseSync(this).CopySourcePosesToTargets();

            foreach (var target in targets)
            {
                EditorUtility.SetDirty(target);

                if (PrefabUtility.IsPartOfPrefabInstance(target))
                    PrefabUtility.RecordPrefabInstancePropertyModifications(target);
            }

            var scene = gameObject.scene;

            if (scene.IsValid())
                EditorSceneManager.MarkSceneDirty(scene);

            return copiedCount;
        }

        private Transform[] GetValidBindingTargetsForEditor()
        {
            var bindings = _bindings ?? Array.Empty<AnimatedContactSensorPoseBinding>();
            var targets = new List<Transform>(bindings.Length);
            var uniqueTargets = new HashSet<Transform>();

            foreach (var binding in bindings)
            {
                if (binding.Source == null || binding.Target == null)
                    continue;

                if (uniqueTargets.Add(binding.Target))
                    targets.Add(binding.Target);
            }

            return targets.ToArray();
        }
    }
}

#endif // UNITY_EDITOR
