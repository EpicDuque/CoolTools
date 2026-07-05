using UnityEditor;
using UnityEngine;

namespace CoolTools.Utilities.Editor
{
    [CustomPropertyDrawer(typeof(ResetValueAttribute))]
    public class ResetValueAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var resetValue = attribute as ResetValueAttribute;

            EditorGUI.BeginProperty(position, label, property);

            EditorGUI.PropertyField(new Rect(position)
            {
                width = position.width * 0.85f,
            }, property, label);

            position.x += position.width * 0.85f + 5;

            // Draw Button with custom green texture.
            var oldColor = GUI.backgroundColor;
            GUI.backgroundColor = new Color(0.56f, 0.73f, 0.56f);

            if (GUI.Button(new Rect(position) { width = position.width * 0.15f - 5, }, "Reset"))
            {
                switch (property.propertyType)
                {
                    case SerializedPropertyType.Boolean:
                        property.boolValue = resetValue.resetBool;
                        break;

                    case SerializedPropertyType.Float:
                        property.floatValue = resetValue.resetFloat;
                        break;

                    case SerializedPropertyType.Integer:
                        property.intValue = resetValue.resetInt;
                        break;

                    case SerializedPropertyType.String:
                        property.stringValue = resetValue.resetString;
                        break;

                    case SerializedPropertyType.Vector2:
                        property.vector2Value = resetValue.resetVector2;
                        break;

                    case SerializedPropertyType.Vector3:
                        property.vector3Value = resetValue.resetVector3;
                        break;
                }
            }

            GUI.backgroundColor = oldColor;

            EditorGUI.EndProperty();
        }
    }
}
