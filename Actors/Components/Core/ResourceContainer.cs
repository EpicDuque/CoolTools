using CoolTools.Utilities;
using UnityEngine;

namespace CoolTools.Actors
{
    public class ResourceContainer : OwnableBehaviour
    {
        [ColorSpacer("Resource Container")]
        [SerializeField] private ActorResource _resource;
        
        [Space(5f)]
        [SerializeField] private IntValueConfig _maxAmount;
        
        [Space(5f)] 
        [SerializeField] private FloatValueConfig _regenRate;

        [Space(10f)]
        [SerializeField] protected int _amount;
        [SerializeField] private bool _initializeAmount;

        protected float _regenCooldown;

        public float RegenCooldown
        {
            get => _regenCooldown;
            set => _regenCooldown = value;
        }

        public ActorResource Resource
        {
            get => _resource;
            set => _resource = value;
        }

        public IntValueConfig MaxAmount => _maxAmount;

        public virtual int Amount
        {
            get => _amount;
            set => _amount = Mathf.Clamp(value, 0, MaxAmount.Value);
        }
        
        public float Percent => (float) Amount / MaxAmount.Value;

        public float RegenRate
        {
            get => _regenRate.Value;
            set => _regenRate.BaseValue = value;
        }
        
        private void OnValidate()
        {
            _maxAmount?.UpdateValue(this);
            _regenRate?.UpdateValue(this);
            
            if(_amount > MaxAmount.Value)
                _amount = MaxAmount.Value;
        }
        
        private void OnEnable()
        {
            Amount = _amount;
        }

        private void Start()
        {
            if(_initializeAmount)
                Restore();
        }

        private void Update()
        {
            UpdateRegen();
        }

        protected virtual void UpdateRegen()
        {
            if (RegenRate <= 0f) return;
            
            _regenCooldown -= Time.deltaTime;
                
            if (_regenCooldown <= 0f)
            {
                _regenCooldown = 0f;
                do
                {
                    _regenCooldown += 1f / RegenRate;
                    Amount++;
                } while (_regenCooldown < Time.deltaTime);
            }
        }

        protected override void OnStatsUpdated()
        {
            _regenRate.UpdateValue(Owner);
            
            var oldPercent = Percent;
            _maxAmount.UpdateValue(Owner);
            
            if (oldPercent > 0f)
                Amount = Mathf.CeilToInt(MaxAmount.Value * oldPercent);
        }

        public virtual void Restore()
        {
            Amount = MaxAmount.Value;
        }
    }
}