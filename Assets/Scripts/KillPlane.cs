using UnityEngine;

namespace BoxCorp
{
    [RequireComponent(typeof(Collider))]
    public class KillPlane : MonoBehaviour
    {
        private void Reset()
        {
            GetComponent<Collider>().isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            Rigidbody body = other.attachedRigidbody;

            if (body != null && body.TryGetComponent(out PhysicsGrabbable grabbable))
            {
                // Prevent soft lock from pig encounter falling out of bounds.
                if (grabbable.GrabbableType == GrabbableType.Pig)
                {
                    GameManager.Instance.PigDefeated();
                }

                grabbable.Recycle();
            }
        }
    }
}
