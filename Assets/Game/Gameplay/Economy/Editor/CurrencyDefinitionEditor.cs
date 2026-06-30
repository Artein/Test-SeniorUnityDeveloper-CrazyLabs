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

            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.PropertyField(_saveIdProperty, new GUIContent("Save ID"));
            }

            EditorGUILayout.PropertyField(_iconProperty);
            serializedObject.ApplyModifiedProperties();
        }
    }
}
