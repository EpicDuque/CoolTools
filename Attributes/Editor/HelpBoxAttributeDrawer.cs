using UnityEditor;
using UnityEngine;

namespace CoolTools.Attributes.Editor
{
    [CustomPropertyDrawer(typeof(HelpBoxAttribute))]
    public class HelpBoxAttributeDrawer : DecoratorDrawer
    {
        public override void OnGUI(Rect position)
        {
            var helpBox = attribute as HelpBoxAttribute;
            
            var helpBoxHeight = GetHelpBoxHeight();

            // EditorGUI.LabelField(new Rect(position) { height = helpBoxHeight }, helpBox.Text, style);
            EditorGUI.HelpBox(new Rect(position) { height = helpBoxHeight }, helpBox.Text, MessageType.Info);
        }

        private float GetHelpBoxHeight()
        {
            var helpBox = attribute as HelpBoxAttribute;
            
            // Determine height of the Text
            return EditorGUIUtility.singleLineHeight * 2.1f + 5;
        }

        public override float GetHeight()
        {
            var helpBox = attribute as HelpBoxAttribute;
            
            return GetHelpBoxHeight() + 5;
        }
    }
}