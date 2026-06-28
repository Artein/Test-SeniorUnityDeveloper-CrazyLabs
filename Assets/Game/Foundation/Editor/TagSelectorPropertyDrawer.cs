using Game.Foundation;
using UnityEditor;
using UnityEngine;

namespace Game.Foundation.Editor
{
    [CustomPropertyDrawer(typeof(TagSelectorAttribute))]
    internal sealed class TagSelectorPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.String)
            {
                EditorGUI.PropertyField(position, property, label);
                return;
            }

            property.stringValue = EditorGUI.TagField(position, label, property.stringValue);
        }
    }
}
