using System.Collections.Generic;
using B83.ExpressionParser;
using CoolTools.Attributes;
using UnityEngine;

namespace CoolTools.Actors
{
    [CreateAssetMenu(fileName = "New Attribute", menuName = "Actor/Stat Attribute")]
    public class AttributeSO : ScriptableObject
    {
        private static List<string> ReservedKeywords = new();
        
        [SerializeField] private string definedName = "Default";
        [SerializeField] private string variableName = "default";
        [SerializeField] private bool suggestVariableName;
        [SerializeField, SpritePreviewSmall] private Sprite _icon;
        
        [Space(10f)]
        [SerializeField] private bool _hidden;
        
        [Space(10f)] 
        [SerializeField, TextArea] private string _description;
        
        public string DefinedName => definedName;
        public string VariableName => variableName;
        public Sprite Icon => _icon;
        
        public bool Hidden => _hidden;
        public string Description => _description;

        // When Assemblies reload, execute below method
        // [InitializeOnLoadMethod]
        public static void GenerateReserved()
        {
            ReservedKeywords.Clear();
            ReservedKeywords.Add("a");
            ReservedKeywords.AddRange(ExpressionParser.FunctionNames);
        }
        
        private void OnValidate()
        {
            if (suggestVariableName)
                variableName = definedName.ToLower().Replace(" ", "");
        }

        private void OnEnable()
        {
            if (ReservedKeywords.Contains(variableName))
            {
                Debug.LogWarning($"[{nameof(AttributeSO)}] Variable name cannot be reserved keyword {variableName}");
            }
        }
    }
}