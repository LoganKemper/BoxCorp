using System;
using UnityEngine;

namespace BoxCorp
{
    public class GameManager : MonoBehaviourSingleton<GameManager>
    {
        [SerializeField] private int _boostThreshold = 5;
        [SerializeField] private int _rewardThreshold = 10;
        [SerializeField] private int _dirtThreshold = 15;
        [SerializeField] private int _dirtLengthAverage = 5;
        [SerializeField] private int _dirtLengthDeviation = 2;
        [SerializeField] private int _pigThreshold = 25;
        [SerializeField] private int _bigScoreThreshold = 50;

        private int _score;
        private int _dirtEnd;
        private bool _gameOver;
        private bool _pigDefeated;

        public event Action<int> OnScored;
        public event Action OnEmployeeClicked;
        public event Action OnComputerScreenClicked;
        public event Action OnInvalidItem;
        public event Action OnBoostThresholdReached;
        public event Action OnRewardThresholdReached;
        public event Action OnDirtThresholdReached;
        public event Action OnDirtEndReached;
        public event Action OnPigThresholdReached;
        public event Action OnPigDefeated;
        public event Action OnBigScoreThresholdReached;
        public event Action OnGameOver;

        public int Score => _score;

        private void Start()
        {
            // Randomize how long the dirty box phase lasts.
            _dirtEnd = _dirtThreshold + _dirtLengthAverage + UnityEngine.Random.Range(
                -_dirtLengthDeviation, _dirtLengthDeviation);
        }

        public void Scored()
        {
            _score++;
            OnScored?.Invoke(_score);

            if (_score == _boostThreshold)
            {
                OnBoostThresholdReached?.Invoke();
            }
            if (_score == _rewardThreshold)
            {
                OnRewardThresholdReached?.Invoke();
            }
            if (_score == _dirtThreshold)
            {
                OnDirtThresholdReached?.Invoke();
            }
            if (_score == _dirtEnd)
            {
                OnDirtEndReached?.Invoke();
            }
            if (_score == _pigThreshold)
            {
                OnPigThresholdReached?.Invoke();
            }
            if (_score == _bigScoreThreshold)
            {
                OnBigScoreThresholdReached?.Invoke();
            }
        }

        public void InvalidItemInHopper()
        {
            OnInvalidItem?.Invoke();
        }

        public void EmployeeClicked()
        {
            OnEmployeeClicked?.Invoke();
        }

        public void ComputerScreenClicked()
        {
            OnComputerScreenClicked?.Invoke();
        }

        public void PigDefeated()
        {
            if (_pigDefeated)
            {
                return;
            }

            _pigDefeated = true;
            OnPigDefeated?.Invoke();
        }

        public void GameOver()
        {
            if (_gameOver)
            {
                return;
            }

            _gameOver = true;
            OnGameOver?.Invoke();
        }
    }
}
