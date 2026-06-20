using UnityEngine;

namespace CoolTools.Actors
{
    public abstract class EffectBase : ScriptableObject
    {
        [SerializeField] private string _effectName;
        [SerializeField, TextArea] private string _effectDescription;
        
        public string EffectName => _effectName;

        public string EffectDescription => _effectDescription;

        public virtual void Execute(Actor source, Detectable target)
        {
        }

        public virtual void Execute(Actor source, Actor target)
        {
        }

        public virtual void Execute(Actor source, Vector3 target)
        {
        }
        
        public virtual void Execute(Actor target)
        {
        }

        // ReSharper disable Unity.PerformanceAnalysis
        public virtual void ResetEffect(Actor target)
        {
        }
    }
}