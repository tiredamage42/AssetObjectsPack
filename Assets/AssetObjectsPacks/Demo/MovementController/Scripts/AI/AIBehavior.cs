//using System.Collections;
//using System.Collections.Generic;
using UnityEngine;

using UnityEngine;

[CreateAssetMenu(fileName = "New AI Behavior", menuName = "AI/Behavior", order = 2)]
public class AIBehavior : ScriptableObject
{
    public float minStrafeDistance = 1;
    public AssetObjectsPacks.Cue navigateToCue;

    public float platformEndWaypointTriggerRadius = 5;
}

