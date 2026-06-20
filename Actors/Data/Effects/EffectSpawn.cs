using CoolTools.Utilities;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CoolTools.Actors
{
    [CreateAssetMenu(fileName = "New Effect Spawn", menuName = "Actor/Effects/Spawn", order = 0)]
    public class EffectSpawn : EffectBase
    {
        [SerializeField] private GameObject _spawnObject;
        
        [Space(10f)]
        [SerializeField] private bool _usePool;
        [SerializeField] private ObjectPoolConfig _poolConfig;
        
        [Space(10f)]
        [SerializeField] private bool _spawnWithOwnership;
        
        [Space(10f)]
        [SerializeField] private bool _localPositionOffset;
        [SerializeField] private Vector3 _positionOffset;
        
        [Space(10f)] 
        [SerializeField] private Vector3 _scaleOffset;

        public GameObject SpawnObject
        {
            get => _spawnObject;
            set => _spawnObject = value;
        }

        public override void Execute(Actor target)
        {
            var position = ResolvePosition(target);
            
            var spawn = SpawnOrPull(_spawnObject, position, target.transform.rotation);
            spawn.transform.localScale += _scaleOffset;
            
            if(_spawnWithOwnership)
                AssignOwnership(target, spawn);
        }

        public override void Execute(Actor source, Detectable target)
        {
            var position = ResolvePosition(source);
            
            // var spawn = Instantiate(_spawnObject, position, target.TargetPoint.rotation);
            var spawn = SpawnOrPull(_spawnObject, position, target.TargetPoint.rotation);
            spawn.transform.localScale += _scaleOffset;
            
            if(_spawnWithOwnership)
                AssignOwnership(source, spawn);
        }
        
        public override void Execute(Actor source, Vector3 target)
        {
            var position = target + _positionOffset;
            
            // var spawn = Instantiate(_spawnObject, position, Quaternion.identity);
            var spawn = SpawnOrPull(_spawnObject, position, Quaternion.identity);
            
            if(_spawnWithOwnership)
                AssignOwnership(source, spawn);
        }

        public override void Execute(Actor source, Actor target)
        {
            Execute(source, target.transform.position);
        }
        
        private GameObject SpawnOrPull(GameObject obj, Vector3 position, Quaternion rotation)
        {
            var activeScene = SceneManager.GetActiveScene();
            
            if (_usePool)
            {
                var pool = ObjectPool.GetPool(_poolConfig.PoolName);
                var instance = pool.Pull(_spawnObject.name, position, rotation, activeScene);

                return instance != null ? instance.gameObject : Instantiate(obj, position, rotation);
            }

            return Instantiate(obj, position, rotation);
        }
        
        private Vector3 ResolvePosition(Actor source)
        {
            return _localPositionOffset ? 
                source.transform.position + source.transform.TransformDirection(_positionOffset) :
                source.transform.position + _positionOffset;
        }

        private void AssignOwnership(Actor source, GameObject spawn)
        {
            if (spawn.TryGetComponent(out OwnableBehaviour ownable))
            {
                ownable.Owner = source;
            }
            else
            {
                Debug.LogWarning($"[{nameof(EffectSpawn)}] {spawn.name} does not have {nameof(OwnableBehaviour)} component.");
            }
        }
    }
}