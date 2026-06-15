using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace BoxCorp
{
    public class UIAnimation : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private UIAnimationData _animationData;
        [SerializeField] private bool _animateOnClick = true;

        private Coroutine _animationCoroutine;
        private Vector3 _initialScale;
        private Quaternion _initialRotation;
        private bool _isAnimating;

        private void Awake()
        {
            _initialScale = transform.localScale;
            _initialRotation = transform.localRotation;
        }

        private void OnDisable()
        {
            if (_isAnimating)
            {
                ResetTransform();
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (_animateOnClick && eventData.button == PointerEventData.InputButton.Left)
            {
                PlayAnimation();
            }
        }

        public void StopAnimation()
        {
            if (_animationCoroutine != null)
            {
                StopCoroutine(_animationCoroutine);
            }

            ResetTransform();
        }

        [ContextMenu(nameof(PlayAnimation))]
        public void PlayAnimation()
        {
            if (_animationCoroutine != null)
            {
                StopCoroutine(_animationCoroutine);
                _animationCoroutine = StartCoroutine(AnimationCoroutine(true));
            }
            else
            {
                _animationCoroutine = StartCoroutine(AnimationCoroutine(false));
            }
        }

        private IEnumerator AnimationCoroutine(bool animationCanceled = false)
        {
            _isAnimating = true;

            // Interpolate into first frame of animation.
            if (animationCanceled)
            {
                float interpolationTime = 0f;
                const float DURATION = 0.05f;

                Quaternion startingRotation = transform.localRotation;
                float startingScaleMultiplier = transform.localScale.x;

                while (interpolationTime < DURATION)
                {
                    float t = interpolationTime / DURATION;
                    float scale = Mathf.Lerp(startingScaleMultiplier, _initialScale.x, t);

                    transform.localScale = new Vector3(scale, scale, scale);
                    transform.localRotation = Quaternion.Lerp(startingRotation, _initialRotation, t);

                    interpolationTime += Time.deltaTime;
                    yield return null;
                }
            }

            if (_animationData.audioClip != null)
            {
                AudioManager.Instance.PlaySFX2D(
                    _animationData.audioClip, _animationData.volume, Random.Range(0.9f, 1.1f));
            }

            // Play animation.
            float time = 0f;
            while (time < _animationData.duration)
            {
                float t = time / _animationData.duration;
                float scaleMultiplier = Mathf.Lerp(
                    1f, _animationData.scaleAmount, _animationData.scaleCurve.Evaluate(t));
                float angle = _animationData.shakeAngle * Mathf.Sin(t * Mathf.PI * 4f) 
                    * _animationData.rotationCurve.Evaluate(t);

                transform.localScale = _initialScale * scaleMultiplier;
                transform.localRotation = Quaternion.Euler(0f, 0f, angle);

                time += Time.deltaTime;
                yield return null;
            }

            ResetTransform();
        }

        private void ResetTransform()
        {
            transform.localScale = _initialScale;
            transform.localRotation = _initialRotation;
            _animationCoroutine = null;
            _isAnimating = false;
        }
    }
}
