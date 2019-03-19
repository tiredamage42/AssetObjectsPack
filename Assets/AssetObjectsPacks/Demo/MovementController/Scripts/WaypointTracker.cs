using UnityEngine;
using AssetObjectsPacks;
using System;

[RequireComponent(typeof(Turner))]
public class WaypointTracker : MovementControllerComponent
{
    public bool allowStrafe;

    Vector3 destination;//, facePosition;
    EventPlayer.EventPlayEnder endEventPlayerPlay;
    public bool hasDestination;
    float arrivalThreshold { get { return behavior.arriveThresholds[controller.speed]; } }
    float arrivalThreshold2 { get { return arrivalThreshold * arrivalThreshold; } }
    float helpArriveThreshold { get { return behavior.arriveHelpThresholds[controller.speed]; } }
    float helpArriveThreshold2 { get { return helpArriveThreshold * helpArriveThreshold; } }

    Turner turner;
    protected override void Awake() {
        base.Awake();
        turner = GetComponent<Turner>();
    }

    //public void SetFacePosition (Vector3 position) {
    //    facePosition = position;
    //}
    void FixedUpdate () {
        UpdateLoop(AnimatorUpdateMode.AnimatePhysics, Time.fixedDeltaTime);
    }
    void LateUpdate () {
        UpdateLoop(AnimatorUpdateMode.Normal, Time.deltaTime);
    }
    void UpdateLoop (AnimatorUpdateMode checkMode, float deltaTime) {    
        if (hasDestination) {
            if (behavior.moveUpdate == checkMode) {
                TrackWaypoint(deltaTime);
            }
        }
    }

    public void ManuallyTriggerWaypointArrival () {
        OnArrive();
            

    }

    void OnArrive () {
        if (hasDestination) {
            Debug.Log("arrive waypoint");
        }
        hasDestination = false;
        turner.doAutoTurn = false;
        //turner.Interrupt();
        if (endEventPlayerPlay != null) {
            endEventPlayerPlay.EndPlay();
            endEventPlayerPlay = null;        
        }
    }
    
    void TrackWaypoint(float deltaTime)
    {
        if (!controller.overrideMovement) {

            float sqrDist = Vector3.SqrMagnitude(transform.position - destination);
                    
            turner.doAutoTurn = sqrDist >= behavior.turnHelpMinDistance * behavior.turnHelpMinDistance;  
            
            Debug.DrawLine(transform.position + Vector3.up, destination + Vector3.up, Color.red);
            
            if (sqrDist <= arrivalThreshold2) {
                OnArrive();
                return;
            }
            if (!controller.overrideMovement) {
                //trigger help slerp if we're below the ehlpd thershold and above arrival
                if (sqrDist <= helpArriveThreshold2) {
                    
                    float baseSpeed = behavior.moveHelpSpeeds[controller.speed];
                    float maxSpeed = behavior.maxMoveHelpSpeed;
                    
                    //move faster towards destination closer you are
                    float speed = Mathf.Lerp(baseSpeed, maxSpeed, 1 - (sqrDist / helpArriveThreshold2));

                    transform.position = Vector3.Lerp(transform.position, destination, deltaTime * speed);
                }
            }
        }



    }
    //public void GoTo (Vector3 newDestination, MovementController.Direction newDirection = MovementController.Direction.Calculated, Action onWaypointArrive = null) {
    public void GoTo (Vector3 newDestination, Action onWaypointArrive = null) {
        
        //figure out manual direction set (cue automatically calculates)
        //controller.SetDirection(newDirection);


        Playlist.InitializePerformance("nav ai manual", behavior.wayPointCue, eventPlayer, false, eventLayer, newDestination, Quaternion.identity, true, onWaypointArrive);
    }


    /*
        parameters:
            layer (internally set), vector3 target, int speed (optional), int direction (optional, calculated if not there)

        makes the controller set up waypoint tracking to go to the specified position
        (cue's runtime interest position)

        the cue ends when the transform is within the arrive threshold
    */


    /*
        start movement cue
        waypoint cue

        so movement should already have been started
    */
    public void GoToWaypoint_Cue(object[] parameters) {
        int l = parameters.Length;

        //unpack parameters
        int layer = (int)parameters[0];
        Vector3 newDestination = (Vector3)parameters[2];
        
        //int newSpeed = (l > 3) ? (int)parameters[3] : -1;
        //MovementController.Direction newDirection = (MovementController.Direction)((l > 4) ? ((int)parameters[4]) : -1);


        float sqrDist = Vector3.SqrMagnitude(transform.position - newDestination);
        //trigger if we're above the arrival threshold
        hasDestination = sqrDist > arrivalThreshold2;

        if (hasDestination) {
            destination = newDestination;
            
            turner.SetTurnTarget(destination);
            
            //calculate movement direction (or set if newDirection is not calculated type)
            //controller.SetDirection(newDirection == MovementController.Direction.Calculated ? CalculateDirection() : newDirection, false);

            //take control of the player's end play callback, to call it when arriving at waypoint
            endEventPlayerPlay = eventPlayer.OverrideEndPlay(layer, null, "Go to waypoint");
        }
        else {
            Debug.Log("skipping within threshold");
            //movement within threshold, skip playing any animation and end the cue/event right after
            eventPlayer.SkipPlay(layer, MovementController.animationPackName);
        }
    }
    /*
    MovementController.Direction CalculateDirection () {

        if (!allowStrafe) {
            return MovementController.Direction.Forward;
        }

        Vector3 a = transform.position;
        Vector3 b = destination;
        Vector3 c = facePosition;

        Vector3 a2b = b - a;
        
        //maybe return current direction (for no sudden changes)
        float threshold = behavior.minStrafeDistance * behavior.minStrafeDistance;
        if (a2b.sqrMagnitude < threshold) {
            return MovementController.Direction.Forward;
        }
        
        a2b.y = 0;
        Vector3 midPoint = (a + b) * .5f;
        Vector3 mid2C = c - midPoint;
        mid2C.y = 0;
        
        float angle = Vector3.Angle(mid2C, a2b);

        /
            ideal angle for look position is 90
            A -------------- B
                    /
                   /
                  C
        /
        //angle is too acute or obtuse between face ("enemy" point) and destination for strafing 
        //(backwards or forwards)
        if (angle <= 45 || angle >= 135) {
            return angle >= 135 ? MovementController.Direction.Forward : MovementController.Direction.Backwards;
        }
        /
                  C
                   \
                    \
            A -------------- B
                     |
                     | <- a2bPerp
        /
        Vector3 a2bPerp = Vector3.Cross(a2b.normalized, Vector3.up);

        return Vector3.Angle(a2bPerp, mid2C) <= 45 ? MovementController.Direction.Right : MovementController.Direction.Left;
    }
    */
}