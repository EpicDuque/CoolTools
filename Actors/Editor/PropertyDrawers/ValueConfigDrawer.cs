using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace CoolTools.Actors.Editor
{
    public abstract class ValueConfigDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var visualTree = Resources.Load<VisualTreeAsset>("CoolTools/ValuePropertyDrawerVisualTree");
            var root = visualTree.CloneTree();

            var propNameLabel = root.Q<Label>("name");
            
            // Determine if property is of type FloatValueConfig or IntValueConfig
            var baseValueProperty = property.FindPropertyRelative("_baseValue");
            string propertyType = string.Empty;
            if (baseValueProperty.propertyType == SerializedPropertyType.Integer)
            {
                propertyType = "Int";
            } else if (baseValueProperty.propertyType == SerializedPropertyType.Float)
            {
                propertyType = "Float";
            }
            
            propNameLabel.text = $"{property.displayName} (<color=green>{propertyType}</color>)";
            
            var formulaToggle = root.Q<Toggle>("formulaToggle");
            var formulaField = root.Q<PropertyField>("fieldFormula");
            var baseValueField = root.Q<PropertyField>("fieldBaseValue");
            // var buttonEvaluate = root.Q<Button>("eval");
            
            baseValueField.SetEnabled(true);
            formulaField.SetEnabled(true);

            // buttonEvaluate.clicked += () => OnEvalClicked(property);
            formulaToggle.RegisterValueChangedCallback(c => OnValueChangeCallback(c, formulaField, baseValueField));
        
            return root;
        }

        private void OnValueChangeCallback(ChangeEvent<bool> evt, PropertyField formulaField, PropertyField baseValueField)
        {
            Debug.Log("Value changed " + evt.newValue);
            
            formulaField.SetEnabled(evt.newValue);
            baseValueField.SetEnabled(!evt.newValue);
        }
    }
}