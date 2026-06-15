using UnityEngine;

namespace BoxCorp
{
    [RequireComponent(typeof(ParticleSystem))]
    public class PooledParticle : MonoBehaviour
    {
        [SerializeField] private ParticleSystem _particleSystem;

        private ParticlePool _pool;

        private void OnParticleSystemStopped()
        {
            if (_pool != null)
            {
                _pool.Return(this);
            }
        }

        private void Reset()
        {
            _particleSystem = GetComponent<ParticleSystem>();
        }

        public void Play(ParticlePool pool)
        {
            _pool = pool;
            _particleSystem.Clear(true);
            _particleSystem.Play(true);
        }
    }
}
