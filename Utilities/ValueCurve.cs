using CoolTools.Utilities;
using UnityEngine;
using UnityEngine.Serialization;

namespace CoolTools.Utilities
{
    [CreateAssetMenu(fileName = "New Value Curve", menuName = "Value Curve", order = 0)]
    public class ValueCurve : ScriptableObject
    {
        [FormerlySerializedAs("scaleX")]
        [Space(10f)]
        [SerializeField] private float _XMax = 1f;
        [FormerlySerializedAs("scaleY")] 
        [SerializeField] private float _YMax = 1f;
        [SerializeField] private float _YOffset;
        
        [Space(10f)]
        [SerializeField] private AnimationCurve curve;

        [Space(10f)] 
        [SerializeField] private float eval = 0.5f;
        [SerializeField] private bool roundToInt;
        
        [SerializeField, InspectorDisabled] private float result = 0f; 

        private void OnValidate()
        {
            result = Evaluate(eval);

            if (roundToInt)
                result = Mathf.RoundToInt(result);
        }

        public float Evaluate(float x) => _YOffset + curve.Evaluate(x / _XMax) * _YMax;
        
        public int EvaluateRounded(float x) => Mathf.RoundToInt(Evaluate(x));
    }
}