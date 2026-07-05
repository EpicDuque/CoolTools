using CoolTools.Utilities;
using UnityEditor;
using UnityEngine;

namespace CoolTools.Utilities.Editor
{
    [CustomPropertyDrawer(typeof(ColorSpacerAttribute))]
    public class ColorSpacerAttributeDrawer : DecoratorDrawer
    {
        private ColorSpacerAttribute colorSpacer => (ColorSpacerAttribute)attribute;

        public override float GetHeight()
        {
            var baseHeight = base.GetHeight() + colorSpacer.spaceHeight;

            return string.IsNullOrEmpty(colorSpacer.title) ? baseHeight : baseHeight + 12f;
        }

        public override void OnGUI(Rect position)
        {
            var width = position.width - 5f;

            // calculate the rect values for where to draw the line in the inspector
            var lineX = (position.x + (position.width / 2)) - width / 2;
            var lineY = position.y + (colorSpacer.spaceHeight);
            var lineHeight = colorSpacer.lineHeight;

            // Draw the line in the calculated place in the inspector
            // (using the built in white pixel texture, tinted with GUI.color)
            // EditorGUI.DrawPreviewTexture(new Rect(lineX, lineY, width, lineHeight), Texture2D.whiteTexture);
            var rect = new Rect(lineX, lineY, width, lineHeight);
            EditorGUI.DrawRect(rect, colorSpacer.lineColor);

            rect.y += 5f;
            rect.height = 15f;

            var oldGuiColor = GUI.color;
            GUI.color = colorSpacer.lineColor + new Color(0.25f, 0.25f, 0.25f);

            var labelStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 14
            };
            
            EditorGUI.LabelField(rect, colorSpacer.title, labelStyle);

            GUI.color = oldGuiColor;
        }
    }
}