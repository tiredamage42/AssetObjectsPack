using UnityEngine;

[CreateAssetMenu(fileName = "New Movement Behavior", menuName = "Movement/Behavior", order = 2)]
public class MovementBehavior : ScriptableObject
{
    public AssetObjectsPacks.Cue turnCue, wayPointCue, moveCue, stillCue;

    public AssetObjectsPacks.Cue platformUpCueShort, platformDownCueShort;
    public AssetObjectsPacks.Cue platformUpCueTall, platformDownCueTall;

    public AssetObjectsPacks.Event turnsEvent;
    public AssetObjectsPacks.Event jumpsEvent;
    public AssetObjectsPacks.Event movesEvent;
    public AssetObjectsPacks.Event stillsEvent;

    public AnimatorUpdateMode turnUpdate = AnimatorUpdateMode.Normal;
    public AnimatorUpdateMode moveUpdate = AnimatorUpdateMode.Normal;

    public float groundDistanceCheckAir = .01f;
    public float groundDistanceCheckGrounded = .25f;
    public float groundRadiusCheck = .1f;
    public LayerMask groundLayerMask;
    public float maxGroundAngle = 45;
    public float minYVelocity = -.125f;

    [Header("Waypoint Tracking")]
    public float[] arriveThresholds = new float[] { .1f, .15f, .2f };
    public float[] arriveHelpThresholds = new float[] { .1f, .15f, .2f };
    public float[] moveHelpSpeeds = new float[] { 1, 2.5f, 4 };


    [Header("Turning")]
    public float[] turnHelpSpeeds = new float[] { 1, 2.5f, 4 };
    public float dirTurnChangeThreshold = 45;
    public float animTurnAngleThreshold = 22.5f;
    public float turnAngleHelpThreshold = 5.0f;
    public float minStrafeDistance = 1;

    [Header("Platforming")]
    public float platformAngleThreshold = 45;
    public LayerMask platformLayerMask;
    public float platformUpDistanceAheadCheck = 1.0f;
    public float platformRadiusCheck = .05f;
    public float platformDownDistanceAheadCheck = .35f;
    
}
