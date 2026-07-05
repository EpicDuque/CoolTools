using UnityEngine;

namespace CoolTools.Utilities
{
    public class ColorSpacerAttribute : PropertyAttribute
    {
        public float spaceHeight;
        public float lineHeight;
        public Color lineColor;
        public string title;

        public ColorSpacerAttribute(string title, float r, float g, float b, float spaceHeight = 15f,
            float lineHeight = 2f)
        {
            this.spaceHeight = spaceHeight;
            this.lineHeight = lineHeight;
        
            lineColor = new Color(r, g, b);

            this.title = title;
        }

        public ColorSpacerAttribute(string title = "", float r = 0.8f, float g = 0.8f, float b = 0.8f)
        {
            spaceHeight = 15f;
            lineHeight = 2;

            lineColor = new Color(r, g, b);

            this.title = title;
        }
    }
}