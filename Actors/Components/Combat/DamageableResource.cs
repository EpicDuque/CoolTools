using System;
using System.Collections.Generic;
using CoolTools.Utilities;
using UnityEngine;
using UnityEngine.Events;

namespace CoolTools.Actors
{
    public class DamageableResource : ResourceContainer, IDamageable
    {
        [ColorSpacer("Damageable Resource")] 
        [SerializeField] protected bool _invincible;
        
        [Space(10f)] 
        [SerializeField] private List<DamageMultiplier> _multipliers;
        
        [ColorSpacer("Events")]
        [SerializeField] protected DamageableEvents _damageableEvents;
        
        [Serializable]
        public struct DamageableEvents
        {
            public UnityEvent<int> OnDamage;
            public UnityEvent<int> OnHeal;
            
            public UnityEvent OnDeath;
            public UnityEvent OnRevive;
        }
        
        public DamageableEvents Events => _damageableEvents;

        [field: SerializeField, InspectorDisabled] 
        public DamageParams LastDamage { get; set; }

        public bool Invincible
        {
            get => _invincible;
            set => _invincible = value;
        }

        public override int Amount
        {
            get => _amount;
            set
            {
                var previousAmount = Amount;
                base.Amount = value;
                
                if (previousAmount > 0 && Amount <= 0)
                    Events.OnDeath?.Invoke();
            }
        }
        
        public GameObject GO => gameObject;
        public virtual bool IsAlive => Amount > 0;
        public int Health => Amount;
        public int MaxHealth => MaxAmount.Value;

        public virtual void DealDamage(DamageParams data)
        {
            if (!IsAlive) return;
            if (Invincible) return;

            var modifiedData = data;
            
            var amount = Mathf.RoundToInt(data.Amount * GetMultiplier(data.Type));
            modifiedData.Amount = amount;
            
            LastDamage = modifiedData;
            Amount -= modifiedData.Amount;
            
            if(modifiedData.Amount < 0)
                Events.OnHeal?.Invoke(modifiedData.Amount);
            else if(modifiedData.Amount > 0)
                Events.OnDamage?.Invoke(modifiedData.Amount);
        }

        private float GetMultiplier(DamageType damageType)
        {
            float totalMultiplier = 1f;
            
            foreach (var multiplier in _multipliers)
            {
                if (multiplier.AllTypes || multiplier.DamageType == damageType)
                {
                    totalMultiplier *= multiplier.Multiplier;
                }
            }

            return totalMultiplier;
        }
        
        public void AddDamageMultiplier(DamageMultiplier multiplier)
        {
            _multipliers.Add(multiplier);
        }
        
        public void RemoveDamageMultiplier(DamageMultiplier multiplier)
        {
            _multipliers.Remove(multiplier);
        }
        
        public void RemoveDamageMultiplier(string id)
        {
            _multipliers.RemoveAll(multiplier => multiplier.ID == id);
        }
        
        [ContextMenu("Kill")]
        public virtual void Kill()
        {
            var data = new DamageParams
            {
                Amount = MaxAmount.Value,
                Critical = false,
                Source = null,
            };
            DealDamage(data);
        }

        public void Revive()
        {
            if (IsAlive) return;
            
            Restore();
            Events.OnRevive?.Invoke();
        }

        protected override void UpdateRegen()
        {
            if (RegenRate <= 0f) return;
            
            _regenCooldown -= Time.deltaTime;
                
            if (_regenCooldown <= 0f)
            {
                _regenCooldown = 0f;
                do
                {
                    _regenCooldown += 1f / RegenRate;
                    if (Health < MaxHealth)
                    {
                        DealDamage(new DamageParams
                        {
                            Amount = -1,
                            Source = Owner,
                            Target = this,
                        });
                    }
                } while (_regenCooldown < Time.deltaTime);
            }
        }
    }
}
