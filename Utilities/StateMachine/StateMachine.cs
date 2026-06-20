using System;
using System.Collections.Generic;
using UnityEngine;

namespace CoolTools.Utilities
{
    public class StateMachine
    {
        public IState CurrentState { get; protected set; }
        public IState PreviousState { get; protected set; }

        public void Initialize(IState state)
        {
            CurrentState = state;
            CurrentState.OnEnter();
        }

        public virtual void UpdateFSM()
        {
            var newState = CurrentState.EvaluateTransitions();

            if (newState != null)
            {
                TransitionTo(newState);
                return;
            }
            
            CurrentState.Update();
        }

        // ReSharper disable Unity.PerformanceAnalysis
        public virtual void TransitionTo(IState newState)
        {
            CurrentState.OnExit();
            
            PreviousState = CurrentState;
            CurrentState = newState;
            
            CurrentState.OnEnter();
        }
    }
}