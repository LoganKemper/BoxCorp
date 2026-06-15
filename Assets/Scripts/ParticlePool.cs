using System.Collections.Generic;
using UnityEngine;

namespace BoxCorp
{
    public class ParticlePool : MonoBehaviourSingleton<ParticlePool>
    {
        [Header("Regular Dust")]
        [SerializeField] private PooledParticle _dustPrefab;
        [SerializeField] private int _dustPoolSize = 10;
        private readonly Queue<PooledParticle> _dustPool = new();

        [Header("Small Dust")]
        [SerializeField] private PooledParticle _smallDustPrefab;
        [SerializeField] private int _smallDustPoolSize = 10;
        private readonly Queue<PooledParticle> _smallDustPool = new();

        [Header("Large Dust")]
        [SerializeField] private PooledParticle _largeDustPrefab;
        [SerializeField] private int _largeDustPoolSize = 3;
        private readonly Queue<PooledParticle> _largeDustPool = new();

        [Header("Box Dirt")]
        [SerializeField] private PooledParticle _boxDirtPrefab;
        [SerializeField] private int _boxDirtPoolSize = 3;
        private readonly Queue<PooledParticle> _boxDirtPool = new();

        protected override void Awake()
        {
            base.Awake();

            // Pre-warm pools
            for (int i = 0; i < _dustPoolSize; i++)
            {
                CreateNew(_dustPrefab, _dustPool);
            }
            for (int i = 0; i < _smallDustPoolSize; i++)
            {
                CreateNew(_smallDustPrefab, _smallDustPool);
            }
            for (int i = 0; i < _largeDustPoolSize; i++)
            {
                CreateNew(_largeDustPrefab, _largeDustPool);
            }
            for (int i = 0; i < _boxDirtPoolSize; i++)
            {
                CreateNew(_boxDirtPrefab, _boxDirtPool);
            }
        }

        public PooledParticle SpawnDust(Vector3 position) => Spawn(_dustPrefab, _dustPool, position);

        public PooledParticle SpawnSmallDust(Vector3 position) => Spawn(_smallDustPrefab, _smallDustPool, position);

        public PooledParticle SpawnLargeDust(Vector3 position) => Spawn(_largeDustPrefab, _largeDustPool, position);

        public PooledParticle SpawnBoxDirt(Vector3 position) => Spawn(_boxDirtPrefab, _boxDirtPool, position);

        public void Return(PooledParticle particle)
        {
            particle.gameObject.SetActive(false);
            particle.transform.SetParent(transform, false);
        }

        private PooledParticle Spawn(PooledParticle prefab, Queue<PooledParticle> pool, Vector3 position)
        {
            PooledParticle particle;
            if (pool.Count > 0)
            {
                particle = pool.Dequeue();
                pool.Enqueue(particle);
            }
            else
            {
                particle = CreateNew(prefab, pool);
            }

            particle.transform.SetParent(null, false);
            particle.transform.SetPositionAndRotation(position, Quaternion.identity);
            particle.gameObject.SetActive(true);
            particle.Play(this);
            return particle;
        }

        private PooledParticle CreateNew(PooledParticle prefab, Queue<PooledParticle> pool)
        {
            PooledParticle particle = Instantiate(prefab, transform);
            particle.gameObject.SetActive(false);
            pool.Enqueue(particle);
            return particle;
        }
    }
}
