using UnityEngine;

namespace BoxCorp
{
    public class BoxSpawner : MonoBehaviour
    {
        [SerializeField] private GrabbablePool pool;
        [SerializeField] private float spawnForceMin = 5f;
        [SerializeField] private float spawnForceMax = 10f;

        [ContextMenu("Spawn Box")]
        public void SpawnBox()
        {
            var grabbable = pool.SpawnGrabbable(transform.position, transform.rotation);
            grabbable.Rb.AddForce(transform.forward * Random.Range(spawnForceMin, spawnForceMax), ForceMode.Impulse);
        }
    }
}