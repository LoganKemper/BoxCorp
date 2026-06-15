using UnityEngine;
using UnityEngine.InputSystem;

namespace BoxCorp
{
    public class DebugControls : MonoBehaviour
    {
        private bool _active;

        private void Awake()
        {
#if UNITY_EDITOR
            _active = true;
#endif
        }

        private void Update()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null)
            {
                return;
            }

            if (!_active)
            {
                // Enable debug mode by pressing []\B all at the same time.
                if (keyboard.leftBracketKey.isPressed && keyboard.rightBracketKey.isPressed
                    && keyboard.backslashKey.isPressed && keyboard.bKey.isPressed)
                {
                    _active = true;
                    Debug.Log("Debug mode enabled.");
                }

                return;
            }

            // Increase score.
            if (keyboard.minusKey.wasPressedThisFrame)
            {
                GameManager.Instance.Scored();
            }
            else if (keyboard.equalsKey.wasPressedThisFrame)
            {
                for (int i = 0; i < 5; i++)
                {
                    GameManager.Instance.Scored();
                }
            }

            // Toggle invincibility.
            else if (keyboard.backspaceKey.wasPressedThisFrame)
            {
                PlayerHealth.Instance.ToggleInvincible();
            }

            // Cap framerate.
            else if (keyboard.digit1Key.wasPressedThisFrame)
            {
                SetFrameRate(5);
            }
            else if (keyboard.digit2Key.wasPressedThisFrame)
            {
                SetFrameRate(15);
            }
            else if (keyboard.digit3Key.wasPressedThisFrame)
            {
                SetFrameRate(30);
            }
            else if (keyboard.digit4Key.wasPressedThisFrame)
            {
                SetFrameRate(60);
            }
            else if (keyboard.digit0Key.wasPressedThisFrame)
            {
                SetFrameRate(-1);
            }

            // Change timescale.
            else if (keyboard.pageUpKey.wasPressedThisFrame)
            {
                Time.timeScale *= 2f;
            }
            else if (keyboard.pageDownKey.wasPressedThisFrame)
            {
                Time.timeScale /= 2f;
            }
            else if (keyboard.homeKey.wasPressedThisFrame)
            {
                Time.timeScale = 1f;
            }
        }

        private void SetFrameRate(int frameRate)
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = frameRate;
        }
    }
}
