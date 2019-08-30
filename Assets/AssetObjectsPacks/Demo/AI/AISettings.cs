// using System.Collections;
// using System.Collections.Generic;
using UnityEngine;

namespace Game.AI {

        
    [CreateAssetMenu()]
    public class AISettings : GameSettings
    {

        [Header("Waypoint Tracking")]
        public float[] arriveThresholds = new float[] { .1f, .25f, .5f };
        public float[] arriveHelpThresholds = new float[] { 9999999f, .5f, 1f };
        public float[] moveHelpSpeeds = new float[] { 5, 0, 0 };
        public float maxMoveHelpSpeed = 3;
        public float waypointTurnHelpMinDistance = .25f;
        public float waypointTurnAnimMinDistance = 2.0f;
        
       
    }
}

