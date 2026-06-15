using System.Collections;
using TMPro;
using UnityEngine;

namespace BoxCorp
{
    public class ComputerScreen : MonoBehaviour
    {
        [Header("Text")]
        [SerializeField] private Messages _messages;
        [SerializeField] private TMP_Text _screenText;
        [SerializeField] private float _characterDelay = 0.04f;
        [SerializeField] private float _spaceDelay = 0.7f;
        [SerializeField] private float _delayAfterText = 5f;
        [SerializeField] private float _idleTimeout = 10f;

        [Header("Audio")]
        [SerializeField] private AudioClip _typeSound;
        [SerializeField] private float _volume = 0.5f;
        [SerializeField] private float _minPitch = 0.8f;
        [SerializeField] private float _maxPitch = 1.2f;

        private Coroutine _typingCoroutine;
        private Coroutine _idleCoroutine;

        private void Awake()
        {
            _screenText.text = string.Empty;
        }

        private void Start()
        {
            GameManager.Instance.OnScored += HandleScored;
            GameManager.Instance.OnInvalidItem += HandleInvalidItem;
            GameManager.Instance.OnEmployeeClicked += HandleEmployeeClicked;
            GameManager.Instance.OnComputerScreenClicked += HandleComputerScreenClicked;
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnScored -= HandleScored;
                GameManager.Instance.OnInvalidItem -= HandleInvalidItem;
                GameManager.Instance.OnEmployeeClicked -= HandleEmployeeClicked;
                GameManager.Instance.OnComputerScreenClicked -= HandleComputerScreenClicked;
            }
        }

        public void TypeOutText(string message)
        {
            if (_idleCoroutine != null)
            {
                StopCoroutine(_idleCoroutine);
                _idleCoroutine = null;
            }

            if (_typingCoroutine != null)
            {
                StopCoroutine(_typingCoroutine);
            }

            _typingCoroutine = StartCoroutine(TypeOutTextCoroutine(message, clearBefore: true));
        }

        private void HandleScored(int _)
        {
            if (_messages.altScoreMessages.Length > 0 && Random.value < 0.5f)
            {
                int index = Random.Range(0, _messages.altScoreMessages.Length);
                TypeOutText(_messages.altScoreMessages[index]);
            }
            else
            {
                TypeOutText(_messages.scoreMessage);
            }
        }

        private void HandleInvalidItem()
        {
            TypeOutText(_messages.invalidMessage);
        }

        private void HandleEmployeeClicked()
        {
            if (_messages.ticklingMessages.Length > 0 && Random.value < 0.2f)
            {
                int index = Random.Range(0, _messages.ticklingMessages.Length);
                TypeOutText(_messages.ticklingMessages[index]);
            }
        }

        private void HandleComputerScreenClicked()
        {
            if (Random.value < 0.5f)
            {
                TypeOutText(_messages.stopTouchingMessage);
            }
        }

        private IEnumerator TypeOutTextCoroutine(string message, bool clearBefore)
        {
            if (clearBefore)
            {
                _screenText.text = string.Empty;
            }

            foreach (char c in message)
            {
                _screenText.text += c;

                if (c == ' ')
                {
                    // Wait for space delay first.
                    if (_spaceDelay > 0f)
                    {
                        yield return new WaitForSeconds(_spaceDelay);
                    }

                    _screenText.text = string.Empty;
                }
                else
                {
                    if (_typeSound != null)
                    {
                        AudioManager.Instance.PlaySFX(
                            _typeSound, transform.position, _volume, Random.Range(_minPitch, _maxPitch));
                    }

                    // Normal character delay.
                    if (_characterDelay > 0f)
                    {
                        yield return new WaitForSeconds(_characterDelay);
                    }
                }
            }

            if (_delayAfterText > 0f)
            {
                yield return new WaitForSeconds(_delayAfterText);
            }

            _screenText.text = string.Empty;

            // After the current message finishes, start idle timer.
            _idleCoroutine = StartCoroutine(IdleTimerCoroutine());
        }

        private IEnumerator IdleTimerCoroutine()
        {
            yield return new WaitForSeconds(_idleTimeout);
            _idleCoroutine = null;

            TypeOutText(_messages.idleMessage);
        }
    }
}
