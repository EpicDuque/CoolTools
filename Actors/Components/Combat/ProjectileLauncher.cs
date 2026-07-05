using System;
using CoolTools.Utilities;
using CoolTools.Utilities;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Events;

namespace CoolTools.Actors
{
    public class ProjectileLauncher : OwnableBehaviour
    {
        [Serializable]
        public struct ProjectileLaunchEvents
        {
            public UnityEvent OnLaunched;
            public UnityEvent<Projectile> OnLaunchedProjectile;
        }

        [ColorSpacer("Projectile Launcher")]
        [SerializeField] private Projectile _projectilePrefab;
        [SerializeField] private ObjectPoolConfig _poolConfig;
        
        [Space(10f)] 
        [SerializeField] private LauncherModule _launchModule;
        [SerializeField, InspectorDisabled] private LauncherModule _moduleInstance;
        
        [Space(10f)]
        [SerializeField] private Transform _launchPoint;
        [SerializeField] private FloatValueConfig _launchForce;

        [Space(10f)] 
        [SerializeField] private ProjectileLaunchEvents _events;

        private static ObjectPool Pool;
        private bool _usePool;
        private bool _hasPool;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void ResetDomain()
        {
            Pool = null;
        }
        
        public Detectable Target { get; set; } = Detectable.Null;
        public Vector3 TargetPosition { get; set; }
        
        public FloatValueConfig LaunchForce => _launchForce;

        public Transform LaunchPoint
        {
            get => _launchPoint;
            set => _launchPoint = value;
        }

        public Projectile ProjectilePrefab
        {
            get => _projectilePrefab;
            set => _projectilePrefab = value;
        }

        public ProjectileLaunchEvents Events => _events;

        public LauncherModule ModuleInstance => _moduleInstance;
        
        public TargetMode ETargetMode { get; set; }

        public enum TargetMode
        {
            None,
            Detectable,
            Position,
        }

        private void Start()
        {
            _moduleInstance = Instantiate(_launchModule);
        }

        protected override void OnStatsUpdated()
        {
            base.OnStatsUpdated();

            _launchForce.UpdateValue(this);
        }

        private void OnValidate()
        {
            _launchForce.UpdateValue(this);
        }

        protected new void Awake()
        {
            base.Awake();

            _usePool = _poolConfig != null;
        }

        public virtual void Launch()
        {
            if (_usePool)
            {
                if (!_hasPool)
                {
                    Pool = ObjectPool.GetPool(_poolConfig);
                    _hasPool = Pool != null;
                }
            }
            
            LaunchForce.UpdateValue(this);
            
            ModuleInstance.UsePool = _hasPool;
            ModuleInstance.Launch(this, Pool);
            
            Events.OnLaunched?.Invoke();
        }
    }
}