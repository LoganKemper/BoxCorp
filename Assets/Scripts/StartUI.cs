using System;
using UnityEngine;

namespace BoxCorp
{
    public class StartUI : MonoBehaviour, IUIScreen
    {
        [Header("Screens")]
        [SerializeField] private GameObject _screen1;
        [SerializeField] private GameObject _screen2;

        [Header("Buttons")]
        [SerializeField] private MenuButton _nextButton;
        [SerializeField] private MenuButton _startButton;

        public event Action OnStartGame;

        public bool IsVisible => gameObject.activeSelf;

        private void Awake()
        {
            _nextButton.OnClick += ShowScreen2;
            _startButton.OnClick += () => OnStartGame?.Invoke();
        }

        public void Init()
        {
            Show();
        }

        public void Show()
        {
            gameObject.SetActive(true);
            ShowScreen1();
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        private void ShowScreen1()
        {
            _screen1.SetActive(true);
            _screen2.SetActive(false);
        }

        private void ShowScreen2()
        {
            _screen1.SetActive(false);
            _screen2.SetActive(true);
        }
    }
}
