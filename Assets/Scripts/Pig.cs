using System.Collections;
using UnityEngine;

namespace BoxCorp
{
    public class Pig : MonoBehaviour
    {
        private enum State : byte
        {
            Idle = 0,
            Walking = 1,
            Running = 2,
            Grabbed = 3,
            Attacking = 4,
            Disarmed = 5,
        }

        private static readonly int ANIM_IDLE = Animator.StringToHash("idle");
        private static readonly int ANIM_WALK = Animator.StringToHash("walk");
        private static readonly int ANIM_RUN = Animator.StringToHash("run");
        private static readonly int ANIM_DANCE = Animator.StringToHash("dance");

        [Header("References")]
        [SerializeField] private PhysicsGrabbable _grabbable;
        [SerializeField] private Animator _animator;
        [SerializeField] private Collider _collider;
        [SerializeField] private Turret _turret;

        [Header("Movement")]
        [SerializeField] private float _runSpeed = 6f;
        [SerializeField] private float _arriveDistance = 0.5f;
        [SerializeField] private float _turnSpeed = 180f;
        [SerializeField] private float _walkSpeedThreshold = 2f;
        [SerializeField] private float _wanderTurnInterval = 2f;

        [Header("Attacking")]
        [SerializeField] private float _aimThreshold = 3f;
        [SerializeField] private float _fireInterval = 0.6f;
        [SerializeField] private float _attackDelay = 1f;

        private Transform _runToTarget;
        private Camera _mainCamera;
        private Vector3 _wanderDirection;
        private State _state;
        private float _nextWanderTime;
        private float _nextFireTime;
        private float _turretYawOffset;
        private int _movingAnimation;
        private bool _engaged;

        private void Awake()
        {
            MountTurret();
            _grabbable.OnGrabbedChanged += HandlePigGrabbedChanged;
            _turret.Grabbable.OnGrabbedChanged += HandleTurretGrabbedChanged;
        }

        private void Start()
        {
            _mainCamera = Camera.main;
        }

        private void OnDestroy()
        {
            _grabbable.OnGrabbedChanged -= HandlePigGrabbedChanged;
            _turret.Grabbable.OnGrabbedChanged -= HandleTurretGrabbedChanged;
        }

        private void FixedUpdate()
        {
            switch (_state)
            {
                case State.Running:
                    UpdateRunning();
                    break;
                case State.Attacking:
                    UpdateAttacking();
                    break;
                case State.Disarmed:
                    UpdateDisarmed();
                    break;
            }
        }

        // Bounce off walls but plow straight through physics objects.
        private void OnCollisionEnter(Collision collision)
        {
            if (_state != State.Disarmed
                || (collision.gameObject.TryGetComponent(out PhysicsGrabbable grabbable) 
                && grabbable.GrabbableType != GrabbableType.Pig))
            {
                return;
            }

            Vector3 normal = collision.GetContact(0).normal;
            if (Mathf.Abs(normal.y) < 0.5f)
            {
                normal.y = 0f;
                _wanderDirection = Vector3.Reflect(_wanderDirection, normal.normalized);
                _nextWanderTime = Time.time + _wanderTurnInterval;
            }
        }

        public void Setup(Transform runToTarget)
        {
            _runToTarget = runToTarget;
        }

        [ContextMenu(nameof(BeginEncounter))]
        public void BeginEncounter()
        {
            _engaged = true;

            if (_state == State.Idle)
            {
                SetState(State.Running);
            }
        }

        // Pooled pigs skip the run-in and attack after a short delay.
        public void BeginAttack()
        {
            _turret.Attach();
            SetTurretCollisionIgnored(true);
            _engaged = true;
            SetState(State.Idle);
            StartCoroutine(DelayedAttackCoroutine());
            _turret.Collider.enabled = false;
        }

        private IEnumerator DelayedAttackCoroutine()
        {
            yield return new WaitForSeconds(_attackDelay);

            _turret.Collider.enabled = true;
            if (_state == State.Idle)
            {
                SetState(State.Attacking);
            }
        }

        private void MountTurret()
        {
            Vector3 pigForward = transform.forward;
            Vector3 turretForward = _turret.transform.forward;
            pigForward.y = 0f;
            turretForward.y = 0f;
            _turretYawOffset = Vector3.SignedAngle(pigForward, turretForward, Vector3.up);

            SetTurretCollisionIgnored(true);
        }

        // While attached, ignore collision between pig and turret.
        private void SetTurretCollisionIgnored(bool ignored)
        {
            Physics.IgnoreCollision(_collider, _turret.Collider, ignored);
        }

        private void UpdateRunning()
        {
            Vector3 toTarget = _runToTarget.position - transform.position;
            toTarget.y = 0f;

            if (toTarget.magnitude < _arriveDistance)
            {
                StopHorizontal();
                SetState(State.Attacking);
                return;
            }

            MoveToward(toTarget.normalized);
        }

        private void UpdateAttacking()
        {
            StopHorizontal();

            Vector3 toCam = _mainCamera.transform.position - transform.position;
            toCam.y = 0f;
            float targetYaw = Mathf.Atan2(toCam.x, toCam.z) * Mathf.Rad2Deg - _turretYawOffset;

            Rigidbody rb = _grabbable.Rigidbody;
            rb.angularVelocity = Vector3.zero;
            rb.MoveRotation(Quaternion.RotateTowards(
                rb.rotation, Quaternion.Euler(0f, targetYaw, 0f), _turnSpeed * Time.fixedDeltaTime));

            _turret.AimAt(_mainCamera.transform.position);

            if (Mathf.Abs(Mathf.DeltaAngle(rb.rotation.eulerAngles.y, targetYaw)) < _aimThreshold
                && Time.time >= _nextFireTime)
            {
                _turret.Fire();
                _nextFireTime = Time.time + _fireInterval;
            }
        }

        private void UpdateDisarmed()
        {
            if (Time.time >= _nextWanderTime)
            {
                PickWanderDirection();
            }

            MoveToward(_wanderDirection);
        }

        private void MoveToward(Vector3 direction)
        {
            Rigidbody rb = _grabbable.Rigidbody;

            // Walk animation when moving slowly, run otherwise.
            float speed = new Vector2(rb.linearVelocity.x, rb.linearVelocity.z).magnitude;
            SetMovingAnimation(speed < _walkSpeedThreshold ? ANIM_WALK : ANIM_RUN);

            rb.linearVelocity = new Vector3(direction.x * _runSpeed, rb.linearVelocity.y, direction.z * _runSpeed);

            if (direction.sqrMagnitude > 0.001f)
            {
                rb.angularVelocity = Vector3.zero;
                Quaternion look = Quaternion.LookRotation(new Vector3(direction.x, 0f, direction.z));
                rb.MoveRotation(Quaternion.RotateTowards(rb.rotation, look, _turnSpeed * Time.fixedDeltaTime));
            }
        }

        private void StopHorizontal()
        {
            Rigidbody rb = _grabbable.Rigidbody;
            rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
        }

        private void PickWanderDirection()
        {
            float angle = Random.Range(0f, Mathf.PI * 2f);
            _wanderDirection = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
            _nextWanderTime = Time.time + _wanderTurnInterval;
        }

        private void HandlePigGrabbedChanged(bool isGrabbed)
        {
            if (isGrabbed)
            {
                SetState(State.Grabbed);
            }
            else if (!_engaged)
            {
                SetState(State.Idle);
            }
            else
            {
                SetState(_turret.IsAttached ? State.Attacking : State.Disarmed);
            }
        }

        private void HandleTurretGrabbedChanged(bool isGrabbed)
        {
            if (isGrabbed && _state != State.Disarmed)
            {
                _engaged = true;
                SetTurretCollisionIgnored(false);
                SetState(State.Disarmed);
            }
        }

        private void SetState(State state)
        {
            _state = state;

            switch (state)
            {
                case State.Idle:
                    _animator.Play(ANIM_IDLE);
                    _animator.speed = 1f;
                    _movingAnimation = 0;
                    break;
                case State.Running:
                    SetMovingAnimation(ANIM_RUN);
                    break;
                case State.Attacking:
                    _animator.Play(ANIM_IDLE);
                    _animator.speed = 1f;
                    _movingAnimation = 0;
                    _nextFireTime = Time.time + _fireInterval;
                    break;
                case State.Grabbed:
                    _animator.Play(ANIM_DANCE);
                    _animator.speed = 2f;
                    _movingAnimation = 0;
                    break;
                case State.Disarmed:
                    SetMovingAnimation(ANIM_RUN);
                    PickWanderDirection();
                    break;
            }
        }

        private void SetMovingAnimation(int anim)
        {
            if (_movingAnimation != anim)
            {
                _movingAnimation = anim;
                _animator.Play(anim);
                _animator.speed = 1f;
            }
        }
    }
}
