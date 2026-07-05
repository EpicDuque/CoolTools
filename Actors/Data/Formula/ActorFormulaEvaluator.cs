using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CoolTools.Utilities;
using CoolTools.Data;
#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;
using ZLinq;

namespace CoolTools.Actors
{
    #pragma warning disable 414
    [CreateAssetMenu(fileName = "New Actor Formula Evaluator", menuName = "Actor/Evaluators/Actor", order = 0)]
    public class ActorFormulaEvaluator : FormulaEvaluator
    {
        [SerializeField] private bool _includeActorPosition = true;
        [SerializeField, InspectorDisabled] private string _positionParamNames = "a.x, a.y, a.z";
        
        [Space(5f)]
        [SerializeField] private bool _includeActorDamageable = true;
        [SerializeField, InspectorDisabled] private string _damageableParamNames = "a.hp, a.maxhp, a.pcthp";
        
        [Space(5f)]
        [SerializeField] private bool _includeActorLevel = true;
        [SerializeField, InspectorDisabled] private string _statProviderParamNames = "a.lvl";
        
        [Space(10f)]
        [SerializeField] private List<CustomParameter> _customParameters;

        private StringBuilder _stringBuilder = new ();

        public List<CustomParameter> CustomParameters => _customParameters;

        [Serializable]
        public struct CustomParameter
        {
            public string Name;
            public float Value;
        }

        public override float Evaluate(EvaluateParams ep)
        {
            if (ep.Formula == null)
            {
                return ep.FallBackValue;
            }

            _inputParameters.Clear();
            AddInputParameters(ep.Formula, ep.Source);
            
            if(ep.Target != null)
                AddInputParameters(ep.Formula, ep.Target);
        
            ep.Parameters = _inputParameters;
            return base.Evaluate(ep);
        }

        public virtual void AddInputParameters(Formula formula, Actor actor)
        {
            if (actor == null) return;
            
            formula.RefreshParameters();

            if (actor.StatProvider != null)
            {
                // Refactor above line into a stringbuilder
                foreach (var av in actor.StatProvider.CurrentStats)
                {
                    if(av.Attribute == null) continue;
                    
                    // If formula doesn't contain this input parmeter
                    bool all = true;
                    
                    string compare = null;

                    foreach (var term in _bakedParameterNames)
                    {
                        // if(term.Contains("a."))
                        //     if(term.Split("a.")[1] != av.Attribute.VariableName) continue;
                        // else if (term != av.Attribute.VariableName) continue;
                        if (!term.Contains(av.Attribute.VariableName) || 
                            av.Attribute.VariableName.Length != term.Length - 2) continue;
                        
                        compare = term;
                        break;
                    }

                    if (compare == null)
                    {
                        Debug.LogWarning($"[{nameof(ActorFormulaEvaluator)}] {name} does not contain the parameter {av.Attribute.VariableName}.");
                        _stringBuilder.Clear();
                        _stringBuilder.Append("a");
                        _stringBuilder.Append(".");
                        _stringBuilder.Append(av.Attribute.VariableName);
                        compare = _stringBuilder.ToString();
                        _bakedParameterNames.Add(compare);
                    }

                    foreach (var ip in formula.InputParameters)
                    {
                        if (ip.Name != compare) continue;

                        all = false;
                        break;
                    }

                    if (all) continue;

                    var param = new Formula.ParameterInput 
                    { 
                        Name = compare, 
                        Value = av.Value
                    };
                    _inputParameters.Add(param);
                }
            }

            if (_includeActorPosition)
            {
                var position = actor.transform.position;
                _inputParameters.Add(new Formula.ParameterInput
                {
                    Name = "a.x",
                    Value = position.x
                });
                _inputParameters.Add(new Formula.ParameterInput
                {
                    Name = "a.y",
                    Value = position.y
                });
                _inputParameters.Add(new Formula.ParameterInput//
                {
                    Name = "a.z",
                    Value = position.z
                });
            }

            if (_includeActorLevel)
            {
                _inputParameters.Add(new Formula.ParameterInput
                {
                    Name = "a.lvl",
                    Value = actor.StatProvider.Level
                });
            }

            if (_includeActorDamageable && actor.HasDamageable)
            {
                _inputParameters.Add(new Formula.ParameterInput
                {
                    Name = "a.hp",
                    Value = actor.DamageableResource.Amount
                });
                _inputParameters.Add(new Formula.ParameterInput
                {
                    Name = "a.maxhp",
                    Value = actor.DamageableResource.MaxAmount.Value
                });
                _inputParameters.Add(new Formula.ParameterInput
                {
                    Name = "a.pcthp",
                    Value = actor.DamageableResource.Percent
                });
            }

            var values = _customParameters
#if ACTOR_ZLINQ
                .AsValueEnumerable()
#endif
                .Select(p => new Formula.ParameterInput
                {
                    Name = p.Name,
                    Value = p.Value,
                })
                .ToArray();
            _inputParameters.AddRange(values);
        }

#if UNITY_EDITOR
        public override void BakeParameters()
        {
            var attributeNames = new List<string>();
            
            // Search for all AttributeSO scripts in the AssetDatabase and add their variableName into attributeNames
            var guids = AssetDatabase.FindAssets("t:AttributeSO");
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var attribute = AssetDatabase.LoadAssetAtPath<AttributeSO>(path);
                attributeNames.Add(attribute.VariableName);
                
                EditorUtility.SetDirty(attribute);
            }
            
            InsertBakedParameterNames(attributeNames);
        }
#endif
    }
    
    public partial struct EvaluateParams
    {
        public Actor Source;
        public Actor Target;
    }
}