using UnityEngine;

namespace CoolTools.Utilities
{
    [System.AttributeUsage(System.AttributeTargets.Field)]
    public class ProgressBarAttribute : PropertyAttribute {
        
        public string maxPropertyName;
        
        public ProgressBarAttribute(string maxPropertyName)
        {
            this.maxPropertyName = maxPropertyName;
        }
    }
}