using UnityEngine;

namespace BoxCorp
{
    public class EffectsOnClick : MonoBehaviour, IClickable
    {
        [SerializeField] private AudioClip _clickedSound;
        [SerializeField] private float _volume = 1f;
        [SerializeField] private Vector2 _pitchRange = new(0.9f, 1.1f);

        public void OnClick(Vector3 clickPoint)
        {
            if (_clickedSound != null)
            {
                AudioManager.Instance.PlaySFX(
                    _clickedSound, transform.position, _volume, Vector2Random(_pitchRange));
            }

            HandleClicked(clickPoint);
        }

        protected virtual void HandleClicked(Vector3 clickPoint) { }

        protected float Vector2Random(Vector2 vector2)
        {
            return Random.Range(vector2.x, vector2.y);
        }
    }
}
