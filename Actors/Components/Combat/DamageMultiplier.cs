using System;
using UnityEngine;

namespace CoolTools.Actors
{
    [Serializable]
    public struct DamageMultiplier : IEquatable<DamageMultiplier>
    {
        [HideInInspector]
        public string ID;
        
        [Space(10f)]
        public DamageType DamageType;
        public bool AllTypes;
            
        [Space(5f)]
        public Formula FormulaMultiplier;
            
        [Space(5f)]
        public float Multiplier;

        public bool Equals(DamageMultiplier other)
        {
            return ID == other.ID;
        }

        public override bool Equals(object obj)
        {
            return obj is DamageMultiplier other && Equals(other);
        }

        public override int GetHashCode()
        {
            return (ID != null ? ID.GetHashCode() : 0);
        }
    }
}