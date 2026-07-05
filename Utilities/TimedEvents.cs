using System;
using CoolTools.Utilities;
using UnityEngine;
using UnityEngine.Events;

namespace CoolTools.Utilities
{
    public class TimedEvents : MonoBehaviour
    {
        [Serializable]
        public struct TimedEvent
        {
            [InspectorDisabled] public bool Played;
            public float Time;
            public UnityEvent Event;
        }

        [SerializeField] private TimedEvent[] _events;

        private float _elapsedTime;

        private void OnEnable()
        {
            _elapsedTime = 0f;
            
            for (int i = 0; i < _events.Length; i++)
            {
                var evnt = _events[i];
                evnt.Played = false;

                _events[i] = evnt;
            }
        }

        private void Update()
        {
            _elapsedTime += Time.deltaTime;
            
            for (int i = 0; i < _events.Length; i++)
            {
                var evnt = _events[i];
                if (evnt.Played) continue;
                
                if (_elapsedTime >= evnt.Time)
                {
                    evnt.Event.Invoke();
                    evnt.Played = true;
                }
                
                _events[i] = evnt; // Update the event in the array
            }
        }
    }
}
