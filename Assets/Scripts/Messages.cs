using UnityEngine;

namespace BoxCorp
{
    [CreateAssetMenu(fileName = "Messages", menuName = "Messages")]
    public class Messages : ScriptableObject
    {
        [Header("Computer Screen Messages")]
        public string scoreMessage;
        public string[] altScoreMessages;
        public string invalidMessage;
        public string idleMessage;
        public string[] ticklingMessages;
        public string stopTouchingMessage;

        [Header("UI Messages")]
        public string scorePrefix;
        public string boosting;
        public string bowling;
        public string dirt;
        public string pigSpawned;
        public string pigDefeated;
        public string bigScore;
    }
}
