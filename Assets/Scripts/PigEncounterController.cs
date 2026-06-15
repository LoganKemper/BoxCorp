using System.Collections;
using UnityEngine;

namespace BoxCorp
{
    public class PigEncounterController : MonoBehaviour
    {
        [SerializeField] private Pig _pig;
        [SerializeField] private Transform _runToTarget;
        [SerializeField] private AudioClip _warningSound;
        [SerializeField] private AudioClip _spawnSound;
        [SerializeField] private AudioClip _defeatedSound;

        private void Start()
        {
            GameManager.Instance.OnPigThresholdReached += HandlePigThresholdReached;
            GameManager.Instance.OnPigDefeated += HandlePigDefeated;

            _pig.gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnPigThresholdReached -= HandlePigThresholdReached;
                GameManager.Instance.OnPigDefeated -= HandlePigDefeated;
            }
        }

        private void HandlePigThresholdReached()
        {
            StartCoroutine(PigSpawnCoroutine());
        }

        private void HandlePigDefeated()
        {
            if (_defeatedSound != null)
            {
                AudioManager.Instance.PlaySFX2D(_defeatedSound, 1f);
            }

            StartCoroutine(PigDefeatedCoroutine());
        }

        private void SpawnPig()
        {
            _pig.gameObject.SetActive(true);
            _pig.Setup(_runToTarget);
            _pig.BeginEncounter();
        }

        private IEnumerator PigSpawnCoroutine()
        {
            WaitForSeconds waitHalfSecond = new(0.5f);
            WaitForSeconds waitOneSecond = new(1f);

            yield return waitOneSecond;

            if (_spawnSound != null)
            {
                AudioManager.Instance.PlaySFX2D(_spawnSound, 0.7f);
            }

            LightingManager.Instance.FadeColor(Color.darkRed, 0.5f);
            yield return waitOneSecond;
            LightingManager.Instance.StartFlicker();
            PlayWarningSound(0.6f);
            yield return waitHalfSecond;
            PlayWarningSound(0.5f);
            yield return waitHalfSecond;
            PlayWarningSound(0.4f);
            yield return waitOneSecond;
            LightingManager.Instance.StopFlicker();
            yield return waitOneSecond;
            SpawnPig();
        }

        private void PlayWarningSound(float pitch)
        {
            if (_warningSound != null)
            {
                AudioManager.Instance.PlaySFX2D(_warningSound, 1f, pitch);
            }
        }

        private IEnumerator PigDefeatedCoroutine()
        {
            yield return new WaitForSeconds(0.5f);
            LightingManager.Instance.FadeToOriginalColor(1f);
        }
    }
}
