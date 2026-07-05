using CoolTools.Utilities;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace CoolTools.Data
{
    public abstract class GameEventListener<T, ET> : MonoBehaviour, IGameEventListener<ET> 
        where T : GameEventSO<ET>
    {
        public T Event;
        [FormerlySerializedAs("unsubscribeOnDisable")] [SerializeField] protected bool _unsubscribeOnDisable = true;
        
        [FormerlySerializedAs("subscribeWithContext")]
        [Tooltip("This will allow the Event ScriptableObject's inspector to have a proper serialized field of this subscriber. " +
                 "Might have a bit of performance impact, but it's not noticeable.")]
        [SerializeField] protected bool _subscribeWithContext = false;
        [SerializeField] protected bool _logEventListen;
        
        [Space(10f)] 
        public UnityEvent<ET> Response;

        [Space(10f)]
        [InspectorDisabled] public ET LastData;

        private bool _subscribed;

        private void OnEnable()
        {
            if (_subscribed) return;
            
            Event.Subscribe(this, _subscribeWithContext ? gameObject : null);
            _subscribed = true;
        }

        private void OnDisable()
        {
            if(!_unsubscribeOnDisable) return;
            
            Event.Unsubscribe(this);
            _subscribed = false;
        }

        public virtual void OnEventRaised(ET data)
        {
            if(_logEventListen)
                Debug.Log($"[{nameof(GameEventListener<T, ET>)} {nameof(T)}] Received event {Event.name} with data {data}", this);
            
            LastData = data;

            Response.Invoke(data);
        }
    }
}
