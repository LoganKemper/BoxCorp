using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace BoxCorp
{
    public class UIAnimation : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private UIAnimationData animationData;
        [SerializeField] private bool animateOnClick = true;

        private Coroutine animationCoroutine;
        private Vector3 initialScale;
        private Quaternion initialRotation;
        private bool isAnimating;

        public void OnPointerClick(PointerEventData eventData)
        {
            if (animateOnClick && eventData.button == PointerEventData.InputButton.Left)
            {
                PlayAnimation();
            }
        }

        public void StopAnimation()
        {
            if (animationCoroutine != null)
            {
                StopCoroutine(animationCoroutine);
            }

            ResetTransform();
        }

        private void Awake()
        {
            initialScale = transform.localScale;
            initialRotation = transform.localRotation;
        }

        private void OnDisable()
        {
            if (isAnimating)
            {
                ResetTransform();
            }
        }

        [ContextMenu(nameof(PlayAnimation))]
        public void PlayAnimation()
        {
            if (animationCoroutine != null)
            {
                StopCoroutine(animationCoroutine);
                animationCoroutine = StartCoroutine(AnimationCoroutine(true));
            }
            else
            {
                animationCoroutine = StartCoroutine(AnimationCoroutine(false));
            }
        }

        private IEnumerator AnimationCoroutine(bool animationCanceled = false)
        {
            isAnimating = true;

            // Interpolate into first frame of animation
            if (animationCanceled)
            {
                float interpolationTime = 0f;
                const float duration = 0.05f;

                Quaternion startingRotation = transform.localRotation;
                float startingScaleMultiplier = transform.localScale.x;

                while (interpolationTime < duration)
                {
                    float t = interpolationTime / duration;
                    float scale = Mathf.Lerp(startingScaleMultiplier, initialScale.x, t);

                    transform.localScale = new Vector3(scale, scale, scale);
                    transform.localRotation = Quaternion.Lerp(startingRotation, initialRotation, t);

                    interpolationTime += Time.deltaTime;
                    yield return null;
                }
            }

            // Play animation
            float time = 0f;
            while (time < animationData.duration)
            {
                float t = time / animationData.duration;
                float scaleMultiplier = Mathf.Lerp(1f, animationData.scaleAmount, animationData.scaleCurve.Evaluate(t));
                float angle = animationData.shakeAngle * Mathf.Sin(t * Mathf.PI * 4f) * animationData.rotationCurve.Evaluate(t);

                transform.localScale = initialScale * scaleMultiplier;
                transform.localRotation = Quaternion.Euler(0f, 0f, angle);

                time += Time.deltaTime;
                yield return null;
            }

            ResetTransform();
        }

        private void ResetTransform()
        {
            transform.localScale = initialScale;
            transform.localRotation = initialRotation;
            animationCoroutine = null;
            isAnimating = false;
        }
    }
}