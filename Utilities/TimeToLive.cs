using CoolTools.Utilities;
using UnityEngine;
using UnityEngine.Events;

namespace CoolTools.Utilities
{
    /// <summary>
    /// Used to automatically destroy or disable an object after a certain amount of time.
    /// If a PoolableObject is attached, it will return to the pool instead.
    /// </summary>
    public class TimeToLive : MonoBehaviour
    {
        [SerializeField, InspectorDisabled] private bool _hasPoolableObject;
        [SerializeField, InspectorDisabled] private PoolableObject _poolableObject;
        
        [Space(10f)]
        [SerializeField] private float _timeToLive;
        [SerializeField] private bool _autoDispose = true;
        [SerializeField] private bool _disableInstead;
        [SerializeField, InspectorDisabled] private float _count;

        [Space(10f)]
        public UnityEvent OnTimeOut;

        public float TTL
        {
            get => _timeToLive;
            set => _timeToLive = value;
        }

        private bool _disposing = false;

        private void OnValidate()
        {
            _hasPoolableObject = TryGetComponent(out _poolableObject);
        }

        private void OnEnable()
        {
            ResetCount();
        }

        private void OnDisable()
        {
            _count = 0f;
        }

        public void ResetCount()
        {
            _count = 0f;
            _disposing = false;
        }

        private void Update()
        {
            _count += Time.deltaTime;

            if (!_disposing && _count >= _timeToLive)
            {
                _disposing = true;
                OnTimeOut?.Invoke();
                DestroyObject();
            }
        }

        private void DestroyObject()
        {
            if (!_autoDispose) return;

            if (_disableInstead)
            {
                gameObject.SetActive(false);
                return;
            }
            
            if (_hasPoolableObject)
            {
                _poolableObject.ReturnToPool();
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}