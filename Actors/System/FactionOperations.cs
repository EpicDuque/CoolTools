using System;
using System.Linq;

namespace CoolTools.Actors
{
    public static class FactionOperations
    {
        [Serializable]
        public enum FactionFilterMode
        {
            NotOwner, OnlyOwner, Include, Exclude
        }
        
        public static bool IsValidFaction(ActorFaction target, ActorFaction[] factionList, FactionFilterMode mode)
        {
            return (mode == FactionFilterMode.Include && factionList.Contains(target)) ||
                   (mode == FactionFilterMode.Exclude && !factionList.Contains(target));
        }
    }
}