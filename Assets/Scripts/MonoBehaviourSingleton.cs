using UnityEngine;

namespace LoganKemper.Utilities
{
    /// <summary>
    /// A simple singleton base class for MonoBehaviours that are manually placed in the scene.
    /// </summary>
    public abstract class MonoBehaviourSingleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        public static T Instance { get; private set; }

        protected virtual void Awake()
        {
            // If no instance exists, this becomes the instance
            if (Instance == null)
            {
                Instance = this as T;
            }
            // If an instance already exists and it's not this one, destroy this GameObject
            else if (Instance != this)
            {
                Debug.LogWarning($"[{typeof(T).Name}] Duplicate instance detected, destroying {gameObject.name}");
                Destroy(gameObject);
            }
        }

        protected virtual void OnDestroy()
        {
            // Clear the instance reference if this was the singleton instance
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
}