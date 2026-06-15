using UnityEngine;

namespace BoxCorp
{
    public class Turret : MonoBehaviour
    {
        [SerializeField] private PhysicsGrabbable _grabbable;
        [SerializeField] private Transform _barrel;
        [SerializeField] private Collider _collider;
        [SerializeField] private ParticleSystem _shootParticles;
        [SerializeField] private AudioClip[] _shootSounds;
        [SerializeField] private float _damagePerShot = 10f;
        [SerializeField] private float _maxBarrelPitch = 60f;

        private Transform _attachParent;
        private Vector3 _attachLocalPosition;
        private Quaternion _attachLocalRotation;
        private Vector3 _attachLocalScale;
        private bool _isAttached;

        public PhysicsGrabbable Grabbable => _grabbable;
        public Collider Collider => _collider;
        public bool IsAttached => _isAttached;

        private void Awake()
        {
            _attachParent = transform.parent;
            transform.GetLocalPositionAndRotation(out _attachLocalPosition, out _attachLocalRotation);
            _attachLocalScale = transform.localScale;
            Attach();
            _grabbable.OnGrabbedChanged += HandleGrabbedChanged;
            _grabbable.OnBoosted += HandleBoosted;
        }


        private void OnDestroy()
        {
            _grabbable.OnGrabbedChanged -= HandleGrabbedChanged;
            _grabbable.OnBoosted -= HandleBoosted;
        }

        // Mount the turret onto its pig.
        public void Attach()
        {
            gameObject.SetActive(true);
            transform.SetParent(_attachParent, false);
            transform.SetLocalPositionAndRotation(_attachLocalPosition, _attachLocalRotation);
            transform.localScale = _attachLocalScale;
            _grabbable.Rigidbody.isKinematic = true;
            _grabbable.Rigidbody.interpolation = RigidbodyInterpolation.None;
            _grabbable.SetCanGrab(true);
            _isAttached = true;
        }

        public void AimAt(Vector3 target)
        {
            Vector3 dir = _barrel.parent.InverseTransformDirection(target - _barrel.position);
            float pitch = -Mathf.Atan2(dir.y, new Vector2(dir.x, dir.z).magnitude) * Mathf.Rad2Deg;
            pitch = Mathf.Clamp(pitch, -_maxBarrelPitch, _maxBarrelPitch);
            _barrel.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        }

        public void Fire()
        {
            if (!_isAttached)
            {
                return;
            }

            if (_shootParticles != null)
            {
                _shootParticles.Play();
            }

            PlayerHealth.Instance.TakeDamage(_damagePerShot);

            if (_shootSounds.Length > 0)
            {
                AudioClip sound = _shootSounds[Random.Range(0, _shootSounds.Length)];
                if (sound == null)
                {
                    return;
                }
                AudioManager.Instance.PlaySFX(sound, transform.position, 0.7f, Random.Range(0.8f, 1.2f));
            }
        }

        private void HandleGrabbedChanged(bool grabbed)
        {
            if (grabbed && _isAttached)
            {
                _isAttached = false;
                transform.SetParent(null, true);
                _grabbable.Rigidbody.isKinematic = false;
                _grabbable.Rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
            }
        }

        private void HandleBoosted()
        {
            _grabbable.Grabbed(null);
            _grabbable.Released(Vector3.zero, Vector3.zero);
        }
    }
}
