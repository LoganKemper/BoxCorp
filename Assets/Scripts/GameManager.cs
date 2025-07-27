using System;
using LoganKemper.Utilities;
using UnityEngine;

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
            }
        }

        public void InvalidItemInHopper()
        {
            OnInvalidItem?.Invoke();
        }
    }
}