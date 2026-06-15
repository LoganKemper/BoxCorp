using System;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

namespace BoxCorp
{
    public class PauseUI : MonoBehaviour, IUIScreen
    {
        private const string VOLUME_PARAM = "MasterVolume";
        private const string VOLUME_PREF_KEY = "Volume";
        private const float DEFAULT_VOLUME = 0.75f;

        [Header("Buttons")]
        [SerializeField] private MenuButton _resumeButton;
        [SerializeField] private MenuButton _resetButton;

        [Header("Volume")]
        [SerializeField] private AudioMixer _audioMixer;
        [SerializeField] private TMP_Text _volumeText;
        [SerializeField] private Slider _volumeSlider;

        public event Action OnResumeGame;
        public event Action OnResetGame;

        public bool IsVisible => gameObject.activeSelf;

        private void Awake()
        {
            _resumeButton.OnClick += () => OnResumeGame?.Invoke();
            _resetButton.OnClick += () => OnResetGame?.Invoke();
        }

        public void Init()
        {
            float savedVolume = Mathf.Clamp01(PlayerPrefs.GetFloat(VOLUME_PREF_KEY, DEFAULT_VOLUME));
            _volumeSlider.SetValueWithoutNotify(savedVolume);
            _volumeSlider.onValueChanged.AddListener(SetVolume);
            SetVolume(savedVolume);
            Hide();
        }

        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        private void SetVolume(float volume)
        {
            float decibels = (volume > 0.001f) ? Mathf.Log10(volume) * 20f : -80f;
            _audioMixer.SetFloat(VOLUME_PARAM, decibels);
            PlayerPrefs.SetFloat(VOLUME_PREF_KEY, volume);
            _volumeText.SetText("Volume: {0:0}%", volume * 100f);
        }
    }
}
