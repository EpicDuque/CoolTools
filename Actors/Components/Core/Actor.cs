using System;
using System.Collections.Generic;
#if ACTOR_ZLINQ
using ZLinq;
#endif
using System.Linq;
using CoolTools.Utilities;
using UnityEngine;
using UnityEngine.Events;

namespace CoolTools.Actors
{
    /// <summary>
    /// Main component of Actor Engine. Actor contains basic functionality that the rest of the components of the Engine
    /// refer to.
    /// </summary>
    public class Actor : MonoBehaviour
    {
        [Serializable]
        public class ActorEvents
        { 
            public UnityEvent<Actor> OnActorInitialized;
            public UnityEvent<Actor> OnActorEnabled;
            public UnityEvent<Actor> OnActorDisabled;
            public UnityEvent<Actor> OnActorDestroyed;
        }

        public event Action<OwnableBehaviour> OnRegisterOwnable;
        public event Action<OwnableBehaviour> OnUnRegisterOwnable;

        #region Serialized Fields

        [Serializable]
        public enum ActorInitMode
        {
            Awake, Enable, Start, Script
        }
        
        [ColorSpacer("Actor")]
        [SerializeField] private ActorInitMode initMode = ActorInitMode.Start;
        [SerializeField] private bool _autoDetectComponents = false;

        [Header("References")] 
        [SerializeField] private GameObject _model;
        [SerializeField] protected StatProvider _statProvider;
        [SerializeField] protected DamageableResource _damageableResource;
        [SerializeField] protected Animator _animator;
        
        [Space(10f)]
        [SerializeField, InspectorDisabled] private List<EffectTarget> _effectTargets = new();
        
        [Header("Data")]
        [SerializeField] private string actorName = "Actor";
        [SerializeField] protected ActorFaction _faction;
        [SerializeField] protected ActorFormulaEvaluator _evaluator;

        [Header("Events")]
        [SerializeField] protected ActorEvents _actorEvents;
        
        #endregion

        #region Private Fields

        private List<OwnableBehaviour> ownedBehaviours = new();

        #endregion
        
        #region Public Properties

        public string ActorName
        {
            get => actorName;
            set => actorName = value;
        }

        public OwnableBehaviour[] OwnedBehaviours => ownedBehaviours.ToArray();

        public StatProvider StatProvider
        {
            get => _statProvider;
            set => _statProvider = value;
        }
        
        public bool HasDamageable { get; protected set; }
        public bool HasStatProvider { get; protected set; }
        public bool HasAnimator { get; protected set; }

        public DamageableResource DamageableResource
        {
            get => _damageableResource;
            set => _damageableResource = value;
        }

        public ActorFaction Faction
        {
            get => _faction;
            set => _faction = value;
        }

        public ActorFormulaEvaluator Evaluator => _evaluator;

        public Animator Animator
        {
            get => _animator;
            protected set => _animator = value;
        }

        public ActorEvents Events => _actorEvents;

        public ActorInitMode InitMode
        {
            get => initMode;
            set => initMode = value;
        }

        public bool IsInitialized { get; set; }

        public GameObject Model
        {
            get => _model;
            protected set => _model = value;
        }

        #endregion

        #region MonoBehaviour Events

        private void OnValidate()
        {
            if (!_autoDetectComponents) return;
            
            SearchForBasicComponents();
        }

        private void Awake()
        {
            if(initMode == ActorInitMode.Awake)
                Initialize();
        }

        protected void OnEnable()
        {
            if(initMode == ActorInitMode.Enable)
                Initialize();
            
            Events.OnActorEnabled?.Invoke(this);
        }

        protected void Start()
        {
            if(initMode == ActorInitMode.Start)
                Initialize();
        }
        
        private void OnDisable()
        {
            Events.OnActorDisabled?.Invoke(this);
        }

        protected void OnDestroy()
        {
            Events.OnActorDestroyed?.Invoke(this);
        }

        #endregion

        public void SearchForBasicComponents()
        {
            StatProvider = GetComponentInChildren<StatProvider>();
            DamageableResource = GetComponentInChildren<DamageableResource>();
            _animator = GetComponentInChildren<Animator>();
            
            HasDamageable = DamageableResource != null;
            HasStatProvider = StatProvider != null;
            HasAnimator = Animator != null;
        }
        
        public virtual void Initialize()
        {
            if (IsInitialized) return;
            
            HasDamageable = DamageableResource != null;
            HasStatProvider = StatProvider != null;
            HasAnimator = Animator != null;
            
            if (HasStatProvider)
            {
                StatProvider.Initialize();
            }
            
            UpdateEffectTargets();

            Events.OnActorInitialized?.Invoke(this);
            IsInitialized = true;
        }

        /// <summary>
        /// Updates the Actor's EffectTargets list from EffectTargets in the Actors hierarchy.
        /// </summary>
        public void UpdateEffectTargets()
        {
            _effectTargets.Clear();
            
            _effectTargets.AddRange(GetComponentsInChildren<EffectTarget>(true)
                .Where(o => o != null));
        }
        
        public void AddEffectTargets(IEnumerable<EffectTarget> targets)
        {
            _effectTargets.AddRange(targets);
        }
        
        /// <summary>
        /// Finds an EffectTarget with the given tag in the Actor's EffectTargets list.
        /// </summary>
        /// <param name="targetTag">Tag Asset to identify target.</param>
        /// <returns>Matching EffectTarget component in Actor's hierarchy.</returns>
        public EffectTarget FindEffectTarget(EffectTargetTag targetTag)
        {
            EffectTarget target = null;
            foreach (var o in _effectTargets)
            {
                if (o != null && o.TargetTag == targetTag)
                {
                    target = o;
                    break;
                }
            }

            if (target != null)
                return target;
                
            Debug.LogError($"[{nameof(Actor)}] Effect Target {targetTag.TagName} not found in Actor: {name}");
            
            return null;
        }

        public bool TryGetEffectTarget(EffectTargetTag effectTag, out EffectTarget target)
        {
            var obj = FindEffectTarget(effectTag);
            target = obj;

            return obj != null;
        }

        public EffectTarget[] FindAllEffectTargets(IEnumerable<EffectTargetTag> tags)
        {
            return _effectTargets
#if ACTOR_ZLINQ
                .AsValueEnumerable()
#endif
                .Where(et => et != null && tags.Contains(et.TargetTag))
                .ToArray();
        }

        protected internal void RegisterOwnership(OwnableBehaviour ownable)
        {
            ownedBehaviours.Add(ownable);
            OnRegisterOwnable?.Invoke(ownable);
        }

        protected internal void UnregisterOwnership(OwnableBehaviour ownable)
        {
            ownedBehaviours.Remove(ownable);
            OnUnRegisterOwnable?.Invoke(ownable);
        }

        public T GetOwnableBehaviour<T>() where T : OwnableBehaviour
        {
            return OwnedBehaviours
            #if ACTOR_ZLINQ
                .AsValueEnumerable()
            #endif
                .FirstOrDefault(ob => ob is T) as T;
        }
        
        public bool TryGetOwnableBehaviour<T>(out T behaviour) where T : OwnableBehaviour
        {
            behaviour = GetOwnableBehaviour<T>();
            return behaviour != null;
        }
        
        public T[] GetOwnableBehaviours<T>() where T : OwnableBehaviour
        {
            return OwnedBehaviours
            #if ACTOR_ZLINQ
                .AsValueEnumerable()
            #endif
                .Where(ob => ob is T)
                .Cast<T>().ToArray();
        }
    }
}
