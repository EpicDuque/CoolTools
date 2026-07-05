using CoolTools.Utilities;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace CoolTools.Utilities
{
    public class PoolableObject : MonoBehaviour
    {
        [field:SerializeField, InspectorDisabled] public ObjectPool Pool { get; set; }
        [InspectorDisabled] public Transform PoolParent;
        
        public UnityEvent ObjectPulled;
        public UnityEvent ObjectReturned;

        public virtual void Initialize()
        {
            gameObject.SetActive(true);
            ObjectPulled?.Invoke();
        }
        
        public virtual void ReturnToPool()
        {
            if (Pool == null)
            {
                Destroy(gameObject);
                return;
            }
            
            ObjectReturned?.Invoke();
            
            gameObject.SetActive(false);
            Pool.Put(this);
        }

        public static void DestroyOrReturn(GameObject obj)
        {
            // obj.transform.localScale = Vector3.one;
            
            if (obj.TryGetComponent<PoolableObject>(out var poolable))
            {
                poolable.ReturnToPool();
            }
            else
            {
                Destroy(obj);
            }
        }
    }
}