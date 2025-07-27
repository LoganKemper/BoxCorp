using BoxCorp;
using System.Collections;
using TMPro;
using UnityEngine;

namespace BoxCorp
{
    public class GameUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text scoreText;
        [SerializeField] private UIAnimation scoreAnimation;
        [SerializeField] private TMP_Text infoText;
        [SerializeField] private UIAnimation infoAnimation;
        [SerializeField] private string scorePrefix = "Boxes: ";
        [SerializeField] private string boostMessage;
        [SerializeField] private string rewardMessage;

        private Coroutine typeCoroutine;

        private void Start()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnScore += OnScore;
                GameManager.Instance.OnBoostThreshold += OnBoostThreshold;
                GameManager.Instance.OnRewardThreshold += OnRewardThreshold;
            }

            infoText.gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnScore -= OnScore;
                GameManager.Instance.OnBoostThreshold -= OnBoostThreshold;
                GameManager.Instance.OnRewardThreshold -= OnRewardThreshold;
            }
        }

        private void OnScore(int newScore)
        {
            scoreText.text = scorePrefix + newScore;
            scoreAnimation.PlayAnimation();
        }

        private void OnBoostThreshold()
        {
            StartTextTyping(infoText, boostMessage);
            infoAnimation.PlayAnimation();
        }

        private void OnRewardThreshold()
        {
            StartTextTyping(infoText, rewardMessage);
            infoAnimation.PlayAnimation();
        }

        private void StartTextTyping(TMP_Text textElement, string message)
        {
            textElement.gameObject.SetActive(true);

            if (typeCoroutine != null)
            {
                StopCoroutine(typeCoroutine);
            }
            typeCoroutine = StartCoroutine(TypeTextCoroutine(textElement, message));
        }

        private IEnumerator TypeTextCoroutine(TMP_Text textElement, 
            string message, 
            float lifetime = 5f, 
            float characterDelay = 0.01f)
        {
            textElement.text = string.Empty;

            foreach (char c in message)
            {
                textElement.text += c;

                yield return new WaitForSeconds(characterDelay);
            }

            yield return new WaitForSeconds(lifetime);
            textElement.text = string.Empty;
        }
    }
}