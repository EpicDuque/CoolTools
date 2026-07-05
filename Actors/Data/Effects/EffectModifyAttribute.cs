using UnityEngine;

namespace CoolTools.Actors
{
    [CreateAssetMenu(fileName = "New Modify Attribute Effect", menuName = "Actor/Effects/Modify Attribute")]
    public class EffectModifyAttribute : EffectBase
    {
        [SerializeField] protected StatProvider.AttributeModification[] _modifications;
        
        public override void Execute(Actor source, Actor target)
        {
            foreach(var mod in _modifications)
            {
                var currentMod = target.StatProvider.GetAttributeModification(mod.Attribute);
                
                currentMod.Multiplier *= mod.Multiplier;
                currentMod.Offset += mod.Offset;
                
                target.StatProvider.SetAttributeModification(currentMod, false);
            }
            
            target.StatProvider.UpdateActorCurrentStats();
        }

        public override void Execute(Actor target)
        {
            Execute(target, target);
        }

        public override void ResetEffect(Actor target)
        {
            base.ResetEffect(target);
            
            foreach(var mod in _modifications)
            {
                var currentMod = target.StatProvider.GetAttributeModification(mod.Attribute);
                
                currentMod.Multiplier /= mod.Multiplier;
                currentMod.Offset -= mod.Offset;
                
                target.StatProvider.SetAttributeModification(currentMod, false);
            }
            
            target.StatProvider.UpdateActorCurrentStats();
        }
    }
}