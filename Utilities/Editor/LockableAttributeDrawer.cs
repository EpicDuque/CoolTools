using CoolTools.Utilities;
using UnityEditor;
using UnityEngine;

namespace CoolTools.Utilities.Editor
{
    
    [CustomPropertyDrawer(typeof(LockableAttribute))]
    public class LockableAttributeDrawer : PropertyDrawer
    {
        private bool locked = true;
        
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var icon = Resources.Load("Icons/Icon_Lock") as Texture;
            
            EditorGUI.BeginProperty(position, label, property);
            
            EditorGUI.BeginDisabledGroup(locked);
            EditorGUI.PropertyField(new Rect(position)
            {
                width = position.width * 0.9f,
            }, property, label);
            EditorGUI.EndDisabledGroup();
            
            position.x += position.width * 0.9f + 4;
            
            var oldColor = GUI.backgroundColor;
            GUI.backgroundColor = locked ? new Color(0.9f, 0.4f, 0.4f) : new Color(0.4f, 0.4f, 0.6f);

            if (GUI.Button(new Rect(position) {width = position.width * 0.1f - 6,}, icon))
            {
                locked = !locked;
            }
            
            EditorGUI.EndProperty();

            GUI.backgroundColor = oldColor;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return base.GetPropertyHeight(property, label);
        }
    }
}
