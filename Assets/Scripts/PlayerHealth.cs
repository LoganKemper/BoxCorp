using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace BoxCorp
{
    public class PlayerHealth : MonoBehaviourSingleton<PlayerHealth>
    {
        [Header("Health")]
        [SerializeField] private float _maxHealth = 100f;
        [SerializeField] private float _regenDelay = 1f;
        [SerializeField] private float _regenPerSecond = 15f;

        [Header("Vignette")]
        [SerializeField] private Volume _damageVolume;
        [SerializeField] private Color _vignetteColor = Color.red;
        [Range(0f, 1f), SerializeField] private float _maxVignetteIntensity = 0.5f;

        [Header("Audio")]
        [SerializeField] private AudioClip _damageSound;
        [SerializeField] private AudioClip _deathSound;

        private Vignette _vignette;
        private float _health;
        private float _lastDamageTime;
        private bool _invincible;

        protected override void Awake()
        {
            base.Awake();

            _health = _maxHealth;

            if (_damageVolume.profile.TryGet(out _vignette))
            {
                _vignette.color.overrideState = true;
                _vignette.color.value = _vignetteColor;
                _vignette.intensity.overrideState = true;
            }

            UpdateVignette();
        }

        private void Update()
        {
            if (_health < _maxHealth && Time.time - _lastDamageTime >= _regenDelay)
            {
                _health = Mathf.Min(_maxHealth, _health + _regenPerSecond * Time.deltaTime);
                UpdateVignette();
            }
        }

        public void TakeDamage(float amount)
        {
            if (_health <= 0f || _invincible)
            {
                return;
            }

            _health = Mathf.Max(0f, _health - amount);
            _lastDamageTime = Time.time;
            UpdateVignette();

            if (_health <= 0f)
            {
                if (_deathSound != null)
                {
                    AudioManager.Instance.PlaySFX2D(_deathSound, 0.8f);
                }

                CameraController.Instance.ShakeBig();
                GameManager.Instance.GameOver();
                return;
            }

            if (_damageSound != null)
            {
                AudioManager.Instance.PlaySFX2D(_damageSound, 0.7f, Random.Range(0.8f, 1.2f));
            }

            CameraController.Instance.Shake(0.5f);
        }

        public void ToggleInvincible()
        {
            _invincible = !_invincible;
        }

        private void UpdateVignette()
        {
            if (_vignette != null)
            {
                _vignette.intensity.value = Mathf.Lerp(0f, _maxVignetteIntensity, 1f - _health / _maxHealth);
            }
        }
    }
}
