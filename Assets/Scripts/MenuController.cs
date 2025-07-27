using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace BoxCorp
{
    public class MenuController : MonoBehaviour
    {
        [Header("UIs")]
        [SerializeField] private GameObject gameUI;
        [SerializeField] private GameObject pauseUI;
        [SerializeField] private GameObject startUI;

        [Header("Start Screens")]
        [SerializeField] private GameObject startScreen1;
        [SerializeField] private GameObject startScreen2;

        [Header("Start Screen Buttons")]
        [SerializeField] private Button nextButton;
        [SerializeField] private Button startButton;

        public static bool GameIsPaused { get; private set; } = false;
        public static bool GameStarted { get; private set; } = false;

        private void Start()
        {
            Time.timeScale = 1f;
            GameIsPaused = true;
            GameStarted = false;

            gameUI.SetActive(false);
            pauseUI.SetActive(false);
            startUI.SetActive(true);

            ShowStartScreen1();

            nextButton.onClick.AddListener(ShowStartScreen2);
            startButton.onClick.AddListener(StartGame);

            GameIsPaused = true;
            GameStarted = false;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape) && GameStarted)
            {
                TogglePaused();
            }
            else if (Input.GetKeyDown(KeyCode.R))
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
        }

        private void TogglePaused()
        {
            SetPause(!GameIsPaused);
        }

        private void SetPause(bool pause)
        {
            GameIsPaused = pause;
            Time.timeScale = pause ? 0f : 1f;

            gameUI.SetActive(!pause);
            pauseUI.SetActive(pause);
        }

        private void ShowStartScreen1()
        {
            startScreen1.SetActive(true);
            startScreen2.SetActive(false);
        }

        private void ShowStartScreen2()
        {
            startScreen1.SetActive(false);
            startScreen2.SetActive(true);
        }

        private void StartGame()
        {
            GameStarted = true;

            SetPause(false);

            gameUI.SetActive(true);
            pauseUI.SetActive(false);
            startUI.SetActive(false);
        }
    }
}