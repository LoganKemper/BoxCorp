using UnityEngine;

namespace BoxCorp
{
    /// <summary>
    /// A simple singleton base class for MonoBehaviours.
    /// </summary>
    public abstract class MonoBehaviourSingleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        public static T Instance { get; private set; }

        protected virtual void Awake()
        {
            if (Instance == null)
            {
                Instance = this as T;
            }
            else if (Instance != this)
            {
                Debug.LogWarning(
                    $"[{typeof(T).Name}] Duplicate detected. Destroying instance on {gameObject.name}.");
                Destroy(this);
            }
        }

        protected virtual void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
}
