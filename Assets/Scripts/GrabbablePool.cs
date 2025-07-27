using System.Collections.Generic;
using UnityEngine;

namespace BoxCorp
{
    public class GrabbablePool : MonoBehaviour
    {
        [SerializeField] private PhysicsGrabbable boxPrefab;
        [SerializeField] private int initialPoolSize = 10;

        private readonly Queue<PhysicsGrabbable> boxPool = new();

        private void Awake()
        {
            // Pre-warm box pool
            for (int i = 0; i < initialPoolSize; i++)
            {
                CreateNewGrabbable();
            }
        }

        public PhysicsGrabbable SpawnGrabbable(Vector3 position, Quaternion rotation)
        {
            PhysicsGrabbable grabbable = (boxPool.Count > 0) ? boxPool.Dequeue() : CreateNewGrabbable();

            grabbable.transform.SetParent(null, true);
            grabbable.transform.SetPositionAndRotation(position, rotation);
            grabbable.gameObject.SetActive(true);
            grabbable.OnSpawned();
            return grabbable;
        }

        public void ReturnGrabbable(PhysicsGrabbable cube)
        {
            cube.gameObject.SetActive(false);
            boxPool.Enqueue(cube);
        }

        private PhysicsGrabbable CreateNewGrabbable()
        {
            PhysicsGrabbable newCube = Instantiate(boxPrefab, transform);
            newCube.gameObject.SetActive(false);
            newCube.SetPool(this);
            boxPool.Enqueue(newCube);
            return newCube;
        }
    }
}