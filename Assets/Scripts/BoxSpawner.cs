using UnityEngine;

namespace BoxCorp
{
    public class BoxSpawner : MonoBehaviour
    {
        private enum DirtPhase : byte
        {
            None = 0,
            AllDirty = 1,
            SomeDirty = 2,
        }

        [Header("References")]
        [SerializeField] private GrabbablePool _pool;
        [SerializeField] private Transform _pigSpawnPoint;

        [Header("Spawn Force")]
        [SerializeField] private float _spawnForceMin = 5f;
        [SerializeField] private float _spawnForceMax = 10f;

        [Header("Spawn Chances")]
        [Range(0f, 1f), SerializeField] private float _bowlingBallChance = 0.02f;
        [Range(0f, 1f), SerializeField] private float _pigChance = 0.1f;
        [Range(0f, 1f), SerializeField] private float _dirtyChanceAfterDirtEnd = 0.1f;

        private DirtPhase _dirtPhase;
        private bool _bowlingBallsEnabled;
        private bool _forceBowlingBallNext;
        private bool _pigsEnabled;

        private void Start()
        {
            GameManager.Instance.OnRewardThresholdReached += HandleRewardThresholdReached;
            GameManager.Instance.OnDirtThresholdReached += HandleDirtThresholdReached;
            GameManager.Instance.OnDirtEndReached += HandleDirtEndReached;
            GameManager.Instance.OnPigDefeated += HandlePigDefeated;
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnRewardThresholdReached -= HandleRewardThresholdReached;
                GameManager.Instance.OnDirtThresholdReached -= HandleDirtThresholdReached;
                GameManager.Instance.OnDirtEndReached -= HandleDirtEndReached;
                GameManager.Instance.OnPigDefeated -= HandlePigDefeated;
            }
        }

        public void SpawnRequested()
        {
            if (WantsBowlingBall())
            {
                PhysicsGrabbable ball = _pool.SpawnBowlingBall(transform.position, transform.rotation);
                if (ball != null)
                {
                    Launch(ball);
                    return;
                }

                SpawnBox();
                return;
            }

            if (WantsPig())
            {
                PhysicsGrabbable pig = _pool.SpawnPig(_pigSpawnPoint.position, transform.rotation);
                if (pig != null)
                {
                    Launch(pig);
                    if (pig.TryGetComponent(out Pig pigController))
                    {
                        pigController.BeginAttack();
                    }
                    return;
                }

                SpawnBox();
                return;
            }

            SpawnBox();
        }

        private bool WantsBowlingBall()
        {
            if (!_bowlingBallsEnabled)
            {
                return false;
            }

            if (_forceBowlingBallNext)
            {
                _forceBowlingBallNext = false;
                return true;
            }

            return Random.value < _bowlingBallChance;
        }

        private bool WantsPig()
        {
            return _pigsEnabled && Random.value < _pigChance;
        }

        private PhysicsGrabbable SpawnBox()
        {
            PhysicsGrabbable grabbable = _pool.SpawnBox(transform.position, transform.rotation);
            Launch(grabbable);

            if (ShouldSpawnDirty())
            {
                grabbable.MakeDirty();
            }

            return grabbable;
        }

        private bool ShouldSpawnDirty()
        {
            return _dirtPhase switch
            {
                DirtPhase.AllDirty => true,
                DirtPhase.SomeDirty => Random.value < _dirtyChanceAfterDirtEnd,
                _ => false,
            };
        }

        private void Launch(PhysicsGrabbable grabbable)
        {
            grabbable.Rigidbody.AddForce(
                grabbable.Rigidbody.mass * Random.Range(_spawnForceMin, _spawnForceMax) * transform.forward,
                ForceMode.Impulse);
        }

        private void HandleRewardThresholdReached()
        {
            _bowlingBallsEnabled = true;
            _forceBowlingBallNext = true;
        }

        private void HandleDirtThresholdReached()
        {
            _dirtPhase = DirtPhase.AllDirty;
        }

        private void HandleDirtEndReached()
        {
            _dirtPhase = DirtPhase.SomeDirty;
        }

        private void HandlePigDefeated()
        {
            _pigsEnabled = true;
        }
    }
}
