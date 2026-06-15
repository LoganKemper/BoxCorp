using UnityEngine;

namespace BoxCorp
{
    public class ComputerScreenClickable : EffectsOnClick
    {
        [SerializeField] private ParticleSystem _sparksParticles;
        [SerializeField] private float _particlesZOffset = -1f;

        protected override void HandleClicked(Vector3 clickPoint)
        {
            base.HandleClicked(clickPoint);

            GameManager.Instance.ComputerScreenClicked();
            _sparksParticles.transform.position = clickPoint + new Vector3(0f, 0f, _particlesZOffset);
            _sparksParticles.Play();
        }
    }
}
