using System.Collections.Generic;
using UnityEngine;

namespace CoolTools.Data
{
    public class GameEventSO<T> : ScriptableObject
    {
        [SerializeField] private bool eventEnabled = true;
        [SerializeField] private bool logEvent;
        [SerializeField, TextArea] private string description;
        
        [Header("Debug And Test")]
        [SerializeField] private T testData;
        
        public T TestData
        {
            get => testData;
            set => testData = value;
        }

        private readonly List<IGameEventListener<T>> listeners = new();

        private void OnEnable()
        {
            listeners.Clear();
        }

        public virtual void Raise(T data)
        {
            if (!eventEnabled) return;
            
            if(logEvent)
                Debug.Log($"[{nameof(GameEventSO<T>)}] Event {name} raised with data {data}", this);
            
            for(int i = listeners.Count -1; i >= 0; i--)
                listeners[i].OnEventRaised(data);
        }

        public void Subscribe(IGameEventListener<T> listener, GameObject contextObject = null)
        {
            listeners.Add(listener);
        }

        public void Unsubscribe(IGameEventListener<T> listener)
        {
            var index = listeners.IndexOf(listener);
            
            listeners.Remove(listener);
        }
        
        public void UnsubscribeAll()
        {
            listeners.Clear();
        }

        public CustomYieldInstruction GetYieldInstruction() => new WaitUntilEvent(this);
        
        private class WaitUntilEvent : CustomYieldInstruction, IGameEventListener<T>
        {
            public override bool keepWaiting => !called;

            private bool called;

            public WaitUntilEvent(GameEventSO<T> gameEvent)
            {
                gameEvent.Subscribe(this);
            }

            public void OnEventRaised(T data)
            {
                called = true;
            }
        }
    }
}