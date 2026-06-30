using UnityEditor;
using UnityEngine;

namespace Game.Gameplay.Economy.Editor
{
    [CustomEditor(typeof(CurrencyDefinition))]
    public sealed class CurrencyDefinitionEditor : UnityEditor.Editor
    {
        private SerializedProperty _saveIdProperty;
        private SerializedProperty _iconProperty;

        private void OnEnable()
        {
            _saveIdProperty = serializedObject.FindProperty(CurrencyDefinition.Serialization.SaveId);
            _iconProperty = serializedObject.FindProperty(CurrencyDefinition.Serialization.Icon);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            var definition = (CurrencyDefinition)target;

            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.PropertyField(_saveIdProperty, new GUIContent("Save ID"));
            }

            DrawSaveIdStatus(definition);
            EditorGUILayout.PropertyField(_iconProperty);
            serializedObject.ApplyModifiedProperties();
        }

        private void DrawSaveIdStatus(CurrencyDefinition definition)
        {
            var status = definition.GetSaveIdStatusForEditor();

            if (status.State == CurrencyDefinitionSaveIdState.Valid || status.State == CurrencyDefinitionSaveIdState.NonAsset)
                return;

            EditorGUILayout.Space();
            EditorGUILayout.HelpBox(GetStatusMessage(status), GetStatusMessageType(status));

            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.TextField("Asset Path", status.AssetPath);
                EditorGUILayout.TextField("Asset GUID", status.AssetGuid);
                EditorGUILayout.TextField("Current Save ID", status.CurrentSaveId);
                EditorGUILayout.TextField("Expected Save ID", status.ExpectedSaveId);
            }

            if (!GUILayout.Button("Set Save ID From Asset GUID"))
                return;

            serializedObject.ApplyModifiedProperties();
            definition.TrySetSaveIdFromAssetGuidForEditor("Set Currency Save ID From Asset GUID");
            serializedObject.Update();
        }

        private MessageType GetStatusMessageType(CurrencyDefinitionSaveIdStatus status)
        {
            return status.State == CurrencyDefinitionSaveIdState.Missing ? MessageType.Warning : MessageType.Error;
        }

        private string GetStatusMessage(CurrencyDefinitionSaveIdStatus status)
        {
            if (status.State == CurrencyDefinitionSaveIdState.Missing)
                return "Currency Definition Save ID is missing. Set it from the asset GUID before using this currency in gameplay content.";

            return
                "Currency Definition Save ID does not match this asset GUID. This usually means the asset was duplicated, edited manually, or its .meta was regenerated. Fix only if this asset should use the current asset identity.";
        }
    }
}
