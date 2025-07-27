using UnityEngine;

namespace BoxCorp
{
    public class DestroyAfterTime : MonoBehaviour
    {
        [SerializeField] private float lifetime = 1f;

        private void Start()
        {
            Destroy(gameObject, lifetime);
        }
    }
}