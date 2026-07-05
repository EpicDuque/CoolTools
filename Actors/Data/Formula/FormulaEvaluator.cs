using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using CoolTools.Utilities;
using UnityEngine;

namespace CoolTools.Actors
{
    [CreateAssetMenu(fileName = "New Formula Evaluator", menuName = "Actor/Evaluators/Generic", order = 0)]
    public class FormulaEvaluator : ScriptableObject
    {
        protected List<Formula.ParameterInput> _inputParameters = new();

        [SerializeField, InspectorDisabled]
        protected List<string> _bakedParameterNames;
        
        public virtual float Evaluate(EvaluateParams ep)
        {
            return ep.Formula != null ? 
                (float) ep.Formula.Evaluate(ep.Parameters) : ep.FallBackValue;
        }

        public void ClearBakedParameterNames()
        {
            _bakedParameterNames.Clear();
        }
        
        protected void InsertBakedParameterNames(IEnumerable<string> names)
        {
            _bakedParameterNames.AddRange(names.Select(n => $"a.{n}"));
        }
        
        public virtual void BakeParameters()
        {
        }
        
        protected string GetBakedName(string varName)
        {
            var paramName = string.Empty;

            foreach (var bakedName in _bakedParameterNames)
            {
                if (!bakedName.Contains(varName)) continue;
                    
                paramName = bakedName;
                break;
            }

            return paramName;
        }
    }
    
    [StructLayout(LayoutKind.Auto)]
    public partial struct EvaluateParams
    {
        public Formula Formula;
        public IEnumerable<Formula.ParameterInput> Parameters;
        public float FallBackValue;
    }
}