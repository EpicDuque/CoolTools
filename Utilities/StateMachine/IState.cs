using System;

namespace CoolTools.Utilities
{
    public interface IState
    {
        StateMachine Context { get; }
        
        void OnEnter();

        void Update();

        void OnExit();
        
        void AddTransition(IState to, Func<bool> condition);
        
        IState EvaluateTransitions();
    }
}