using UnityEngine;
using TMPro;
using System.Collections;

namespace BoxCorp
{
    public class ComputerScreen : MonoBehaviour
    {
        [Header("Text Settings")]
        [SerializeField] private TMP_Text screenText;
        [SerializeField] private float characterDelay = 0.05f;
        [SerializeField] private float spaceDelay = 0.05f;
        [SerializeField] private float delayAfterText = 5f;
        [SerializeField] private float idleTimeout = 10f;

        [Header("Messages")]
        [SerializeField] private string scoreMessage;
        [SerializeField] private string invalidMessage;
        [SerializeField] private string idleMessage;

        private Coroutine typingCoroutine;
        private Coroutine idleCoroutine;

        public void TypeOutText(string message)
        {
            if (idleCoroutine != null)
            {
                StopCoroutine(idleCoroutine);
                idleCoroutine = null;
            }

            if (typingCoroutine != null)
            {
                StopCoroutine(typingCoroutine);
            }

            typingCoroutine = StartCoroutine(TypeOutTextCoroutine(message, true));
        }

        private void Awake()
        {
            screenText.text = string.Empty;
        }

        private void Start()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnScore += OnScore;
                GameManager.Instance.OnInvalidItem += OnInvalidItem;
            }
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnScore -= OnScore;
                GameManager.Instance.OnInvalidItem -= OnInvalidItem;
            }
        }

        private void OnScore(int _)
        {
            TypeOutText(scoreMessage);
        }

        private void OnInvalidItem()
        {
            TypeOutText(invalidMessage);
        }

        private IEnumerator TypeOutTextCoroutine(string message, bool clearBefore)
        {
            if (clearBefore)
            {
                screenText.text = string.Empty;
            }

            foreach (char c in message)
            {
                screenText.text += c;

                if (c == ' ')
                {
                    // Wait for space delay first
                    if (spaceDelay > 0f)
                    {
                        yield return new WaitForSeconds(spaceDelay);
                    }

                    screenText.text = string.Empty;
                }
                else
                {
                    // Normal character delay
                    if (characterDelay > 0f)
                    {
                        yield return new WaitForSeconds(characterDelay);
                    }
                }
            }

            if (delayAfterText > 0f)
            {
                yield return new WaitForSeconds(delayAfterText);
            }

            screenText.text = string.Empty;

            // After the current message finishes, start idle timer
            idleCoroutine = StartCoroutine(IdleTimerCoroutine());
        }

        private IEnumerator IdleTimerCoroutine()
        {
            yield return new WaitForSeconds(idleTimeout);
            idleCoroutine = null;

            TypeOutText(idleMessage);
        }
    }
}