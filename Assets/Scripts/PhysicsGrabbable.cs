using UnityEngine;
using UnityEngine.UIElements;

namespace BoxCorp
{
    public enum GrabbableType
    {
        Box,
        Prop
    }

    [RequireComponent(typeof(Rigidbody))]
    public class PhysicsGrabbable : MonoBehaviour
    {
        [Header("Grabbable Settings")]
        [SerializeField] private GrabbableType grabbableType = GrabbableType.Box;
        [SerializeField] private float gravityMultiplier = 2f;
        [SerializeField] private float grabbedDamping = 10f;
        [SerializeField] private float ungrabbledDamping = 1f;

        [Header("Contact Particles")]
        [SerializeField] private GameObject dustParticlesPrefab;
        [SerializeField] private GameObject smallDustParticlesPrefab;
        [SerializeField] private float forceThreshold = 5f;
        [SerializeField] private float particlesInterval = 0.5f;

        public Rigidbody Rb { get; private set; }
        public bool IsGrabbed { get; private set; } = false;
        public bool CanGrab { get; private set; } = true;

        private GrabbablePool pool;
        private Grabber grabber;
        private float lastImpactTime = 0f;

        public void Grabbed(Grabber grabber)
        {
            this.grabber = grabber;
            IsGrabbed = true;
            Rb.useGravity = true;
            Rb.linearDamping = grabbedDamping;
        }

        public void Released(Vector3 releaseVector, Vector3 angularVelocity)
        {
            grabber = null;
            IsGrabbed = false;
            Rb.useGravity = true;
            Rb.linearDamping = ungrabbledDamping;
            Rb.linearVelocity = releaseVector;
            Rb.angularVelocity = angularVelocity;
        }

        public void OnSpawned()
        {
            Rb.linearVelocity = Vector3.zero;
            Rb.angularVelocity = Vector3.zero;
            IsGrabbed = false;
            CanGrab = true;
        }

        public void SetPool(GrabbablePool pool)
        {
            this.pool = pool;
        }

        public void InHopper()
        {
            if (pool == null || grabbableType != GrabbableType.Box)
            {
                Destroy(gameObject);
                return;
            }

            pool.ReturnGrabbable(this);
        }

        public void SetCanGrab(bool canGrab)
        {
            CanGrab = canGrab;

            if (!canGrab && grabber != null)
            {
                grabber.LetGo();
            }
        }

        public GrabbableType GetGrabbableType()
        {
            return grabbableType;
        }

        public void Boosted()
        {
            SpawnSmallDustParticles(transform.position);
        }

        private void Awake()
        {
            Rb = GetComponent<Rigidbody>();
        }

        private void FixedUpdate()
        {
            // Add extra gravity
            Rb.AddForce(Physics.gravity * (gravityMultiplier - 1f), ForceMode.Acceleration);
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.relativeVelocity.magnitude > forceThreshold
                && lastImpactTime < Time.time - particlesInterval)
            {
                SpawnDustParticles(collision.GetContact(0).point);
                lastImpactTime = Time.time;
            }
        }

        private void SpawnDustParticles(Vector3 position)
        {
            if (dustParticlesPrefab != null)
            {
                Instantiate(dustParticlesPrefab, position, Quaternion.identity);
            }
        }

        private void SpawnSmallDustParticles(Vector3 position)
        {
            if (smallDustParticlesPrefab != null)
            {
                Instantiate(smallDustParticlesPrefab, position, Quaternion.identity);
            }
        }
    }
}