using System;
using System.Collections.Generic;
using System.Linq;
using B83.ExpressionParser;
using CoolTools.Utilities;
using UnityEngine;

namespace CoolTools.Actors
{
    [CreateAssetMenu(fileName = "New Formula", menuName = "Formula", order = 0)]
    public class Formula : ScriptableObject
    {
        [Serializable]
        public struct ParameterInput
        {
            [Lockable] public string Name;
            public double Value;
        }
        
        [SerializeField, TextArea] private string rawExpression;
        
        [Space(10f)]
        [SerializeField, TextArea, InspectorDisabled] private string parsedExpression;
        [SerializeField] private List<ParameterInput> inputParameters;
        [SerializeField] private List<float> _constants;
        
        [Space(10f)]
        [SerializeField, InspectorDisabled] private double result;
        
        private Expression cachedExpression;
        private ExpressionDelegate cachedDelegate;
        private ExpressionParser parser;

        public ParameterInput[] InputParameters => inputParameters.ToArray();

        private List<ParameterInput> _tempParams = new();
//
        public string RawExpression
        {
            get => rawExpression;
            set => rawExpression = value;
        }

        public string ParsedExpression
        {
            get => parsedExpression;
            protected set => parsedExpression = value;
        }
        
        public double Result => result;

        private void OnEnable()
        {
            parser ??= new ExpressionParser();
            Parse();
        }

        public void Parse()
        {
            if (string.IsNullOrEmpty(rawExpression)) return;
            
            parser ??= new ExpressionParser();

            cachedExpression = parser.GetExpressionFromString(rawExpression);
            
            ParsedExpression = cachedExpression.ToString();
            
            RefreshParameters();
            
            for (int i = 0; i < MaxParamLength; i++)
            {
                _paramOrder[i] = string.Empty;
            }
            
            for (var i = 0; i < inputParameters.Count; i++)
            {
                var ip = inputParameters[i];
                _paramOrder[i] = ip.Name;
            }
            
            cachedDelegate = cachedExpression.ToDelegate(_paramOrder);
        }

        public void RefreshParameters()
        {
            inputParameters.Clear();

            if (cachedExpression == null) return;

            inputParameters.Clear();
            foreach (var param in cachedExpression.Parameters)
            {
                inputParameters.Add(new ParameterInput
                {
                    Name = param.Key,
                    Value = param.Value.Value
                });
            }
        }

        private void SetParameters(IEnumerable<ParameterInput> parameters)
        {
            inputParameters.Clear();
            
            inputParameters.AddRange(parameters);
        }

        private const int MaxParamLength = 32;
        private string[] _paramOrder = new string[MaxParamLength];
        private double[] _paramValues = new double[MaxParamLength];
        
        /// <summary>
        /// Evaluate parsed expression with existing parameters.
        /// </summary>
        public double Evaluate()
        {
            if(cachedExpression == null)
                Parse();

            if (cachedExpression == null) return 0f;
            
            for (int i = 0; i < MaxParamLength; i++)
            {
                _paramValues[i] = 0;
                _paramOrder[i] = string.Empty;
            }
            
            for (var i = 0; i < inputParameters.Count; i++)
            {
                var ip = inputParameters[i];
                
                _paramOrder[i] = ip.Name;
                _paramValues[i] = ip.Value;
            }
            
            cachedExpression.RefreshParamOrder(_paramOrder);

            result = cachedDelegate.Invoke(_paramValues);
            
            return result;
        }

        private readonly string[] _cachedConstants = new[] { "c0", "c1", "c2", "c3", "c4", "c5", "c6", "c7", "c8", "c9" };

        public double Evaluate(IEnumerable<ParameterInput> parameters)
        {
            _tempParams.Clear();
            _tempParams.AddRange(parameters);
            
            // Add All constants to tempParams
            for (int i = 0; i < _constants.Count; i++)
            {
                _tempParams.Add(new ParameterInput
                {
                    Name = _cachedConstants[i],
                    Value = _constants[i],
                });
            }
            
            SetParameters(_tempParams);

            return Evaluate();
        }
    }
}