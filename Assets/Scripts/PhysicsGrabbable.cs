using UnityEngine;

namespace BoxCorp
{
    public enum GrabbableType : byte
    {
        Box = 0,
        DirtyBox = 1,
        Prop = 2,
        Pig = 3,
        Turret = 4,
    }

    [RequireComponent(typeof(Rigidbody))]
    public class PhysicsGrabbable : MonoBehaviour
    {
        [Header("Grabbable Settings")]
        [SerializeField] private Rigidbody _rb;
        [SerializeField] private GrabbableType _grabbableType = GrabbableType.Box;
        [SerializeField] private float _gravityMultiplier = 2f;
        [SerializeField] private float _grabbedDamping = 10f;
        [SerializeField] private float _ungrabbedDamping = 1f;

        [Header("Weighted")]
        [SerializeField] private Vector3 _centerOfMassOffset = Vector3.zero;
        [SerializeField] private float _selfRightingStrength = 0f;
        [SerializeField] private float _selfRightingDamping = 0.5f;

        [Header("Contact Particles")]
        [SerializeField] private float _forceThreshold = 5f;
        [SerializeField] private float _particlesInterval = 0.5f;

        [Header("Dirt")]
        [SerializeField] private GameObject _dirt;
        [SerializeField] private AudioClip _dirtRemovedSound;
        [SerializeField] private float _dirtShakeThreshold = 30f;
        [SerializeField] private float _dirtKnockOffForce = 25f;

        [Header("General Audio")]
        [SerializeField] private AudioClip _grabSound;
        [SerializeField] private AudioClip _boostSound;
        [SerializeField] private AudioClip[] _collisionSounds;
        [SerializeField] private float _collisionSoundInterval = 0.2f;

        [Header("Spin Audio")]
        [SerializeField] private AudioClip _spinSound;
        [SerializeField] private float _spinSoundMinPitch = 0.5f;
        [SerializeField] private float _spinSoundMaxPitch = 1.5f;
        [SerializeField] private float _spinSoundVolume = 0.15f;

        private GrabbablePool _pool;
        private Grabber _grabber;
        private GrabbableType _baseType;
        private Vector3 _lastVelocity;
        private float _lastImpactTime = 0f;
        private float _lastSoundTime = 0f;
        private float _lastSpinSoundTime = 0f;
        private bool _isGrabbed;
        private bool _canGrab = true;

        public System.Action<bool> OnGrabbedChanged;
        public System.Action OnBoosted;

        public Rigidbody Rigidbody => _rb;
        public GrabbableType GrabbableType => _grabbableType;
        public bool IsGrabbed => _isGrabbed;
        public bool CanGrab => _canGrab;

        private void Awake()
        {
            _baseType = _grabbableType;

            if (_centerOfMassOffset != Vector3.zero)
            {
                _rb.centerOfMass = _centerOfMassOffset;
            }
        }

        private void FixedUpdate()
        {
            // Add extra gravity.
            _rb.AddForce(Physics.gravity * (_gravityMultiplier - 1f), ForceMode.Acceleration);

            // Bottom-heavy objects swing upright while held.
            if (_isGrabbed && _selfRightingStrength > 0f)
            {
                float tilt = Vector3.SignedAngle(transform.up, Vector3.up, Vector3.forward);
                float torqueZ = tilt * _selfRightingStrength - _rb.angularVelocity.z * _selfRightingDamping;
                _rb.AddTorque(0f, 0f, torqueZ, ForceMode.Acceleration);
            }

            // Shake the dirt off with a sharp change in velocity.
            if (_grabbableType == GrabbableType.DirtyBox && _isGrabbed)
            {
                float jerk = (_rb.linearVelocity - _lastVelocity).magnitude;
                if (jerk > _dirtShakeThreshold)
                {
                    CleanDirt();
                }
            }
            _lastVelocity = _rb.linearVelocity;

            UpdateSpinAudio();
        }

        private void UpdateSpinAudio()
        {
            const float SPIN_SOUND_MIN_ANGULAR_SPEED = 12f;
            const float SPIN_SOUND_MAX_ANGULAR_SPEED = 20f;
            const float SPIN_SOUND_SLOW_INTERVAL = 0.5f;
            const float SPIN_SOUND_FAST_INTERVAL = 0.175f;
            const float SPIN_SOUND_PITCH_OFFSET = 0.125f;
            const float SPIN_SOUND_VOLUME_FLOOR = 0.4f;

            if (!_isGrabbed || _spinSound == null)
            {
                return;
            }

            float angularSpeed = _rb.angularVelocity.magnitude;
            if (angularSpeed < SPIN_SOUND_MIN_ANGULAR_SPEED)
            {
                return;
            }

            float t = Mathf.Clamp01(Mathf.InverseLerp(
                SPIN_SOUND_MIN_ANGULAR_SPEED, SPIN_SOUND_MAX_ANGULAR_SPEED, angularSpeed));

            float interval = Mathf.Lerp(SPIN_SOUND_SLOW_INTERVAL, SPIN_SOUND_FAST_INTERVAL, t);
            if (Time.time - _lastSpinSoundTime < interval)
            {
                return;
            }
            _lastSpinSoundTime = Time.time;

            float pitch = Mathf.Lerp(_spinSoundMinPitch, _spinSoundMaxPitch, t) 
                + Random.Range(-SPIN_SOUND_PITCH_OFFSET, SPIN_SOUND_PITCH_OFFSET);
            float volume = _spinSoundVolume * Mathf.Lerp(SPIN_SOUND_VOLUME_FLOOR, 1f, t);
            AudioManager.Instance.PlaySFX(_spinSound, transform.position, volume, pitch);
        }

        private void OnCollisionEnter(Collision collision)
        {
            float velocityMagnitude = collision.relativeVelocity.magnitude;

            // A hard enough throw or impact knocks the dirt loose.
            if (_grabbableType == GrabbableType.DirtyBox && velocityMagnitude > _dirtKnockOffForce)
            {
                CleanDirt();
            }

            if (velocityMagnitude > _forceThreshold && _lastImpactTime < Time.time - _particlesInterval)
            {
                SpawnDustParticles(collision.GetContact(0).point);
                _lastImpactTime = Time.time;
            }

            if (_lastSoundTime < Time.time - _collisionSoundInterval && _collisionSounds.Length > 0)
            {
                float volume = Mathf.Min(velocityMagnitude * 0.15f, 1f) * 0.5f;
                if (volume < 0.1f)
                {
                    return;
                }

                AudioClip clip = _collisionSounds[Random.Range(0, _collisionSounds.Length)];
                if (clip == null)
                {
                    return;
                }

                _lastSoundTime = Time.time;
                AudioManager.Instance.PlaySFX(clip, transform.position, volume, Random.Range(0.7f, 1.1f));
            }
        }

        public void Grabbed(Grabber grabber)
        {
            _grabber = grabber;
            _isGrabbed = true;
            _rb.useGravity = true;
            _rb.linearDamping = _grabbedDamping;

            if (_grabSound != null)
            {
                AudioManager.Instance.PlaySFX(_grabSound, transform.position, 1f, Random.Range(0.85f, 1.15f));
            }

            OnGrabbedChanged?.Invoke(true);
        }

        public void Released(Vector3 releaseVector, Vector3 angularVelocity)
        {
            _grabber = null;
            _isGrabbed = false;
            _rb.useGravity = true;
            _rb.linearDamping = _ungrabbedDamping;
            _rb.linearVelocity = releaseVector;
            _rb.angularVelocity = angularVelocity;

            OnGrabbedChanged?.Invoke(false);
        }

        public void Spawned()
        {
            _rb.linearVelocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
            _lastVelocity = Vector3.zero;
            _grabber = null;
            _isGrabbed = false;
            _canGrab = true;
            _grabbableType = _baseType;

            if (_dirt != null)
            {
                _dirt.SetActive(false);
            }
        }

        public void SetPool(GrabbablePool pool)
        {
            _pool = pool;
        }

        public void InHopper()
        {
            Recycle();
        }

        public void Recycle()
        {
            if (_isGrabbed)
            {
                SetCanGrab(false);
            }

            if (_pool == null)
            {
                gameObject.SetActive(false);
                return;
            }

            _pool.ReturnGrabbable(this);
        }

        public void SetCanGrab(bool canGrab)
        {
            _canGrab = canGrab;

            if (!canGrab && _grabber != null)
            {
                _grabber.LetGo();
                _grabber = null;
                _isGrabbed = false;

                OnGrabbedChanged?.Invoke(false);
            }
        }

        [ContextMenu(nameof(MakeDirty))]
        public void MakeDirty()
        {
            _grabbableType = GrabbableType.DirtyBox;
            _lastVelocity = _rb.linearVelocity;

            if (_dirt != null)
            {
                _dirt.SetActive(true);
            }
        }

        [ContextMenu(nameof(CleanDirt))]
        private void CleanDirt()
        {
            _grabbableType = GrabbableType.Box;

            if (_dirt != null)
            {
                _dirt.SetActive(false);
            }

            SpawnDirtParticles(transform.position);

            if (_dirtRemovedSound != null)
            {
                AudioManager.Instance.PlaySFX(_dirtRemovedSound, transform.position, 1f, Random.Range(0.9f, 1.1f));
            }
        }

        public void Boosted()
        {
            SpawnSmallDustParticles(transform.position);

            if (_grabSound != null)
            {
                AudioManager.Instance.PlaySFX(_grabSound, transform.position, 0.4f, Random.Range(0.85f, 1.15f));
            }

            if (_boostSound != null)
            {
                AudioManager.Instance.PlaySFX(_boostSound, transform.position, 0.8f, Random.Range(0.8f, 1.2f));
            }

            OnBoosted?.Invoke();
        }

        private void SpawnDustParticles(Vector3 position)
        {
            if (ParticlePool.Instance != null)
            {
                ParticlePool.Instance.SpawnDust(position);
            }
        }

        private void SpawnSmallDustParticles(Vector3 position)
        {
            if (ParticlePool.Instance != null)
            {
                ParticlePool.Instance.SpawnSmallDust(position);
            }
        }

        private void SpawnDirtParticles(Vector3 position)
        {
            if (ParticlePool.Instance != null)
            {
                ParticlePool.Instance.SpawnBoxDirt(position);
            }
        }
    }
}
