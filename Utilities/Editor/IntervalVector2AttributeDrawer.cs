using UnityEditor;
using UnityEngine;

namespace CoolTools.Utilities.Editor
{
    [CustomPropertyDrawer(typeof(IntervalVector2Attribute))]
    public class IntervalVector2AttributeDrawer : PropertyDrawer
    {
        private const float RangeValueWidth = 40;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var interval = attribute as IntervalVector2Attribute;

            EditorGUI.BeginProperty(position, label, property);

            EditorGUI.LabelField(new Rect(position)
            {
                width = EditorGUIUtility.labelWidth
            }, label);

            var propertyBegin = position.x + EditorGUIUtility.labelWidth + 2;

            // Create basic rectangle
            var rect = new Rect(position)
            {
                x = propertyBegin,
                height = EditorGUIUtility.singleLineHeight,
                // width = RangeValueWidth,
            };

            if (property.propertyType != SerializedPropertyType.Vector2 &&
                property.propertyType != SerializedPropertyType.Vector2Int)
            {
                rect.height = EditorGUIUtility.singleLineHeight * 2f;
                EditorGUI.HelpBox(rect, "Use IntervalVector2 Attribute with a Vector2 property.", MessageType.Error);
                return;
            }
            
            var minVal = property.vector2Value.x;
            var maxVal = property.vector2Value.y;

            // Draw min value field
            var fieldRect = new Rect(position)
            {
                x = rect.x,
                width = RangeValueWidth
            };
            minVal = EditorGUI.FloatField(fieldRect, minVal);
            rect.x += rect.width + 5f;

            // Draw Slider
            // rect.width = position.width - propertyBegin - (RangeValueWidth * 2) + 7;
            var sliderRect = new Rect(rect)
            {
                x = fieldRect.xMax + 2,
                width = position.width - propertyBegin - (RangeValueWidth * 2) - 10
            };
            EditorGUI.MinMaxSlider(sliderRect, ref minVal, ref maxVal, interval.min, interval.max);

            // Draw max value field
            // rect.x += rect.width + 5f;
            // rect.width = RangeValueWidth;
            fieldRect = new Rect(fieldRect)
            {
                x = sliderRect.xMax + 2,
            };
            maxVal = EditorGUI.FloatField(fieldRect, maxVal);

            // Keep value presice in three decimal places
            maxVal = Mathf.Round(maxVal * 100f) / 100f;
            minVal = Mathf.Round(minVal * 100f) / 100f;

            maxVal = Mathf.Clamp(maxVal, minVal, interval.max);
            minVal = Mathf.Clamp(minVal, interval.min, maxVal);

            property.vector2Value = new Vector2(minVal, maxVal);

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.Vector2 &&
                property.propertyType != SerializedPropertyType.Vector2Int)
                return EditorGUIUtility.singleLineHeight * 2f;

            return EditorGUIUtility.singleLineHeight;
        }
    }
}