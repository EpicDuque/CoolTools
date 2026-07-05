using System;
using CoolTools.Utilities;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;

namespace CoolTools.Actors
{
    /// <summary>
    /// This component can be used as a base for Actor movement, but you are free to implement your own movement logic.
    /// </summary>
    public class MovementBehaviour : OwnableBehaviour, IInputMovable, IInputLookable
    {
        [Serializable]
        public class MovementSettings
        {
            // [HelpBox("Deprecated, use CCMovementSettings instead.")]
            [Tooltip("How fast the character turns to face movement direction")]
            [Range(0f, 50f)]
            public float RotationSmoothTime;
            public float MovementAccel;

            [Space(10f)] 
            public bool UseGravity = true;
            public LayerMask WhatIsGround;
            public float GravityMultiplier = 1f;
            public float TerminalVelocity;
        }
        
        [Serializable]
        public class AnimationParams
        {
            public bool NormalizeBlend;
            public string RunBlendParam = "RunBlend";
            public string StrafeBlendParam = "StrafeBlend";
            public string MotionSpeedParam = "MotionSpeed";
            
            [HideInInspector] public int animID_RunBlend;
            [HideInInspector] public int animID_StrafeBlend;
            [HideInInspector] public int animID_MotionSpeed;
            
            public void CreateHashes()
            {
                animID_RunBlend = !string.IsNullOrEmpty(RunBlendParam)  ? Animator.StringToHash(RunBlendParam) : -1;
                animID_StrafeBlend = !string.IsNullOrEmpty(StrafeBlendParam)  ? Animator.StringToHash(StrafeBlendParam) : -1;
                animID_MotionSpeed = !string.IsNullOrEmpty(MotionSpeedParam)  ? Animator.StringToHash(MotionSpeedParam) : -1;
            }
        }
        
        [ColorSpacer("Movement Behaviour")] 
        [SerializeField] protected CharacterController _characterController;
        [SerializeField] private NavMeshAgent _navMeshAgent;

        [Space(10f)] 
        [SerializeField] private Transform _rotationY;
        
        [Space(10f)]
        [SerializeField] protected MovementSettings _movementSettings;
        
        [Space(5f)]
        [SerializeField] protected FloatValueConfig _maxMovementSpeed;

        [Space(10f)] 
        [SerializeField] private AnimationParams _animationParams;
        
        [FormerlySerializedAs("speed")]
        [ColorSpacer("Debug")]
        [SerializeField, InspectorDisabled] protected float _speed;
        [SerializeField, InspectorDisabled] private Vector2 _movementInput;
        [SerializeField, InspectorDisabled] private float _valueFromCurve;
        
        protected Vector3 _lastPosition;

        private bool _limitSpeed = true;
        private bool _hasRotationX;
        private float walkRunAnimBlend;
        private float strafeAnimBlend;
        
        private float targetRotationAngle;
        private float fallTimeoutDelta;
        private float verticalVelocity;
        private float rotationVelocityY;
        private float rotationVelocityX;
        
        private Collider[] _groundDetectResults;

        public FloatValueConfig MaxMovementSpeed
        {
            get => _maxMovementSpeed;
            set => _maxMovementSpeed = value;
        }
        
        public MovementSettings Settings => _movementSettings;

        public bool HasCharacterController { get; protected set; }
        public bool HasNavMeshAgent { get; protected set; }

        public CharacterController CharacterController
        {
            get => _characterController;
            protected set => _characterController = value;
        }
        
        public NavMeshAgent NavMeshAgent => _navMeshAgent;
        
        public float Acceleration => _movementSettings.MovementAccel;

        /// <summary>
        /// Represents the intention of movement by the Actor.
        /// MovementInput should be provided as an X Y input (Joystick or Buttons) that projects into a X Z plane (Y input is Z axis in world).
        /// </summary>
        public Vector2 MovementInput
        {
            get => _movementInput;
            set
            {
                _movementInput = value;
                // if (_movementInput == Vector2.zero)
                //     Stop();
            }
        }
        
        /// <summary>
        /// Current velocity of the Actor (Read Only)
        /// </summary>
        [field: SerializeField, InspectorDisabled]
        public Vector3 Velocity { get; set; }
        
        /// <summary>
        /// Look direction target for the actor. Actor will lerp it's rotation towards this direction.
        /// </summary>
        public Vector3 LookInput { get; set; }
        public bool CanMove { get; set; } = true;
        public bool CanRotate { get; set; } = true;
        [field:SerializeField] public bool IsGrounded { get; set; }

        /// <summary>
        /// Resultant scalar movement speed of this Actor.
        /// </summary>
        public float Speed
        {
            get => _speed;
            set => _speed = value;
        }
        
        protected new void Reset()
        {
            base.Reset();
            
            _characterController = GetComponent<CharacterController>();
            _navMeshAgent = GetComponent<NavMeshAgent>();
        }

        private void OnValidate()
        {
            if(Owner != null)
                _maxMovementSpeed?.UpdateValue(Owner);
            else
                _maxMovementSpeed?.UpdateValue();
            
            HasCharacterController = _characterController != null;
            HasNavMeshAgent = _navMeshAgent != null;

            SyncAgentProperties();
        }
        
        private void OnEnable()
        {
            MovementInput = Vector2.zero;
            _limitSpeed = true;

            if (HasNavMeshAgent)
                NavMeshAgent.enabled = true;
        }

        private void OnDisable()
        {
            if (HasNavMeshAgent)
                NavMeshAgent.enabled = false;
        }

        private new void Awake()
        {
            base.Awake();
            
            Setup();
        }

        public virtual void Setup()
        {
            HasCharacterController = _characterController != null;
            HasNavMeshAgent = _navMeshAgent != null;

            if (HasNavMeshAgent)
            {
                _navMeshAgent.updatePosition = true;
                _navMeshAgent.updateRotation = false;
            }

            _animationParams.CreateHashes();
            _groundDetectResults = new Collider[32];
            _speed = 0;
            _lastPosition = transform.position;
            
            LookInput = InitialLookDirection();
            MovementInput = Vector2.zero;
            Velocity = Vector3.zero;

            if(HasCharacterController && CharacterController.enabled)
                _characterController.SimpleMove(Vector3.zero);
            
            SyncAgentProperties();
        }
        
        private void SyncAgentProperties()
        {
            if (!HasNavMeshAgent) return;
            
            _navMeshAgent.speed = _maxMovementSpeed.Value;
        }
        
        protected override void OnStatsUpdated()
        {
            _maxMovementSpeed.UpdateValue(this);
            
            SyncAgentProperties();
        }

        protected virtual float GetRotationSmoothTime()
        {
            return _movementSettings.RotationSmoothTime * Time.deltaTime;
        }

        /// <summary>
        /// Execute a step in the Actor's movement.
        /// </summary>
        protected virtual void MoveStep()
        {
            var movement = Vector3.zero;
            if (HasNavMeshAgent) return;

            if(CanMove)
            {
                movement = new Vector3(MovementInput.x, 0f, MovementInput.y).normalized * 
                           (_speed * Time.deltaTime);
            }

            CCMove(movement);
        }
        
        /// <summary>
        /// Execute a rotation step in the Actor's movement.
        /// </summary>
        protected virtual void RotateStep()
        {
            if (Time.deltaTime <= 0f) return;
            if (!CanRotate) return;
            
            if (HasNavMeshAgent)
            {
                if (!_navMeshAgent.updateRotation && _navMeshAgent.desiredVelocity.sqrMagnitude > 0.2f)
                {
                    LookInput = new Vector3(_navMeshAgent.desiredVelocity.x, 0f, _navMeshAgent.desiredVelocity.z).normalized;
                }
            }

            var transformY = _rotationY;
            
            var target = Mathf.Atan2(LookInput.x, LookInput.z) * Mathf.Rad2Deg;
            
            var rotationY = Mathf.SmoothDampAngle(transformY.rotation.eulerAngles.y, target,
                ref rotationVelocityY, GetRotationSmoothTime());
            
            transformY.rotation = Quaternion.Euler(0.0f, rotationY, 0.0f);
        }
        
        private void GravityStep()
        {
            if (!Settings.UseGravity) return;
            if (HasNavMeshAgent) return;
            
            if (!IsGrounded)
            {
                verticalVelocity += Physics.gravity.y * Settings.GravityMultiplier * Time.deltaTime;
            }
            else
            {
                verticalVelocity = -1f;
            }
            
            // Limit by terminal velocity
            verticalVelocity = Mathf.Clamp(verticalVelocity, -Settings.TerminalVelocity, 
                Settings.TerminalVelocity);

            var move = new Vector3(0, verticalVelocity, 0) * Time.deltaTime;

            CCMove(move);
        }

        private void CCMove(Vector3 movement)
        {
            if (HasCharacterController)
            {
                if(_characterController.enabled)
                    _characterController.Move(movement);
            }
            else
            {
                Owner.transform.Translate(movement);
            }
        }
        

        protected void Update()
        {
            UpdateSpeed();
            GravityStep();
            MoveStep();
            RotateStep();
            
            UpdateMovementAnimator(_speed);
        }

        private void FixedUpdate()
        {
            // SphereCast at position to determine if we are grounded.
            IsGrounded = Physics.OverlapSphereNonAlloc(transform.position, 0.2f, _groundDetectResults, _movementSettings.WhatIsGround) > 0;
        }

        /// <summary>
        /// Calculate's current Velocity and Speed of the Actor taking into account it's movement medium.
        /// </summary>
        protected virtual void UpdateSpeed() // replaced by MovementSystem
        {
            if (HasNavMeshAgent)
            {
                Velocity = _navMeshAgent.velocity;
            } else if (HasCharacterController)
            {
                Velocity = _characterController.velocity;
            }
            else
            {
                Velocity = (transform.position - _lastPosition) / Time.deltaTime;
            }
            
            _lastPosition = transform.position;
            var currentSpeed = Velocity.magnitude;

            if (currentSpeed > _maxMovementSpeed.Value)
                currentSpeed = _maxMovementSpeed.Value;
            
            // const float speedOffset = 0.1f;
            
            var targetValue = _maxMovementSpeed.Value * MovementInput.magnitude;
            
            _speed = Mathf.Lerp(currentSpeed, targetValue, Time.deltaTime * _movementSettings.MovementAccel);
            _speed = Mathf.Round(_speed * 1000) / 1000;
            _speed = Mathf.Clamp(_speed, 0f, _maxMovementSpeed.Value);
        }
        
        /// <summary>
        /// Execute's an update step in the Actor's animator movement parameters.
        /// </summary>
        /// <param name="currentSpeed">Speed to calculate parameters from</param>
        protected virtual void UpdateMovementAnimator(float currentSpeed)
        {
            if (!Owner.HasAnimator) return;
            
            var forwardDot = Vector3.Dot(Velocity.normalized, transform.forward);
            var rightDot = Vector3.Dot(Velocity.normalized, transform.right);
            
            if(_animationParams.NormalizeBlend)
            {
                // normalize the blend values
                walkRunAnimBlend = Mathf.Lerp(-1f, 1f, 0.5f + (currentSpeed * 0.5f / _maxMovementSpeed.Value) * forwardDot);
                strafeAnimBlend = Mathf.Lerp(-1f, 1f, 0.5f + (currentSpeed * 0.5f / _maxMovementSpeed.Value) * rightDot);
            }
            else
            {
                walkRunAnimBlend = Mathf.Lerp(walkRunAnimBlend, currentSpeed * forwardDot, Time.deltaTime * _movementSettings.MovementAccel);
                strafeAnimBlend = Mathf.Lerp(strafeAnimBlend, currentSpeed * rightDot, Time.deltaTime * _movementSettings.MovementAccel);
            }

            // update animator if using character
            if(_animationParams.animID_RunBlend != -1)
                Owner.Animator.SetFloat(_animationParams.animID_RunBlend, walkRunAnimBlend);
            
            if(_animationParams.animID_StrafeBlend != -1)
                Owner.Animator.SetFloat(_animationParams.animID_StrafeBlend, strafeAnimBlend);
            
            if(_animationParams.animID_MotionSpeed != -1)
                Owner.Animator.SetFloat(_animationParams.animID_MotionSpeed, 1f);
        }

        /// <summary>
        /// Helper function to set the LookDirection vector to look to target.
        /// </summary>
        /// <param name="target"></param>
        public virtual void SetLookDirectionTarget(Vector3 target)
        {
            LookInput = DirectionToPosition(target);
        }
        
        /// <summary>
        /// Instantly rotates Actor to direction.
        /// </summary>
        /// <param name="dir">Direction to rotate.</param>
        public void InstantLookDir(Vector3 dir)
        {
            var target = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;

            LookInput = dir;
            
            transform.rotation = Quaternion.Euler(0.0f, target, 0.0f);
        }

        /// <summary>
        /// Helper function that returns the direction from this Actor's position to another position.
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        protected Vector3 DirectionToPosition(Vector3 pos)
        {
            var position = transform.position;
            
            var relativePos = new Vector3(pos.x, position.y, pos.z);
            
            return (relativePos - position).normalized;
        }
        
        protected virtual Vector3 InitialLookDirection() => transform.forward;
        
        public virtual void Stop()
        {
            if(HasNavMeshAgent)
                _navMeshAgent.isStopped = true;
            
            MovementInput = Vector2.zero;
        }
        
        public void Teleport(Vector3 position)
        {
            if (HasCharacterController)
                _characterController.enabled = false;

            if (HasNavMeshAgent)
                _navMeshAgent.Warp(position);
            else
                transform.position = position;
            
            
            if (HasCharacterController)
                _characterController.enabled = true;
        }
        
        public void SetDestination(Vector3 position)
        {
            if (!HasNavMeshAgent) return;
            
            _navMeshAgent.isStopped = false;
            _navMeshAgent.SetDestination(position);
        }
    }
}