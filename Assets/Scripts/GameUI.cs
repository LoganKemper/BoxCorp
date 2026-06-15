using System.Collections;
using TMPro;
using UnityEngine;

namespace BoxCorp
{
    public class GameUI : MonoBehaviour, IUIScreen
    {
        private static readonly WaitForSeconds WAIT_CHAR_DELAY = new(0.01f);
        private static readonly WaitForSeconds WAIT_TEXT_LIFETIME = new(5f);

        [SerializeField] private Messages _messages;
        [SerializeField] private TMP_Text _scoreText;
        [SerializeField] private UIAnimation _scoreAnimation;
        [SerializeField] private TMP_Text _infoText;
        [SerializeField] private UIAnimation _infoAnimation;

        private Coroutine _typingCoroutine;

        public bool IsVisible => gameObject.activeSelf;

        private void Start()
        {
            GameManager.Instance.OnScored += HandleScored;
            GameManager.Instance.OnBoostThresholdReached += HandleBoostThresholdReached;
            GameManager.Instance.OnRewardThresholdReached += HandleRewardThresholdReached;
            GameManager.Instance.OnDirtThresholdReached += HandleDirtThresholdReached;
            GameManager.Instance.OnPigThresholdReached += HandlePigThresholdReached;
            GameManager.Instance.OnPigDefeated += HandlePigDefeated;
            GameManager.Instance.OnBigScoreThresholdReached += HandleBigScoreThresholdReached;

            _infoText.gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnScored -= HandleScored;
                GameManager.Instance.OnBoostThresholdReached -= HandleBoostThresholdReached;
                GameManager.Instance.OnRewardThresholdReached -= HandleRewardThresholdReached;
                GameManager.Instance.OnDirtThresholdReached -= HandleDirtThresholdReached;
                GameManager.Instance.OnPigThresholdReached -= HandlePigThresholdReached;
                GameManager.Instance.OnPigDefeated -= HandlePigDefeated;
                GameManager.Instance.OnBigScoreThresholdReached -= HandleBigScoreThresholdReached;
            }
        }

        public void Init()
        {
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

        private void HandleScored(int newScore)
        {
            _scoreText.text = _messages.scorePrefix + newScore;
            _scoreAnimation.PlayAnimation();
        }

        private void HandleBoostThresholdReached()
        {
            StartTextTyping(_infoText, _messages.boosting);
            _infoAnimation.PlayAnimation();
        }

        private void HandleRewardThresholdReached()
        {
            StartTextTyping(_infoText, _messages.bowling);
            _infoAnimation.PlayAnimation();
        }

        private void HandleDirtThresholdReached()
        {
            StartTextTyping(_infoText, _messages.dirt);
            _infoAnimation.PlayAnimation();
        }

        private void HandlePigThresholdReached()
        {
            StartTextTyping(_infoText, _messages.pigSpawned);
            _infoAnimation.PlayAnimation();
        }

        private void HandlePigDefeated()
        {
            StartTextTyping(_infoText, _messages.pigDefeated);
            _infoAnimation.PlayAnimation();
        }

        private void HandleBigScoreThresholdReached()
        {
            StartTextTyping(_infoText, _messages.bigScore);
            _infoAnimation.PlayAnimation();
            _scoreText.color = Color.gold;
        }

        private void StartTextTyping(TMP_Text textElement, string message)
        {
            textElement.gameObject.SetActive(true);

            if (_typingCoroutine != null)
            {
                StopCoroutine(_typingCoroutine);
            }
            _typingCoroutine = StartCoroutine(TypeTextCoroutine(textElement, message));
        }

        private IEnumerator TypeTextCoroutine(TMP_Text textElement, string message)
        {
            textElement.raycastTarget = true;
            textElement.maxVisibleCharacters = 0;
            textElement.text = message;
            textElement.ForceMeshUpdate();
            int totalChars = textElement.textInfo.characterCount;

            for (int visible = 1; visible <= totalChars; visible++)
            {
                textElement.maxVisibleCharacters = visible;

                yield return WAIT_CHAR_DELAY;
            }

            yield return WAIT_TEXT_LIFETIME;
            textElement.text = string.Empty;
            textElement.maxVisibleCharacters = int.MaxValue;
            textElement.raycastTarget = false;
        }
    }
}
