using CoolTools.Utilities;
using UnityEditor;
using UnityEngine;

namespace CoolTools.Utilities.Editor
{
    [CustomPropertyDrawer(typeof(RequiredAttribute))]
    public class RequiredAttributeDrawer : PropertyDrawer
    {
        private bool error;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            error = property.propertyType != SerializedPropertyType.ObjectReference ||
                    property.objectReferenceValue == null;

            if (property.propertyType == SerializedPropertyType.ObjectReference)
            {
                if (property.objectReferenceValue == null)
                {
                    position.y += 5;
                    EditorGUI.HelpBox(new Rect(position)
                    {
                        height = EditorGUIUtility.singleLineHeight * 1.5f,
                    }, "This Field is required.", MessageType.Error);
                    position.y += EditorGUIUtility.singleLineHeight * 1.5f + 5;
                }
            }
            else
            {
                position.y += 5;
                EditorGUI.HelpBox(new Rect(position)
                {
                    height = EditorGUIUtility.singleLineHeight * 2,
                }, "RequiredAttribute only works on Object Reference properties.", MessageType.Warning);
                position.y += EditorGUIUtility.singleLineHeight * 2 + 5;
            }

            var propHeight = base.GetPropertyHeight(property, label);
            
            var guiContent = new GUIContent(label)
            {
                // Change the GUIContent background depending on error bool
                // image = error ? EditorGUIUtility.IconContent("console.erroricon.sml").image : null
            };

            var originalColor = GUI.backgroundColor;
            GUI.backgroundColor = error ? Color.red : GUI.backgroundColor;
            
            EditorGUI.PropertyField(new Rect(position) { height = propHeight }, 
                property, guiContent, true);

            GUI.backgroundColor = originalColor;
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return error
                ? base.GetPropertyHeight(property, label) + EditorGUIUtility.singleLineHeight * 2 + 5
                : base.GetPropertyHeight(property, label);
        }
    }
}
