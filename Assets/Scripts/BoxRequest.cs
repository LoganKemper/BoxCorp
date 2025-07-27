using System.Collections;
using UnityEngine;

namespace BoxCorp
{
    public class BoxRequest : MonoBehaviour, IClickable
    {
        [SerializeField] private BoxSpawner boxSpawner;
        [SerializeField] private Animator animator;
        [SerializeField] private float cooldown = 0.25f;

        private readonly int pushed = Animator.StringToHash("Pushed");
        private bool canPress = true;

        public void Pressed()
        {
            boxSpawner.SpawnBox();
            animator.SetTrigger(pushed);
        }

        public void OnClick()
        {
            if (canPress)
            {
                Pressed();
                canPress = false;

                StartCoroutine(CooldownEnumerator());
            }
        }

        private IEnumerator CooldownEnumerator()
        {
            yield return new WaitForSeconds(cooldown);
            canPress = true;
        }

        private void OnEnable()
        {
            canPress = true;
        }
    }
}