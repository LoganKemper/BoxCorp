using System.Collections;
using UnityEngine;

namespace BoxCorp
{
    public class BoxHopper : MonoBehaviour
    {
        [SerializeField] private Light pointLight;
        [SerializeField] private Color defaultColor = Color.white;
        [SerializeField] private Color scoreColor = Color.green;
        [SerializeField] private Color incorrectColor = Color.red;

        private Coroutine lightCoroutine;

        private void Start()
        {
            pointLight.color = defaultColor;
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.TryGetComponent(out PhysicsGrabbable grabbable))
            {
                if (grabbable.GetGrabbableType() == GrabbableType.Box)
                {
                    if (lightCoroutine != null)
                    {
                        StopCoroutine(lightCoroutine);
                    }
                    lightCoroutine = StartCoroutine(LightCoroutine(scoreColor));

                    GameManager.Instance.Scored();
                }
                else
                {
                    if (lightCoroutine != null)
                    {
                        StopCoroutine(lightCoroutine);
                    }
                    lightCoroutine = StartCoroutine(LightCoroutine(incorrectColor));

                    GameManager.Instance.InvalidItemInHopper();
                }

                StartCoroutine(SuckInObject(grabbable));
            }
        }

        private IEnumerator SuckInObject(PhysicsGrabbable grabbable)
        {
            grabbable.SetCanGrab(false);

            Vector3 startPosition = grabbable.transform.position;
            float timeElapsed = 0f;
            const float duration = 0.2f;

            while (timeElapsed < duration)
            {
                grabbable.transform.position = Vector3.Lerp(startPosition, transform.position, timeElapsed / duration);
                timeElapsed += Time.deltaTime;
                yield return null;
            }

            grabbable.InHopper();
        }

        private IEnumerator LightCoroutine(Color targetColor)
        {
            float timeElapsed = 0f;
            float duration = 0.15f;
            Color startColor = pointLight.color;

            while (timeElapsed < duration)
            {
                pointLight.color = Color.Lerp(startColor, targetColor, timeElapsed / duration);
                timeElapsed += Time.deltaTime;
                yield return null;
            }

            pointLight.color = targetColor;

            yield return new WaitForSeconds(1f);

            timeElapsed = 0f;
            duration = 0.5f;

            while (timeElapsed < duration)
            {
                pointLight.color = Color.Lerp(targetColor, defaultColor, timeElapsed / duration);
                timeElapsed += Time.deltaTime;
                yield return null;
            }

            pointLight.color = defaultColor;
        }
    }
}