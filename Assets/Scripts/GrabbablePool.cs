using System.Collections.Generic;
using UnityEngine;

namespace BoxCorp
{
    public class GrabbablePool : MonoBehaviour
    {
        [Header("Boxes")]
        [SerializeField] private PhysicsGrabbable _boxPrefab;
        [SerializeField] private int _initialPoolSize = 16;
        [SerializeField] private int _maxActiveBoxes = 32;
        private readonly Queue<PhysicsGrabbable> _boxPool = new();
        private readonly List<PhysicsGrabbable> _activeBoxes = new();

        [Header("Bowling Balls")]
        [SerializeField] private PhysicsGrabbable _bowlingBallPrefab;
        [SerializeField] private int _bowlingBallPoolSize = 1;
        private readonly Queue<PhysicsGrabbable> _bowlingBallPool = new();
        private readonly List<PhysicsGrabbable> _activeBowlingBalls = new();

        [Header("Pigs")]
        [SerializeField] private PhysicsGrabbable _pigPrefab;
        [SerializeField] private int _pigPoolSize = 3;
        private readonly Queue<PhysicsGrabbable> _pigPool = new();
        private readonly List<PhysicsGrabbable> _activePigs = new();

        private void Awake()
        {
            Prewarm(_boxPrefab, _boxPool, _initialPoolSize);
            Prewarm(_bowlingBallPrefab, _bowlingBallPool, _bowlingBallPoolSize);
            Prewarm(_pigPrefab, _pigPool, _pigPoolSize);
        }

        public PhysicsGrabbable SpawnBox(Vector3 position, Quaternion rotation)
        {
            if (_activeBoxes.Count >= _maxActiveBoxes)
            {
                PhysicsGrabbable toEvict = _activeBoxes[0];
                for (int i = 0; i < _activeBoxes.Count; i++)
                {
                    if (!_activeBoxes[i].IsGrabbed)
                    {
                        toEvict = _activeBoxes[i];
                        break;
                    }
                }
                toEvict.Recycle();
            }

            PhysicsGrabbable grabbable = (_boxPool.Count > 0) ? _boxPool.Dequeue() : CreateNew(_boxPrefab);
            Activate(grabbable, position, rotation);
            _activeBoxes.Add(grabbable);
            return grabbable;
        }

        public PhysicsGrabbable SpawnBowlingBall(Vector3 position, Quaternion rotation)
        {
            if (_bowlingBallPool.Count == 0)
            {
                return null;
            }

            PhysicsGrabbable grabbable = _bowlingBallPool.Dequeue();
            Activate(grabbable, position, rotation);
            _activeBowlingBalls.Add(grabbable);
            return grabbable;
        }

        public PhysicsGrabbable SpawnPig(Vector3 position, Quaternion rotation)
        {
            if (_pigPool.Count == 0)
            {
                return null;
            }

            PhysicsGrabbable grabbable = _pigPool.Dequeue();
            Activate(grabbable, position, rotation);
            _activePigs.Add(grabbable);
            return grabbable;
        }

        public void ReturnGrabbable(PhysicsGrabbable grabbable)
        {
            // Route by which pool the object came from.
            if (_activeBoxes.Remove(grabbable))
            {
                ReturnToPool(grabbable, _boxPool);
            }
            else if (_activeBowlingBalls.Remove(grabbable))
            {
                ReturnToPool(grabbable, _bowlingBallPool);
            }
            else if (_activePigs.Remove(grabbable))
            {
                ReturnToPool(grabbable, _pigPool);
            }
        }

        private void ReturnToPool(PhysicsGrabbable grabbable, Queue<PhysicsGrabbable> pool)
        {
            grabbable.gameObject.SetActive(false);
            pool.Enqueue(grabbable);
        }

        private void Prewarm(PhysicsGrabbable prefab, Queue<PhysicsGrabbable> pool, int count)
        {
            for (int i = 0; i < count; i++)
            {
                pool.Enqueue(CreateNew(prefab));
            }
        }

        private void Activate(PhysicsGrabbable grabbable, Vector3 position, Quaternion rotation)
        {
            grabbable.transform.SetParent(null, true);
            grabbable.transform.SetPositionAndRotation(position, rotation);
            grabbable.gameObject.SetActive(true);
            grabbable.Spawned();
        }

        private PhysicsGrabbable CreateNew(PhysicsGrabbable prefab)
        {
            PhysicsGrabbable newGrabbable = Instantiate(prefab, transform);
            newGrabbable.gameObject.SetActive(false);
            newGrabbable.SetPool(this);
            return newGrabbable;
        }
    }
}
