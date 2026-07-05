using System.Globalization;
using UnityEditor;
using UnityEngine;
using CoolTools.Utilities;

namespace CoolTools.Utilities.Editor
{
    [CustomPropertyDrawer(typeof(ProgressBarAttribute))]
    public class ProgressBarAttributeDrawer : PropertyDrawer {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            var progressBarAttribute = attribute as ProgressBarAttribute;
            
            var maxValueProperty = property.serializedObject.FindProperty(progressBarAttribute.maxPropertyName);
            
            if(maxValueProperty.propertyType != SerializedPropertyType.Integer && maxValueProperty.propertyType != SerializedPropertyType.Float) {
                Debug.LogError("MaxValueAttribute can only be used on int or float fields.");
                return;
            }
            
            if(maxValueProperty.propertyType == SerializedPropertyType.Integer && maxValueProperty.intValue <= 0) {
                Debug.LogError("MaxValueAttribute can only be used on int fields with a value greater than 0.");
                return;
            }
            
            if(maxValueProperty.propertyType == SerializedPropertyType.Float && maxValueProperty.floatValue <= 0f) {
                Debug.LogError("MaxValueAttribute can only be used on float fields with a value greater than 0.");
                return;
            }
            
            var maxValue = maxValueProperty.propertyType == SerializedPropertyType.Integer ? 
                maxValueProperty.intValue : maxValueProperty.floatValue;

            EditorGUI.BeginProperty(position, label, property);
            EditorGUI.LabelField(position, label);

            Rect barPosition = position;
            barPosition.y += EditorGUIUtility.singleLineHeight;
            barPosition.height = EditorGUIUtility.singleLineHeight;

            if(property.propertyType == SerializedPropertyType.Integer)
                EditorGUI.ProgressBar(barPosition, property.intValue / maxValue, property.intValue + "/" + maxValue);
            else if(property.propertyType == SerializedPropertyType.Float)
                EditorGUI.ProgressBar(barPosition, property.floatValue / maxValue, property.floatValue.ToString(CultureInfo.InvariantCulture) + "/" + maxValue.ToString(CultureInfo.InvariantCulture));

            Rect sliderPosition = position;
            sliderPosition.y += EditorGUIUtility.singleLineHeight * 2;
            sliderPosition.height = EditorGUIUtility.singleLineHeight;

            if (property.propertyType == SerializedPropertyType.Integer) {
                property.intValue = EditorGUI.IntSlider(sliderPosition, property.displayName, property.intValue, 0, (int)maxValue);
            } else if (property.propertyType == SerializedPropertyType.Float) {
                property.floatValue = EditorGUI.Slider(sliderPosition, property.displayName, property.floatValue, 0f, maxValue);
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            return EditorGUIUtility.singleLineHeight * 3;
        }
    }

}