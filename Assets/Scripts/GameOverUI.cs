using System;
using TMPro;
using UnityEngine;

namespace BoxCorp
{
    public class GameOverUI : MonoBehaviour, IUIScreen
    {
        [SerializeField] private Messages _messages;
        [SerializeField] private TMP_Text _scoreText;
        [SerializeField] private MenuButton _resetButton;

        public event Action OnResetGame;

        public bool IsVisible => gameObject.activeSelf;

        private void Awake()
        {
            _resetButton.OnClick += () => OnResetGame?.Invoke();
        }

        public void Init()
        {
            Hide();
        }

        public void Show()
        {
            gameObject.SetActive(true);
            _scoreText.SetText($"{_messages.scorePrefix}{GameManager.Instance.Score}");
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}
