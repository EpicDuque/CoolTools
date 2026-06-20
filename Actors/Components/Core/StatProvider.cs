using System;
using System.Collections.Generic;
using System.Linq;
using CoolTools.Attributes;
using CoolTools.Utilities;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace CoolTools.Actors
{
    public class StatProvider : MonoBehaviour
    {
        #region Structs
        
        [Serializable]
        public struct AttributeModification
        {
            public AttributeSO Attribute;
            public float Multiplier;
            public int Offset;

            public AttributeModification(AttributeSO attribute)
            {
                Attribute = attribute;
                Offset = 0;
                Multiplier = 1f;
            }
        }

        [Serializable]
        public struct StatProviderEvents
        {
            public UnityEvent OnLevelUp;
            // public UnityEvent StatsUpdated;
        }
        #endregion
        
        [Serializable]
        public enum ProviderInitMode
        {
            Awake, Start, Script
        }
        
        [FormerlySerializedAs("statSheet")] 
        [SerializeField] private StatSheet _statSheet;

        [Header("Level & EXP")]
        [SerializeField, Min(-1)] private int initialLevel = 1;
        [SerializeField, Min(1)] private int maxLevel = 99;
        [SerializeField, Min(-1)] private int level = 1;

        [Space(10f)] 
        [SerializeField] private bool _useExperience;
        [SerializeField] protected int _experience;
        [SerializeField] protected ValueCurve expLevelCurve;
        [SerializeField, InspectorDisabled] protected int _expNextLevel;
        
        [Header("Runtime Stats")] 
        [SerializeField] private bool _skipStatUpgradeEvent;
        [SerializeField] private List<StatSheet.AttributeValue> _currentStats;
        
        [Space(10f)] 
        [SerializeField] private List<AttributeModification> attributeModifications;
        
        [Space(10f)] 
        [SerializeField] private StatProviderEvents _events;
        
        protected int _expBaseLevel;
        protected int _previousExp;
        protected int _previousLevel;
        
        private StatSheet.AttributeValue[] _cachedBaseStats;
        private StatSheet _previousStatSheet;

        public StatSheet.AttributeValue[] CurrentStats => _currentStats.ToArray();

        public AttributeModification[] AttributeModifications => attributeModifications.ToArray();

        public StatProviderEvents Events => _events;

        public bool SkipStatUpgradeEvent
        {
            get => _skipStatUpgradeEvent;
            set => _skipStatUpgradeEvent = value;
        }
        
        public event Action StatsUpdated;
        
        /// <summary>
        /// Data asset used by the Actor.
        /// </summary>
        public StatSheet StatSheet
        {
            get => _statSheet;
            set => _statSheet = value;
        }

        public int InitialLevel
        {
            get => initialLevel;
            set => initialLevel = value;
        }

        public int Level
        {
            get => level;
            set
            {
                if (value > maxLevel)
                {
                    level = maxLevel;
                }
                else
                {
                    level = value;
                }
                
                if(level > _previousLevel)
                    Events.OnLevelUp?.Invoke();

                _previousLevel = level;
            }
        }

        public int MaxLevel => maxLevel;


        public int Experience
        {
            get => _experience;
            set
            {
                if (level >= maxLevel) return;
                
                _experience = value;
                _previousExp = value;
                UpdateExperience();
            }
        }

        public int ExpNextLevel
        {
            get => _expNextLevel;
            protected set => _expNextLevel = value;
        }

        public int ExpBaseLevel => _expBaseLevel;

        protected void OnValidate()
        {
            if (Application.isPlaying) return;
            if (StatSheet == null) return;

            UpdateActorCurrentStats();
        }
        
        public void Initialize()
        {
            _previousLevel = Level;
            
            Level = initialLevel;
            
            _experience = 0;

            if (_useExperience)
            {
                if(expLevelCurve != null)
                    _expNextLevel = Mathf.RoundToInt(expLevelCurve.Evaluate(Level + 1));

                UpdateExperience();
            }
            
            UpdateActorCurrentStats();
        }
        
        protected void UpdateExperience()
        {
            while (Experience >= _expNextLevel)
            {
                Level++;
                
                _expBaseLevel = _experience;
                _expNextLevel += Mathf.RoundToInt(expLevelCurve.Evaluate(Level + 1));
            }
            
            UpdateActorCurrentStats();
        }

        [ContextMenu("UpdateStats")]
        public void UpdateActorCurrentStats()
        {
            if (SkipStatUpgradeEvent) return;
            if (StatSheet == null) return;

            if (_previousStatSheet == null || StatSheet != _previousStatSheet)
            {
                _previousStatSheet = StatSheet;
                _cachedBaseStats = StatSheet.BaseStats;
            }

            var statsLen = _cachedBaseStats.Length;
            
            _currentStats.Clear();

            for (int i = 0; i < statsLen; i++)
            {
                var attribute = StatSheet.BaseStats[i].Attribute;
                _currentStats.Add(new StatSheet.AttributeValue
                {
                    Attribute = attribute,
                    Value = GetStatValue(attribute),
                });
            }

            if (!Application.isPlaying) return;
            
            StatsUpdated?.Invoke();
        }

        public int GetStatValue(AttributeSO attribute, AttributeModification[] modifications)
        {
            if (StatSheet == null) return 0;
            
            var curveValue = StatSheet.GetLevelCurveValue(attribute, Level);
            
            var modification = GetAttributeModification(attribute, modifications);
            
            var value = (modification.Multiplier * curveValue) + modification.Offset;

            return Mathf.RoundToInt(value);
        }
        
        public int GetCurrentStatValue(AttributeSO attribute)
        {
            var stat = new StatSheet.AttributeValue();
            foreach (var stat1 in CurrentStats)
            {
                if (stat1.Attribute == attribute)
                {
                    stat = stat1;
                    break;
                }
            }

            if (stat.Attribute == null)
                return -1;
            
            return stat.Value;
        }
        
        public int GetStatValue(AttributeSO attribute)
        {
            return GetStatValue(attribute, AttributeModifications);
        }
        
        public AttributeModification GetAttributeModification(AttributeSO attr)
        {
            return GetAttributeModification(attr, AttributeModifications);
        }
        
        public static AttributeModification GetAttributeModification(AttributeSO attr, AttributeModification[] modifications)
        {
            foreach (var st in modifications)
            {
                if (st.Attribute == attr) return st;
            }

            return new AttributeModification(attr);
        }

        public void SetAttributeModification(AttributeModification modification, bool updateStats = true)
        {
            var index = -1;
            for (int i = 0; i < attributeModifications.Count; i++)
            {
                if (attributeModifications[i].Attribute == modification.Attribute)
                {
                    index = i;
                    break;
                }
            }
        
            if (index == -1)
            {
                attributeModifications.Add(modification);
            }
            else
            {
                attributeModifications[index] = modification;
            }
            
            if(updateStats)
                UpdateActorCurrentStats();
        }

        public void ResetModifications()
        {
            attributeModifications.Clear();
        }

        private void OnDestroy()
        {
            StatsUpdated = null;
        }
    }
}