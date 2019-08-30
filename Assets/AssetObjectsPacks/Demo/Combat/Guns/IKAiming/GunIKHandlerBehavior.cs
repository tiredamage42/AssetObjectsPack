using UnityEngine;

namespace Game.Combat {

    // TODO: replace with transform behavior...

    [CreateAssetMenu(fileName = "New IK Handler Behavior", menuName = "Combat/IK Handler Behavior", order = 2)]
    public class GunIKHandlerBehavior : ScriptableObject
    {
        public Vector3 localAimHeadPos;
        public Vector3 localHipHandPos, localHipHandRot;
    }
}