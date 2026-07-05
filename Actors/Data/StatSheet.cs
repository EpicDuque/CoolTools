using System;
using System.Collections.Generic;
using System.Linq;
using CoolTools.Utilities;
using UnityEngine;
using UnityEngine.Serialization;

namespace CoolTools.Actors
{
    [CreateAssetMenu(menuName = "Actor/Stat Sheet", fileName = "New Actor Stat Sheet")]
    public class StatSheet : ScriptableObject
    {
        [Serializable]
        public struct AttributeValue
        {
            public AttributeSO Attribute;
            public int Value;

            public bool Equals(AttributeValue other)
            {
                return Attribute == other.Attribute && Value == other.Value;
            }
        }
        
        [Serializable]
        public struct StatsLevelCurve
        {
            public AttributeSO Attribute;
            public ValueCurve Curve;
        }
        
        [SerializeField] private List<AttributeValue> _baseStats = new();
        
        [Space(10f)] 
        [SerializeField] private bool _curveDeterminesBaseStats;
        [SerializeField] private StatsLevelCurve[] _attributeLevelCurves;
        
        public AttributeValue[] BaseStats => _baseStats.ToArray();

        public StatsLevelCurve[] AttributeLevelCurves => _attributeLevelCurves;

        private void OnValidate()
        {
            if (_curveDeterminesBaseStats)
            {
                foreach(var curve in _attributeLevelCurves)
                {
                    if (curve.Attribute == null) continue;

                    for (int i = 0; i < _baseStats.Count; i++)
                    {
                        var stat = _baseStats[i];
                        if (stat.Attribute != curve.Attribute) continue;
                        
                        _baseStats[i] = new AttributeValue
                        {
                            Attribute = stat.Attribute,
                            Value = Mathf.RoundToInt(curve.Curve.Evaluate(1))
                        };
                        
                        break;
                    }
                }
            }
        }

        public int GetBaseAttributeValue(AttributeSO definition)
        {
            if (_baseStats == null || _baseStats.Count == 0) return -1;
            
            var stat = _baseStats.FirstOrDefault(s => s.Attribute == definition);
            
            if (stat.Attribute == null) return -1;
            
            return stat.Value;
        }

        public float GetLevelCurveValue(AttributeSO attribute, int level)
        {
            var baseStat = GetBaseAttributeValue(attribute);
            if (AttributeLevelCurves == null || AttributeLevelCurves.Length == 0)
            {
                return baseStat;
            }
            
            var levelCurve = new StatsLevelCurve();
            
            foreach (var lv in AttributeLevelCurves)
            {
                if (lv.Attribute == attribute)
                {
                    levelCurve = lv;
                    break;
                }
            }

            return levelCurve.Attribute != null ? levelCurve.Curve.Evaluate(level) : baseStat;
        }
    }
}