using System;
using UnityEngine;

namespace CoolTools.Actors
{
    public interface IDamageable
    {
        public GameObject GO { get; }
        public bool IsAlive { get; }
        public bool Invincible { get; set; }
        void DealDamage(DamageParams data);
        public int Health { get; }
        public int MaxHealth { get; }
    }
    
    [Serializable]
    public struct DamageParams
    {
        public int Amount;
        public Actor Source;
        public IDamageable Target;
        public GameObject SourceObject;
        public DamageType Type;
        public bool Critical;
        public Vector3 HitPoint;
    }
}