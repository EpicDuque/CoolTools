using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

namespace CoolTools.Utilities
{
    public class ObjectPool : MonoBehaviour
    {
        [FormerlySerializedAs("config")] 
        [SerializeField] private ObjectPoolConfig _config;
        [SerializeField] private bool _autoInitialize;
        
        [Header("Misc")] 
        [SerializeField] private bool _instantiateAsync;
        [SerializeField] private bool showWarnings;
        [SerializeField] private bool showErrors;
        
        [Space(10f)]
        public UnityEvent PoolCreated;
        
        [Header("Add Objects Helper")]
        [SerializeField] private List<PoolableObject> objectsToAdd;
        [SerializeField] private int amountPerObject;
        private static Dictionary<string, ObjectPool> pools = new ();
        
        private readonly Dictionary<string, Queue<PoolableObject>> cachedPool = new();
        
        private List<Transform> parents = new();

        public bool IsCreated { get; protected set; }
        public float LoadingPercent { get; protected set; }
        public string PoolName => _config.PoolName;

        public ObjectPoolConfig Config => _config;

        public PoolableObject[] AllObjects
        {
            get
            {
                var list = new List<PoolableObject>();
                foreach (var queue in cachedPool.Values)
                {
                    list.AddRange(queue);
                }

                return list.ToArray();
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void ReloadDomain()
        {
            pools = new();
        }

        public void Awake()
        {
            pools.Add(PoolName, this);
        }

        private void OnEnable()
        {
            if (_autoInitialize && !IsCreated)
            {
                Initialize();
            }
        }

        public void OnDestroy()
        {
            pools.Remove(PoolName);
            DestroyPool();
        }

        public void Initialize()
        {
            IsCreated = false;
            
            StartCoroutine(PoolCreationRoutine());
        }
        
        #if UNITY_EDITOR
        public void AddObjectListToPool()
        {
            foreach (var obj in objectsToAdd)
            {
                var data = new ObjectPoolConfig.PoolData
                {
                    Key = obj.name,
                    Prefab = obj,
                    Size = amountPerObject
                };

                _config.PoolDataList.Add(data);
            }
        }
        #endif

        public void AddObjectToPool(PoolableObject obj, int amount)
        {
            var data = new ObjectPoolConfig.PoolData
            {
                Key = obj.name,
                Prefab = obj,
                Size = amount
            };

            _config.PoolDataList.Add(data);
        }


        public void DestroyPool()
        {
            foreach (var p in parents)
            {
                if (p == null || p.gameObject == null) continue;
                Destroy(p.gameObject);
            }
            
            parents.Clear();
            cachedPool.Clear();
        }

        private IEnumerator PoolCreationRoutine()
        {
            DestroyPool();
            
            IsCreated = false;
            LoadingPercent = 0f;
            
            var poolsCreated = 0;
            
            var operations = new List<AsyncInstantiateOperation<GameObject>>();
            _config.Pool = this;
            
            foreach(var data in _config.PoolDataList)
            {
                if (data.Prefab == null) continue;
                
                var q = new Queue<PoolableObject>();
                var parent = new GameObject($"Pool_{data.Key}");
                parent.transform.position = new Vector3(0f, 1000f, 0f);
                
                SceneManager.MoveGameObjectToScene(parent, gameObject.scene);
                
                parents.Add(parent.transform);
                
                operations.Clear();
                
                if (data.Size > 0)
                {
                    if (_instantiateAsync)
                    {
                        var op = InstantiateAsync(data.Prefab.gameObject, data.Size, 
                            parent: parent.transform);
                        
                        op.allowSceneActivation = true;
                        operations.Add(op);
                    }
                    else
                    {
                        
                        for (int i = 0; i < data.Size; i++)
                        {
                            var obj = Instantiate(data.Prefab, parent.transform);
                            obj.Pool = this;
                            obj.PoolParent = parent.transform;
                            obj.transform.localPosition = Vector3.zero;
                            obj.transform.localRotation = Quaternion.identity;
                            obj.gameObject.SetActive(false);
                            
                            q.Enqueue(obj);
                        }
                    }
                }

                if (_instantiateAsync)
                {
                    yield return new WaitUntil(() => operations.All(op => op.progress >= 0.99f));
                    
                    foreach (var op in operations)
                    {
                        // op.allowSceneActivation = true;
                        // yield return op;
                        for (int i = 0; i < op.Result.Length; i++)
                        {
                            var obj = op.Result[i].GetComponent<PoolableObject>();
                            obj.gameObject.SetActive(false);
                            
                            obj.Pool = this;
                            obj.transform.localPosition = Vector3.zero;
                            obj.transform.localRotation = Quaternion.identity;
                            
                            q.Enqueue(obj);
                        }
                    }
                }

                poolsCreated++;
                LoadingPercent = poolsCreated / (float)_config.PoolDataList.Count;
                cachedPool.Add(data.Key, q);
                yield return null;
            }
            
            LoadingPercent = 1f;
            PoolCreated?.Invoke();

            yield return null;
            IsCreated = true;
        }
        
        public PoolableObject Pull(GameObject obj, Vector3 position, Quaternion rotation)
        {
            var scene = SceneManager.GetActiveScene();
            
            return Pull(obj.name, position, rotation, scene);
        }
        
        public PoolableObject Pull(GameObject obj, Vector3 position, Quaternion rotation, Scene scene)
        {
            return Pull(obj.name, position, rotation, scene);
        }
        
        // ReSharper disable Unity.PerformanceAnalysis
        public PoolableObject Pull(string key, Vector3 position, Quaternion rotation, Scene moveToScene)
        {
            if (!cachedPool.TryGetValue(key, out var q))
            {
                if(showErrors)
                    Debug.LogError($"[{nameof(ObjectPool)}] Attempted to pull non-existing key: {key} from pool: {PoolName}", gameObject);
                
                return null;
            }
            
            if (!q.TryDequeue(out var obj))
            {
                if (showWarnings)
                {
                    Debug.LogWarning($"[{nameof(ObjectPool)}] Stretching Pool: {PoolName} | Instantiated {key}. \n" +
                                     $"New Pool Size: {q.Count+1}", gameObject);
                }

                var poolData = new ObjectPoolConfig.PoolData();
                foreach (var d in _config.PoolDataList)
                {
                    if (d.Key == key)
                    {
                        poolData = d;
                        break;
                    }
                }

                var prefab = poolData.Prefab;
                Transform parent = null;
                bool hasParent = false;
                foreach (var p in parents)
                {
                    if (p.name.Equals($"Pool_{key}"))
                    {
                        parent = p;
                        hasParent = true;
                        break;
                    }
                }

                if (!hasParent)
                {
                    if(showErrors)
                        Debug.LogError($"[{nameof(ObjectPool)}] No Parent with name: {key} found", gameObject);
                }

                var instance = hasParent ? Instantiate(prefab, parent.transform) : Instantiate(prefab);
                instance.Pool = this;
                instance.PoolParent = hasParent ? parent.transform : null;
                instance.transform.localPosition = Vector3.zero;
                instance.transform.localRotation = Quaternion.identity;
                instance.gameObject.SetActive(false);
                    
                q.Enqueue(instance);
                obj = q.Dequeue();
            }

            Transform myTransform;
            (myTransform = obj.transform).SetParent(null);
            myTransform.position = position;
            myTransform.rotation = rotation;
            
            SceneManager.MoveGameObjectToScene(obj.gameObject, moveToScene);
            
            obj.Initialize();
            return obj;
        }

        public T Pull<T>(string key, Vector3 position, Quaternion rotation, Scene moveToScene) where T : Component
        {
            var obj = Pull(key, position, rotation, moveToScene);
            
            return !obj.TryGetComponent(out T component) ? null : component;
        }

        public T Pull<T>(string key, Transform parent, Scene moveToScene) where T : Component
        {
            var obj = Pull(key, parent.position, parent.rotation, moveToScene);
            
            if (!obj.TryGetComponent(out T component))
                return null;

            obj.transform.SetParent(parent);
            return component;
        }
        
        public T Pull<T>(T prefab, Vector3 position, Quaternion rotation, Scene moveToScene) where T : Component
        {
            return Pull<T>(prefab.name, position, rotation, moveToScene);
        }

        // ReSharper disable Unity.PerformanceAnalysis
        /// <summary>
        /// Returns this GameObject to it's corresponding pool. Disables the GameObject.
        /// </summary>
        /// <param name="instance"></param>
        public void Put(PoolableObject instance)
        {
            // Try to remove instance.name since it's a heavy operation
            var key = instance.name.Replace("(Clone)", "").Trim();
            
            var parent = instance.PoolParent;
            var q = cachedPool[key];
            
            if(instance.gameObject.activeSelf)
                instance.gameObject.SetActive(false);
            
            instance.transform.SetParent(null);
            SceneManager.MoveGameObjectToScene(instance.gameObject, gameObject.scene);

            // Transform myTransform;
            // (myTransform = instance.transform).SetParent(parent);
            // myTransform.parent = parent;
            var myTransform = instance.transform;
            myTransform.SetParent(parent, false);

            myTransform.localPosition = Vector3.zero;
            myTransform.localRotation = Quaternion.identity;
            
            q.Enqueue(instance);
        }

        public static ObjectPool GetPool(string poolName)
        {
            return pools.ContainsKey(poolName) ? pools[poolName] : null;
        }
        
        public static ObjectPool GetPool(ObjectPoolConfig config) => GetPool(config.PoolName);
        
        public static bool PoolValidAndCreated(ObjectPoolConfig config, out ObjectPool objectPool)
        {
            objectPool = GetPool(config);

            return objectPool != null && objectPool.IsCreated;
        }
    }
}
