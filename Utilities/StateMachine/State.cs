using System;
using System.Collections.Generic;
using UnityEngine;

namespace CoolTools.Utilities
{
    public abstract class State : IState
    {
        public StateMachine Context { get; }
        
        private readonly Dictionary<IState, Func<bool>> _transitions = new();

        public State(StateMachine context)
        {
            Context = context;
        }

        public void AddTransition(IState to, Func<bool> condition)
        {
            _transitions.Add(to, condition);
        }

        public IState EvaluateTransitions()
        {
            foreach (var kvp in _transitions)
            {
                if (kvp.Value.Invoke())
                {
                    return kvp.Key;
                }
            }
            
            return null;
        }

        void IState.OnEnter() => OnEnter();
        void IState.Update() => Update();
        void IState.OnExit() => OnExit();

        protected abstract void OnEnter();
        protected abstract void Update();
        protected abstract void OnExit();
    }
}