using UnityEngine;

namespace BoxCorp
{
    [CreateAssetMenu(fileName = "UI Animation Data", menuName = "UI Animation Data")]
    public class UIAnimationData : ScriptableObject
    {
        public float duration = 0.5f;
        public float scaleAmount = 1.2f;
        public float shakeAngle = 15f;
        public AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        public AnimationCurve rotationCurve = AnimationCurve.EaseInOut(0, 0, 1, 0);
    }
}