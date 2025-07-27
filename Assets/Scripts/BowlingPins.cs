using UnityEngine;

namespace BoxCorp
{
    public class BowlingPins : MonoBehaviour
    {
        [SerializeField] private GameObject pins;

        private void Start()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnRewardThreshold += ActivatePins;
            }
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnRewardThreshold -= ActivatePins;
            }
        }

        private void ActivatePins()
        {
            pins.SetActive(true);
        }
    }
}