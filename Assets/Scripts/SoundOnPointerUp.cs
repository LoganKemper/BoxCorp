using UnityEngine;
using UnityEngine.EventSystems;

namespace BoxCorp
{
    public class SoundOnPointerUp : MonoBehaviour, IPointerUpHandler
    {
        [SerializeField] private AudioClip _sound;
        [SerializeField] private float _volume = 1f;

        public void OnPointerUp(PointerEventData eventData)
        {
            AudioManager.Instance.PlaySFX2D(_sound, _volume);
        }
    }
}
