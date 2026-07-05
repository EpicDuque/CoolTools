using System;
using System.Collections.Generic;
using CoolTools.Utilities;
using Cooltools.Utilities;
using UnityEngine;

namespace CoolTools.Actors
{
    public class DetectableScanner : OwnableBehaviour
    {
        [ColorSpacer("Scanner")]
        [SerializeField] private LayerMask _layerMask;
        [SerializeField] private int _scanIntervalFixedFrame = 1;
        [SerializeField] private int _maxDetectedAmount = 8;
        
        [Space(5f)]
        [SerializeField] private FloatValueConfig _scanRadius;
        
        [Header("Debug")] 
        [SerializeField] private bool _alwaysDrawGizmo = false;
        [SerializeField] private Color _gizmoColor = Color.red;
        [SerializeField, InspectorDisabled] private int _fixedStepCount;
        [SerializeField, InspectorDisabled] private Detectable[] _detectedTargets;
        [SerializeField, InspectorDisabled] private int _detectedAmount = 0;
        [SerializeField, InspectorDisabled] private int _scannerId;

        private Collider[] _hits;
        private List<Detectable> _sortList = new();
        private DistanceComparator _distanceComparator;
        private List<Detectable> _objectsToRemove = new();

        public Detectable[] DetectedTargets => _detectedTargets;
        public Detectable FirstTarget => _detectedAmount > 0 ? _detectedTargets[0] : default;
        public int DetectedAmount => _detectedAmount;

        public int MaxDetectedAmount => _maxDetectedAmount;

        public FloatValueConfig ScanRadius => _scanRadius;

        public static int GlobalCount = 0;
        
        // public ScannerSystem.ScanData ScanData { get; set; }


        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void ReloadDomain()
        {
            GlobalCount = 0;
        }

        private void OnValidate()
        {
            _scanRadius.UpdateValue(this);
        }

        private new void Awake()
        {
            base.Awake();
            
            _scannerId = GlobalCount;
            GlobalCount++;
            
            _hits = new Collider[_maxDetectedAmount];
            _detectedTargets = new Detectable[_maxDetectedAmount];
            _distanceComparator = new DistanceComparator(transform);
            
            for(int i = 0; i < _maxDetectedAmount; i++)
            {
                _detectedTargets[i] = Detectable.Null;
            }
        }

        private void OnDisable()
        {
            ClearDetectedTargets();
        }

        private void Update()
        {
            _objectsToRemove.Clear();
            
            foreach (var obj in _detectedTargets)
            {
                if(obj == Detectable.Null) continue;
                
                if(obj.BypassDetection)
                    _objectsToRemove.AddUnique(obj);
                
                if(!obj.gameObject.activeSelf)
                    _objectsToRemove.AddUnique(obj);
                
                if (obj == null)
                    _objectsToRemove.AddUnique(obj);
            }

            if (_objectsToRemove.Count > 0)
            {
                foreach (var obj in _objectsToRemove)
                {
                    OnDetectedDisable(obj);
                }
                
                SortDetectedTargets();
            }
        }

        private void FixedUpdate()
        {
            _fixedStepCount++;
            
            if (_scannerId % _scanIntervalFixedFrame != _fixedStepCount % _scanIntervalFixedFrame) 
                return;
            
            Scan();
        }

        protected override void OnStatsUpdated()
        {
            base.OnStatsUpdated();
            
            _scanRadius.UpdateValue(Owner);
        }

        private void Scan()
        {
            ClearDetectedTargets();
            _detectedAmount = 0;
            
            var hits = Physics.OverlapSphereNonAlloc(transform.position, _scanRadius.Value, _hits, _layerMask);
            if (hits == 0)
            {
                return;
            }

            PopulateDetectedTargets(hits);

            SortDetectedTargets();
        }

        private void PopulateDetectedTargets(int hits)
        {
            int slot = 0;
            // Store all the IDamageables in a list and sort them using the DistanceComparator
            for (int i = 0; i < hits; i++)
            {
                var hit = _hits[i];
                Detectable detectable;
                if (hit.TryGetComponent(out detectable) && !detectable.BypassDetection && detectable.Owner != Owner)
                {
                    _detectedTargets[slot] = detectable;
                    
                    _detectedAmount++;
                    slot++;
                }
            }
        }
        
        private void OnDetectedDisable(Detectable detectable)
        {
            var index = Array.IndexOf(_detectedTargets, detectable);
            if (index != -1)
            {
                _detectedTargets[index] = Detectable.Null;
                _detectedAmount--;
            }
        }

        private void ClearDetectedTargets()
        {
            for (var i = 0; i < _detectedTargets.Length; i++)
            {
                _detectedTargets[i] = Detectable.Null;
            }
            
            // DO NOT SET DETECTED AMOUNT = 0 HERE, IT BREAKS THE SCANNER
        }
        
        public void SortDetectedTargets()
        {
            // Array.Sort(_detectedTargets, 0, _detectedAmount, _distanceComparator);
            _sortList.Clear();

            for (int i = 0; i < _detectedAmount; i++)
            {
                if (_detectedTargets[i] == Detectable.Null) continue;
                if (!_detectedTargets[i].gameObject.activeInHierarchy) continue;
                
                _sortList.Add(_detectedTargets[i]);
            }
            
            _sortList.Sort(_distanceComparator);
            
            ClearDetectedTargets();
            
            for (int i = 0; i < _sortList.Count; i++)
            {
                _detectedTargets[i] = _sortList[i];
            }
        }
        
        private class DistanceComparator : IComparer<Detectable>
        {
            private readonly Transform _transform;

            public DistanceComparator(Transform transform)
            {
                _transform = transform;
            }

            public int Compare(Detectable a, Detectable b)
            {
                var aDistance = Vector3.SqrMagnitude(_transform.position - a.TargetPoint.position);
                var bDistance = Vector3.SqrMagnitude(_transform.position - b.TargetPoint.position);

                return aDistance.CompareTo(bDistance);
            }
        }
        
        private void OnDrawGizmos()
        {
            if (!_alwaysDrawGizmo) return;
            
            Gizmos.color = _gizmoColor;
            Gizmos.DrawWireSphere(transform.position, _scanRadius.Value);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = _gizmoColor;
            Gizmos.DrawWireSphere(transform.position, _scanRadius.Value);
        }

        public void UpdateResults(ColliderHit[] results, int hits)
        {
            ClearDetectedTargets();
            _detectedAmount = 0;
            
            int slot = 0;
            // Store all the IDamageables in a list and sort them using the DistanceComparator
            for (int i = 0; i < hits; i++)
            {
                if (results[i].entityId == EntityId.None) continue;
                
                var hit = results[i].collider;

                if (hit.TryGetComponent(out Detectable detectable) && !detectable.BypassDetection)
                {
                    _detectedTargets[slot] = detectable;
                    
                    _detectedAmount++;
                    slot++;
                }
            }
            
            SortDetectedTargets();
        }
    }
}