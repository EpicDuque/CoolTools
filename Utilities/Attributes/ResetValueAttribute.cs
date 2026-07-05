using UnityEngine;

namespace CoolTools.Utilities
{
    public class ResetValueAttribute : PropertyAttribute
    {
        public float resetFloat;
        public int resetInt;
        public bool resetBool;
        public string resetString;
        public Vector2 resetVector2;
        public Vector3 resetVector3;

        public ResetValueAttribute(float val)
        {
            resetFloat = val;
        }

        public ResetValueAttribute(int val)
        {
            resetInt = val;
        }

        public ResetValueAttribute(bool val)
        {
            resetBool = val;
        }

        public ResetValueAttribute(string val)
        {
            resetString = val;
        }

        public ResetValueAttribute(float x, float y)
        {
            resetVector2 = new Vector2(x, y);
        }

        public ResetValueAttribute(float x, float y, float z)
        {
            resetVector3 = new Vector3(x, y, z);
        }
    }
}