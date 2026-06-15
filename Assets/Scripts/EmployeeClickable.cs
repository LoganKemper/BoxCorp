using UnityEngine;

namespace BoxCorp
{
    public class EmployeeClickable : EffectsOnClick
    {
        protected override void HandleClicked(Vector3 clickPoint)
        {
            base.HandleClicked(clickPoint);

            ParticlePool.Instance.SpawnSmallDust(clickPoint);
            GameManager.Instance.EmployeeClicked();
        }
    }
}
