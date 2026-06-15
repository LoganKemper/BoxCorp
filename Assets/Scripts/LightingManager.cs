using System.Collections;
using UnityEngine;

namespace BoxCorp
{
    public class LightingManager : MonoBehaviourSingleton<LightingManager>
    {
        [SerializeField] private Light _directionalLight;
        [SerializeField] private float _flickerIntensity = 0.5f;
        [SerializeField] private float _flickerInterval = 0.05f;

        private Coroutine _fadeCoroutine;
        private Coroutine _flickerCoroutine;
        private Color _originalColor;
        private float _baseIntensity;

        protected override void Awake()
        {
            base.Awake();

            _originalColor = _directionalLight.color;
            _baseIntensity = _directionalLight.intensity;
        }

        public void FadeColor(Color toColor, float duration)
        {
            if (_fadeCoroutine != null)
            {
                StopCoroutine(_fadeCoroutine);
            }
            _fadeCoroutine = StartCoroutine(FadeColorCoroutine(toColor, duration));
        }

        public void FadeToOriginalColor(float duration)
        {
            FadeColor(_originalColor, duration);
        }

        public void StartFlicker()
        {
            StartFlicker(_flickerIntensity, _flickerInterval);
        }

        public void StartFlicker(float intensity, float interval)
        {
            if (_flickerCoroutine != null)
            {
                StopCoroutine(_flickerCoroutine);
            }
            _flickerCoroutine = StartCoroutine(FlickerCoroutine(intensity, interval));
        }

        public void StopFlicker()
        {
            if (_flickerCoroutine != null)
            {
                StopCoroutine(_flickerCoroutine);
                _flickerCoroutine = null;
            }
            _directionalLight.intensity = _baseIntensity;
        }

        private IEnumerator FadeColorCoroutine(Color toColor, float duration)
        {
            float timeElapsed = 0f;
            Color startColor = _directionalLight.color;

            while (timeElapsed < duration)
            {
                _directionalLight.color = Color.Lerp(startColor, toColor, timeElapsed / duration);
                timeElapsed += Time.deltaTime;
                yield return null;
            }

            _directionalLight.color = toColor;
            _fadeCoroutine = null;
        }

        private IEnumerator FlickerCoroutine(float intensity, float interval)
        {
            while (true)
            {
                _directionalLight.intensity = Mathf.Max(0f, _baseIntensity + Random.Range(-intensity, intensity));
                yield return new WaitForSeconds(interval);
            }
        }
    }
}
