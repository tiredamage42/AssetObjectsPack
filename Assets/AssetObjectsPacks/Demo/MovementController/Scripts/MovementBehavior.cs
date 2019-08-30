using UnityEngine;

namespace Movement {

[CreateAssetMenu(fileName = "New Movement Behavior", menuName = "Movement/Behavior", order = 2)]
public class MovementBehavior : ScriptableObject
{
    public AssetObjectsPacks.Event[] defaultTurnEvents, stillEvents, moveEvents;


    public AssetObjectsPacks.CueBehavior turnCue, moveCue, stillCue;

    public float groundDistanceCheckAir = .01f;
    public float groundDistanceCheckGrounded = .25f;
    public float groundRadiusCheck = .1f;
    public LayerMask groundLayerMask;
    public float maxGroundAngle = 45;
    public float minYVelocity = -.125f;


    // [Header("Waypoint Tracking")]
    // public float[] arriveThresholds = new float[] { .1f, .25f, .5f };
    // public float[] arriveHelpThresholds = new float[] { 9999999f, .5f, 1f };
    // public float[] moveHelpSpeeds = new float[] { 5, 0, 0 };
    // public float maxMoveHelpSpeed = 3;
    // public float waypointTurnHelpMinDistance = .25f;
    // public float waypointTurnAnimMinDistance = 2.0f;

    
    [Header("Turning")]
    public float[] turnHelpSpeeds = new float[] { 1, 2.5f, 4 };
    public float dirTurnChangeThreshold = 45;
    // public float animTurnAngleThreshold = 22.5f;
    public float turnAngleHelpThreshold = 5.0f;
    
}
}

