using System;
using UnityEngine;
using UnityEngine.Events;
using Object = UnityEngine.Object;

namespace CoolTools.Actors
{
    public class ActorAnimationEventListener : MonoBehaviour
    {
        [Serializable]
        public struct TargetEventInvoke
        {
            public AnimationEventSO Event;
            public UnityEvent OnEvent;
        }
        
        public UnityEvent<AnimationEventSO> OnEvent;
        
        [Space(10f)]
        public TargetEventInvoke[] TargetEvents;
        
        public AnimationEventSO LastEvent { get; private set; }

        private void Event(Object ev)
        {
            if (ev is not AnimationEventSO animEvent)
            {
                Debug.LogWarning($"[{nameof(ActorAnimationEventListener)}] Animation event Object input must be of type {nameof(AnimationEventSO)}");
                return;
            }

            OnEvent?.Invoke(animEvent);
            
            LastEvent = animEvent;
            
            foreach (var targetEvent in TargetEvents)
            {
                if (targetEvent.Event == animEvent)
                {
                    targetEvent.OnEvent?.Invoke();
                }
            }
        }
    }
}