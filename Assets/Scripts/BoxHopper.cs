using System.Collections;
using UnityEngine;

namespace BoxCorp
{
    public class BoxHopper : MonoBehaviour
    {
        private readonly static WaitForSeconds WAIT_ONE_SECOND = new(1f);

        [Header("Light")]
        [SerializeField] private Light _hopperLight;
        [SerializeField] private Color _defaultColor = Color.white;
        [SerializeField] private Color _scoreColor = Color.green;
        [SerializeField] private Color _incorrectColor = Color.red;
        [SerializeField] private Color _pigColor = Color.pink;

        [Header("Audio")]
        [SerializeField] private AudioClip _boxSound;
        [SerializeField] private AudioClip _invalidSound;

        private Coroutine _lightCoroutine;

        private void Start()
        {
            _hopperLight.color = _defaultColor;
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.TryGetComponent(out PhysicsGrabbable grabbable))
            {
                if (grabbable.GrabbableType == GrabbableType.DirtyBox)
                {
                    return;
                }

                Color lightColor;
                if (grabbable.GrabbableType == GrabbableType.Box)
                {
                    lightColor = _scoreColor;

                    GameManager.Instance.Scored();

                    if (_boxSound != null)
                    {
                        AudioManager.Instance.PlaySFX(
                            _boxSound, transform.position, 0.9f, Random.Range(0.8f, 1.2f));
                    }
                }
                else
                {
                    if (grabbable.GrabbableType == GrabbableType.Pig)
                    {
                        lightColor = _pigColor;
                        GameManager.Instance.PigDefeated();
                    }
                    else
                    {
                        lightColor = _incorrectColor;
                    }

                    GameManager.Instance.InvalidItemInHopper();

                    if (_invalidSound != null)
                    {
                        AudioManager.Instance.PlaySFX(
                            _invalidSound, transform.position, 0.7f, Random.Range(0.8f, 1.1f));
                    }
                }

                if (_lightCoroutine != null)
                {
                    StopCoroutine(_lightCoroutine);
                }
                _lightCoroutine = StartCoroutine(LightCoroutine(lightColor));

                StartCoroutine(SuckInObjectCoroutine(grabbable));
            }
        }

        private IEnumerator SuckInObjectCoroutine(PhysicsGrabbable grabbable)
        {
            grabbable.SetCanGrab(false);

            Vector3 startPosition = grabbable.transform.position;
            float timeElapsed = 0f;
            const float DURATION = 0.2f;

            while (timeElapsed < DURATION)
            {
                grabbable.transform.position = Vector3.Lerp(
                    startPosition, transform.position, timeElapsed / DURATION);
                timeElapsed += Time.deltaTime;
                yield return null;
            }

            grabbable.InHopper();
        }

        private IEnumerator LightCoroutine(Color targetColor)
        {
            float timeElapsed = 0f;
            float duration = 0.15f;
            Color startColor = _hopperLight.color;

            while (timeElapsed < duration)
            {
                _hopperLight.color = Color.Lerp(startColor, targetColor, timeElapsed / duration);
                timeElapsed += Time.deltaTime;
                yield return null;
            }

            _hopperLight.color = targetColor;

            yield return WAIT_ONE_SECOND;

            timeElapsed = 0f;
            duration = 0.5f;

            while (timeElapsed < duration)
            {
                _hopperLight.color = Color.Lerp(targetColor, _defaultColor, timeElapsed / duration);
                timeElapsed += Time.deltaTime;
                yield return null;
            }

            _hopperLight.color = _defaultColor;
        }
    }
}
