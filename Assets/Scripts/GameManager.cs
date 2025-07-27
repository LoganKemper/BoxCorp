using System;
using LoganKemper.Utilities;
using UnityEngine;
using UnityEngine.Events;

namespace BoxCorp
{
    public class GameManager : MonoBehaviourSingleton<GameManager>
    {
        public event Action<int> OnScore;
        public event Action OnInvalidItem;
        public event Action OnBoostThreshold;
        public event Action OnRewardThreshold;

        [SerializeField] private int boostThreshold = 5;
        [SerializeField] private int rewardThreshold = 10;
        [Space(20), SerializeField] private UnityEvent onRewardEvent;

        private int score = 0;

        public void Scored()
        {
            score++;
            OnScore?.Invoke(score);

            if (score == boostThreshold)
            {
                OnBoostThreshold?.Invoke();
            }
            else if (score == rewardThreshold)
            {
                OnRewardThreshold?.Invoke();
                onRewardEvent.Invoke();
            }
        }

        public void InvalidItemInHopper()
        {
            OnInvalidItem?.Invoke();
        }
    }
}