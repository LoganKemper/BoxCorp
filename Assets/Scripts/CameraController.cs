using UnityEngine;

namespace BoxCorp
{
    public class CameraController : MonoBehaviourSingleton<CameraController>
    {
        [Header("Shake")]
        [SerializeField] private float _maxPositionOffset = 0.25f;
        [SerializeField] private float _maxRotationAngle = 2.5f;
        [SerializeField] private float _traumaDecayPerSecond = 1.5f;
        [SerializeField] private float _noiseFrequency = 25f;
        [SerializeField] private float _traumaExponent = 2f;

        private Vector3 _baseLocalPosition;
        private Quaternion _baseLocalRotation;
        private float _trauma;

        private void Start()
        {
            _baseLocalPosition = transform.localPosition;
            _baseLocalRotation = transform.localRotation;
        }

        private void LateUpdate()
        {
            if (_trauma <= 0f)
            {
                transform.SetLocalPositionAndRotation(_baseLocalPosition, _baseLocalRotation);
                return;
            }

            float shake = Mathf.Pow(_trauma, _traumaExponent);
            float time = Time.unscaledTime * _noiseFrequency;

            Vector3 offset = new(SampleNoise(time, 0f), SampleNoise(time, 10f), 0f);
            transform.localPosition = _baseLocalPosition + offset * (shake * _maxPositionOffset);

            float pitch = SampleNoise(time, 20f) * shake * _maxRotationAngle;
            float yaw = SampleNoise(time, 30f) * shake * _maxRotationAngle;
            float roll = SampleNoise(time, 40f) * shake * _maxRotationAngle;
            transform.localRotation = _baseLocalRotation * Quaternion.Euler(pitch, yaw, roll);

            _trauma = Mathf.Clamp01(_trauma - _traumaDecayPerSecond * Time.unscaledDeltaTime);
        }

        public void AddTrauma(float amount)
        {
            _trauma = Mathf.Clamp01(_trauma + amount);
        }

        public void Shake(float amount = 0.4f)
        {
            AddTrauma(amount);
        }

        public void ShakeSmall()
        {
            AddTrauma(0.3f);
        }

        public void ShakeBig()
        {
            AddTrauma(0.8f);
        }

        private static float SampleNoise(float time, float seed)
        {
            return Mathf.PerlinNoise(seed, time) * 2f - 1f;
        }

        [ContextMenu(nameof(TestShake))]
        private void TestShake()
        {
            AddTrauma(1f);
        }
    }
}
