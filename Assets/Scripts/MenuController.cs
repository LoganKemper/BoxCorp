using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace BoxCorp
{
    public class MenuController : MonoBehaviour
    {
        public static bool GameIsPaused { get; private set; }
        public static bool GameStarted { get; private set; }

        [Header("Screens")]
        [SerializeField] private StartUI _startUI;
        [SerializeField] private GameUI _gameUI;
        [SerializeField] private PauseUI _pauseUI;
        [SerializeField] private GameOverUI _gameOverUI;

        [Header("Pause Toggle")]
        [SerializeField] private MenuButton _pauseButton;
        [SerializeField] private MenuButton _unpauseButton;

        [Header("Audio")]
        [SerializeField] private AudioClip _pauseSound;

        private void Start()
        {
            Time.timeScale = 1f;
            GameIsPaused = true;
            GameStarted = false;

            _startUI.OnStartGame += StartGame;
            _pauseUI.OnResumeGame += () => SetPause(false);
            _pauseUI.OnResetGame += ReloadScene;
            _gameOverUI.OnResetGame += ReloadScene;

            _pauseButton.OnClick += () => SetPause(true);
            _unpauseButton.OnClick += () => SetPause(false);

            GameManager.Instance.OnGameOver += HandleGameOver;

            _gameUI.Init();
            _pauseUI.Init();
            _gameOverUI.Init();
            _startUI.Init();
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGameOver -= HandleGameOver;
            }
        }

        private void Update()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null)
            {
                return;
            }

            if ((keyboard.escapeKey.wasPressedThisFrame || keyboard.pKey.wasPressedThisFrame) && GameStarted)
            {
                SetPause(!GameIsPaused);
            }
            else if (keyboard.rKey.wasPressedThisFrame && GameIsPaused)
            {
                ReloadScene();
            }
        }

        private void ReloadScene()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        private void SetPause(bool pause, bool playSound = true)
        {
            GameIsPaused = pause;
            Time.timeScale = pause ? 0f : 1f;

            if (pause)
            {
                _gameUI.Hide();
                _pauseUI.Show();
            }
            else
            {
                _pauseUI.Hide();
                _gameUI.Show();
            }

            _pauseButton.gameObject.SetActive(!pause);
            _unpauseButton.gameObject.SetActive(pause);

            if (playSound && _pauseSound != null)
            {
                AudioManager.Instance.PlaySFX2D(_pauseSound, 0.8f, pause ? 0.8f : 1.2f);
            }
        }

        private void StartGame()
        {
            GameStarted = true;

            _startUI.Hide();
            SetPause(false, playSound: false);
        }

        private void HandleGameOver()
        {
            GameStarted = false;
            GameIsPaused = true;
            Time.timeScale = 0f;

            _gameUI.Hide();
            _pauseUI.Hide();
            _startUI.Hide();
            _gameOverUI.Show();
        }
    }
}
