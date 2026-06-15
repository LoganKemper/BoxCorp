using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BoxCorp
{
    public class MenuButton : MonoBehaviour,
        IPointerEnterHandler, IPointerExitHandler,
        IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
    {
        [Header("Target")]
        [SerializeField] private Graphic _targetGraphic;

        [Header("Colors")]
        [SerializeField] private Color _normalColor = Color.white;
        [SerializeField] private Color _hoverColor = Color.lightGray;
        [SerializeField] private Color _pressedColor = Color.gray;
        [SerializeField] private float _fadeDuration = 0.1f;

        [Header("Audio")]
        [SerializeField] private AudioClip _hoverSound;
        [SerializeField] private AudioClip _clickSound;

        private bool _isPointerInside;
        private bool _isPointerDown;

        public Action OnClick;

        private void OnEnable()
        {
            // Reset to a clean state.
            _isPointerInside = false;
            _isPointerDown = false;
            ApplyColor(_normalColor, instant: true);
        }

        private void Reset()
        {
            _targetGraphic = GetComponent<Graphic>();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _isPointerInside = true;
            UpdateColor();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _isPointerInside = false;
            UpdateColor();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _isPointerDown = true;
            PlaySound(_hoverSound);
            UpdateColor();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _isPointerDown = false;
            UpdateColor();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
            {
                return;
            }

            PlaySound(_clickSound);
            OnClick?.Invoke();
        }

        private void UpdateColor()
        {
            Color target =
                _isPointerDown ? _pressedColor :
                _isPointerInside ? _hoverColor :
                _normalColor;

            ApplyColor(target, instant: false);
        }

        private void ApplyColor(Color color, bool instant)
        {
            if (_targetGraphic == null)
            {
                return;
            }

            _targetGraphic.CrossFadeColor(color, instant ? 0f : _fadeDuration, ignoreTimeScale: true, useAlpha: true);
        }

        private void PlaySound(AudioClip clip)
        {
            if (clip != null)
            {
                AudioManager.Instance.PlaySFX2D(clip);
            }
        }
    }
}
