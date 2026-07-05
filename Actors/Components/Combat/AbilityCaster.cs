using System;
using System.Collections;
using System.Linq;
using CoolTools.Utilities;
using UnityEngine;
using UnityEngine.Events;
using ZLinq;

namespace CoolTools.Actors
{
    public class AbilityCaster : OwnableBehaviour
    {
        [Serializable]
        public struct Events
        {
            public UnityEvent<AbilityBase> OnActorCastAbility;
            public UnityEvent<AbilityBase> OnActorEndCast;
            public UnityEvent<AbilityBase> OnActorCastInterrupt;
        }
        
        [ColorSpacer("Ability Caster")]
        [SerializeField] protected AbilityBase _castingAbility;
        [SerializeField] protected FloatValueConfig _durationMultiplier;
        [SerializeField] protected FloatValueConfig _cooldownMultiplier;
        
        [Space(10f)] 
        [SerializeField] protected ActorAnimationEventListener _eventListener;
        
        [Space(10f)]
        [Tooltip("Resource containers to use for Ability resource costs")]
        [SerializeField] protected ResourceContainer[] _resourceContainers;

        [Space(10f)]
        [SerializeField] protected Events _actorCombatEvents;
        
        [Header("Debug")]
        [SerializeField, InspectorDisabled] protected CasterState _state;
        [SerializeField, InspectorDisabled] private CastingTarget _castingTarget;
        
        private bool _abilityEndSignal;
        
        private Vector3 _targetPosition;
        private Detectable _targetDetectable;
        private Actor _targetActor;
        
        private enum CastingTarget
        {
            None,
            Position,
            IDetectable,
            Actor,
        }
        
        [field: SerializeField, InspectorDisabled]
        public float Cooldown { get; protected set; }

        public CasterState State
        {
            get => _state;
            protected set => _state = value;
        }

        public AbilityBase CastingAbility
        {
            get => _castingAbility;
            set => _castingAbility = value;
        }
        
        public Events ActorCombatEvents => _actorCombatEvents;
        
        public bool IsReady => Cooldown <= 0f && State != CasterState.Casting;

        public ActorAnimationEventListener EventListener
        {
            get => _eventListener;
            set
            {
                if(_eventListener != null)
                    _eventListener.OnEvent.RemoveListener(OnAbilityEvent);
                
                _eventListener = value;
                
                if(_eventListener != null)
                    _eventListener.OnEvent.AddListener(OnAbilityEvent);
            }
        }
        
        [Serializable]
        public enum CasterState
        {
            Ready,
            Casting,
            Cooldown,
        }

        private Coroutine _castingRoutine;

        protected new void Reset()
        {
            base.Reset();

            _cooldownMultiplier.Multiplier = 1;
            _durationMultiplier.Multiplier = 1;

            _cooldownMultiplier.BaseValue = 1;
            _durationMultiplier.BaseValue = 1;
            
            _cooldownMultiplier.UpdateValue(this);
            _durationMultiplier.UpdateValue(this);
        }

        private void OnValidate()
        {
            _cooldownMultiplier.UpdateValue(this);
            _durationMultiplier.UpdateValue(this);
        }

        private void OnEnable()
        {
            if(_eventListener != null)
                _eventListener.OnEvent.AddListener(OnAbilityEvent);
        }

        private void OnDisable()
        {
            if(_eventListener != null)
                _eventListener.OnEvent.RemoveListener(OnAbilityEvent);
        }

        protected new void Awake()
        {
            base.Awake();
            
            ResetCaster();
        }

        protected override void OnStatsUpdated()
        {
            base.OnStatsUpdated();
            
            var eparams = new EvaluateParams
            {
                Source = Owner,
            };
            
            _cooldownMultiplier.UpdateValue(Owner.Evaluator, eparams);
            _durationMultiplier.UpdateValue(Owner.Evaluator, eparams);
        }

        private void OnAbilityEvent(AnimationEventSO animEvent)
        {
            if (State != CasterState.Casting) return;

            if (!animEvent.EndsAbility)
            {
                var ev = _castingAbility.EventEffects
                    .FirstOrDefault(ef => ef.Event == animEvent);

                if (ev.Event == null) return;
                
                foreach (var effect in ev.Effects)
                {
                    switch (_castingTarget)
                    {
                        case CastingTarget.None:
                            effect.Execute(Owner);
                            break;
                        case CastingTarget.Position:
                            effect.Execute(Owner, _targetPosition);
                            break;
                        case CastingTarget.IDetectable:
                            effect.Execute(Owner, _targetDetectable);
                            break;
                        case CastingTarget.Actor:
                            effect.Execute(Owner, _targetActor);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }
            else
            {
                _abilityEndSignal = true;
            }
        }
        
        #region CastAbility
        
        private IEnumerator CastingRoutine(IEnumerator effectExecuteRoutine)
        {
            State = CasterState.Casting;

            StartCoroutine(effectExecuteRoutine);

            if (_castingAbility.AbilityDuration > 0f)
            {
                var duration = _castingAbility.AbilityDuration * _durationMultiplier.Value;
                // yield return new WaitForSeconds(duration);
                
                var timer = 0f;
                while (timer < duration)
                {
                    timer += Time.deltaTime;
                    yield return null;
                }
            }
            else
            {
                _abilityEndSignal = false;
                while (!_abilityEndSignal)
                    yield return null;
            }
            
            EndAbility();

            State = CasterState.Cooldown;
            
            // Apply cooldown multipliers here
            // yield return new WaitUntil(() => Cooldown <= 0f);
            while(Cooldown > 0f)
                yield return null;
            
            Cooldown = 0f;
            State = CasterState.Ready;
            
            _castingRoutine = null;
            _abilityEndSignal = false;
        }

        private void ExecuteEffects()
        {
            foreach (var effect in _castingAbility.AbilityEffects)
            {
                effect.Execute(Owner);
            }
        }
        
        private void ExecuteEffects(Vector3 position)
        {
            foreach (var effect in _castingAbility.AbilityEffects)
            {
                effect.Execute(Owner, position);
            }
        }
        
        private void ExecuteEffects(Detectable target)
        {
            foreach (var effect in _castingAbility.AbilityEffects)
            {
                effect.Execute(Owner, target);
            }
        }
        
        private void ExecuteEffects(Actor target)
        {
            foreach (var effect in _castingAbility.AbilityEffects)
            {
                effect.Execute(Owner, target);
            }
        }
        
        private IEnumerator ExecuteEffectsRoutine()
        {
            foreach(var effect in _castingAbility.AbilityEffects)
            {
                effect.Execute(Owner);
                if (effect is IEffectWaiter waiter)
                {
                    yield return waiter.Wait;
                }
            }
        }
        
        private IEnumerator ExecuteEffectsRoutine(Vector3 position)
        {
            foreach(var effect in _castingAbility.AbilityEffects)
            {
                effect.Execute(Owner, position);
                if (effect is IEffectWaiter waiter)
                {
                    yield return waiter.Wait;
                }
            }
        }
        
        private IEnumerator ExecuteEffectsRoutine(Detectable target)
        {
            foreach(var effect in _castingAbility.AbilityEffects)
            {
                effect.Execute(Owner, target);
                if (effect is IEffectWaiter waiter)
                {
                    yield return waiter.Wait;
                }
            }
        }
        
        private IEnumerator ExecuteEffectsRoutine(Actor target)
        {
            foreach(var effect in _castingAbility.AbilityEffects)
            {
                effect.Execute(Owner, target);
                if (effect is IEffectWaiter waiter)
                {
                    yield return waiter.Wait;
                }
            }
        }

        public void CastAbility()
        {
            if (!IsReadyToCast()) return;
            
            State = CasterState.Casting;
            _castingTarget = CastingTarget.None;
            
            _castingRoutine = StartCoroutine(CastingRoutine(ExecuteEffectsRoutine()));
            ActorCombatEvents.OnActorCastAbility?.Invoke(_castingAbility);
            
            TriggerAnimatorFields();
        }

        public void CastAbility(Vector3 position)
        {
            if (!IsReadyToCast()) return;
            
            State = CasterState.Casting;
            _castingTarget = CastingTarget.Position;
            _targetPosition = position;
            
            _castingRoutine = StartCoroutine(CastingRoutine(ExecuteEffectsRoutine(position)));
            ActorCombatEvents.OnActorCastAbility?.Invoke(_castingAbility);
            
            TriggerAnimatorFields();
        }

        public void CastAbility(Detectable target)
        {
            if (!IsReadyToCast()) return;

            State = CasterState.Casting;
            _castingTarget = CastingTarget.IDetectable;
            _targetDetectable = target;
            
            // Cast the ability with target transform here
            
            _castingRoutine = StartCoroutine(CastingRoutine(ExecuteEffectsRoutine(target)));
            ActorCombatEvents.OnActorCastAbility?.Invoke(_castingAbility);
            
            TriggerAnimatorFields();
        }
        
        public void CastAbility(Actor target)
        {
            if (!IsReadyToCast()) return;

            State = CasterState.Casting;
            _castingTarget = CastingTarget.Actor;
            _targetActor = target;
            
            // Cast the ability with target transform here
            
            _castingRoutine = StartCoroutine(CastingRoutine(ExecuteEffectsRoutine(target)));
            ActorCombatEvents.OnActorCastAbility?.Invoke(_castingAbility);
            
            TriggerAnimatorFields();
        }
        
        private void TriggerAnimatorFields()
        {
            if(_castingAbility.SetTrigger && Owner.HasAnimator)
                Owner.Animator.SetTrigger(_castingAbility.TriggerName);
        }

        #endregion
        
        public void ResetCaster()
        {
            StopAllCoroutines();
            Cooldown = 0f;
            State = CasterState.Ready;
        }
        
        public bool IsReadyToCast()
        {
            if (Cooldown > 0f || State != CasterState.Ready) return false;
            
            if (_castingAbility == null) return false;
            
            if (!HasResourcesForAbility(_castingAbility))
            {
                Debug.Log($"[{nameof(AbilityCaster)}] tried to cast an Ability without the required resources");
                return false;
            }
            
            return true;
        }

        public bool HasResourcesForAbility(AbilityBase ability)
        {
            return ability.ResourceCosts.Length == 0 || ability.ResourceCosts.AsValueEnumerable().All(rc =>
                _resourceContainers.AsValueEnumerable().Any(c => c.Resource == rc.Resource && c.Amount >= rc.Amount));
        }

        private void EndAbility(float cooldown = -1f)
        {
            if (State == CasterState.Casting || _castingAbility != null)
            {
                ActorCombatEvents.OnActorEndCast?.Invoke(_castingAbility);
            }

            Cooldown = _castingAbility.Cooldown * _cooldownMultiplier.Value;
        }

        public void InterruptAbility()
        {
            if (State != CasterState.Casting) return;

            _abilityEndSignal = true;
        }

        private void Update()
        {
            if (State == CasterState.Cooldown)
            {
                if (Cooldown > 0f)
                    Cooldown -= Time.deltaTime;
                else
                {
                    Cooldown = 0f;
                    State = CasterState.Ready;
                }
            }
        }

        public void ForceCooldown(float cooldown = -1f)
        {
            if (State == CasterState.Casting)
            {
                StopAllCoroutines();
                
                EndAbility();

                State = CasterState.Cooldown;
                Cooldown = cooldown > -1f ? cooldown : _castingAbility.Cooldown;
                _castingRoutine = null;
            }
            else
            {
                State = CasterState.Cooldown;
                Cooldown = cooldown > -1f ? cooldown : _castingAbility.Cooldown;
            }
        }
    }
}
