using System;
using CoolTools.Utilities;
using UnityEngine;
using UnityEngine.Serialization;

namespace CoolTools.Actors
{
    [CreateAssetMenu(fileName = "New Actor Resource", menuName = "Actor/Resource", order = 0)]
    public class ActorResource : ScriptableObject
    {
        [FormerlySerializedAs("resourceName")] 
        [SerializeField] private string _resourceName;
        [SerializeField, SpritePreviewSmall] private Sprite _recourceIcon;
        
        public string ResourceName => _resourceName;
        public Sprite ResourceIcon => _recourceIcon;
    }
    
    [Serializable]
    public struct ResourceCost
    {
        public ActorResource Resource;
        public int Amount;
    }
}