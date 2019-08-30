using UnityEngine;

namespace Game.AI {

    [CreateAssetMenu(fileName = "New AI Behavior", menuName = "AI/Behavior", order = 2)]
    public class AIBehavior : ScriptableObject
    {
        public AssetObjectsPacks.Cue navigateToCue;
        public float minStrafeDistance = 1;
        public float platformEndWaypointTriggerRadius = 5;

    }
}

