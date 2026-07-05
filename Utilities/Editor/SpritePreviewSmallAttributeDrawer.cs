using UnityEditor;
using UnityEngine;

namespace CoolTools.Utilities.Editor
{
    [CustomPropertyDrawer(typeof(SpritePreviewSmallAttribute))]
    public class SpritePreviewSmallAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            if (property.propertyType != SerializedPropertyType.ObjectReference)
            {
                EditorGUI.PropertyField(position, property, label);
                EditorGUI.EndProperty();
            }

            var iconRect = new Rect(position)
            {
                x = EditorGUIUtility.labelWidth - 40,
                width = GetPropertyHeight(property, label),
            };

            var sp = (Sprite) property.objectReferenceValue;
            if (sp != null)
            {
                GUI.DrawTexture(iconRect, sp.texture);
            }
        
            EditorGUI.PropertyField(position, property, label);
        
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight * 1.5f;
        }
    }
}