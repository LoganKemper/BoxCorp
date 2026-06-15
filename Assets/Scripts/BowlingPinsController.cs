using UnityEngine;

namespace BoxCorp
{
    public class BowlingPinsController : MonoBehaviour
    {
        [SerializeField] private GameObject _pinsRoot;
        [SerializeField] private Transform _dustSpawnPoint;
        [SerializeField] private AudioClip _spawnSound;

        private void Start()
        {
            GameManager.Instance.OnRewardThresholdReached += HandleRewardThresholdReached;

            _pinsRoot.SetActive(false);
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnRewardThresholdReached -= HandleRewardThresholdReached;
            }
        }

        private void HandleRewardThresholdReached()
        {
            _pinsRoot.SetActive(true);
            ParticlePool.Instance.SpawnLargeDust(_dustSpawnPoint.position);

            if (_spawnSound != null)
            {
                AudioManager.Instance.PlaySFX(_spawnSound, transform.position);
            }
        }
    }
}
