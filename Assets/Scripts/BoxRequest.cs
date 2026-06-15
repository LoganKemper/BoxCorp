using System.Collections;
using UnityEngine;

namespace BoxCorp
{
    public class BoxRequest : MonoBehaviour, IClickable
    {
        private static readonly int ANIM_PUSHED = Animator.StringToHash("Pushed");
        private static readonly WaitForSeconds WAIT_COOLDOWN = new(0.18f);

        [SerializeField] private BoxSpawner _boxSpawner;
        [SerializeField] private Animator _animator;
        [SerializeField] private AudioClip _beepSound;
        [SerializeField] private AudioClip _clickSound;

        private bool _canPress = true;

        private void OnEnable()
        {
            _canPress = true;
        }

        public void Pressed()
        {
            _boxSpawner.SpawnRequested();
            _animator.SetTrigger(ANIM_PUSHED);

            if (_beepSound != null)
            {
                AudioManager.Instance.PlaySFX(_beepSound, transform.position, 1f, Random.Range(0.8f, 1.2f));
            }

            if (_clickSound != null)
            {
                AudioManager.Instance.PlaySFX(_clickSound, transform.position, 0.8f, Random.Range(0.8f, 1.2f));
            }
        }

        public void OnClick(Vector3 clickPoint)
        {
            if (_canPress)
            {
                Pressed();
                _canPress = false;

                StartCoroutine(CooldownCoroutine());
            }
        }

        private IEnumerator CooldownCoroutine()
        {
            yield return WAIT_COOLDOWN;
            _canPress = true;
        }
    }
}
