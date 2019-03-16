using UnityEngine;
using AssetObjectsPacks;
using System;

[System.Serializable] public class Mover : MovementControllerComponent
{
    public bool allowStrafe;

    Vector3 destination, facePosition;
    Action onWaypointArrive;
    bool hasDestination, needsHelp;
    float arrivalThreshold { get { return behavior.arriveThresholds[movementController.speed]; } }
    float arrivalThreshold2 { get { return arrivalThreshold * arrivalThreshold; } }

    float helpArriveThreshold { get { return behavior.arriveHelpThresholds[movementController.speed]; } }
    float helpArriveThreshold2 { get { return helpArriveThreshold * helpArriveThreshold; } }


    public void SetFacePosition (Vector3 position) {
        facePosition = position;
    }
    public void FixedUpdate () {
        //CheckGrounded();
        UpdateLoop(AnimatorUpdateMode.AnimatePhysics, Time.fixedDeltaTime);
    }
    public void LateUpdate () {
        UpdateLoop(AnimatorUpdateMode.Normal, Time.deltaTime);
    }
void UpdateLoop (AnimatorUpdateMode checkMode, float deltaTime) {
        
        
        if (!eventPlayer.overrideMovement) {
            if (behavior.turnUpdate == checkMode) 
                Update(deltaTime);
            if (behavior.moveUpdate == checkMode) {
                Update(deltaTime);
            }
        }
    }
    
    void Update(float deltaTime)
    {
        if (hasDestination) {
            //if (movementController.movingFwd) {
                movementController.turner.AutoTurnerUpdate();
            //}

            //Debug.DrawLine(transform.position, destination, Color.red);

            float sqrDist = Vector3.SqrMagnitude(transform.position - destination);

        //trigger if we're above the arrival threshold
        hasDestination = sqrDist > arrivalThreshold2;

        //trigger help slerp if we're below the ehlpd thershold and above arrival
        needsHelp = !eventPlayer.overrideMovement && hasDestination && sqrDist <= helpArriveThreshold2;

        
            if (!hasDestination) {
                //hasDestination = false;
                //Debug.Log("arrived");
                movementController.turner.ForceEnd();
                if (onWaypointArrive != null) {
                    
                    onWaypointArrive();
                }
            }
            else {
                if (needsHelp) {
                    transform.position = Vector3.Lerp(transform.position, destination, deltaTime * behavior.moveHelpSpeeds[movementController.speed]);
                }
            }
        }
    }

    /*
        parameters:
            layer (internally set), cue, vector3 target, int speed (optional), int direction (optional, calculated if not there)

        makes the controller set up waypoint tracking to go to the specified position
        (cue's runtime interest position)

        the cue ends when the transform is within the arrive threshold
    */
    public void MovementControllerGoTo_Cue(object[] parameters) {
        Debug.Log("going to cue");
        int l = parameters.Length;

        int layerToUse = (int)parameters[0];
        InitializeMoveTo (
            (Vector3)parameters[2], 
            (l > 3) ? (int)parameters[3] : -1, 
            (MovementController.Direction)((l > 4) ? ((int)parameters[4]) : -1)
        );
        
        //so change doesnt register and override cue animation
        movementController.SetDirection(movementController.direction);
        movementController.SetSpeed(movementController.speed);


        if (hasDestination) {
            //take control of the players end event
            //to call it when arriving at waypoint
            onWaypointArrive = eventPlayer.OverrideEndEvent(layerToUse, null);

            // check if cue doesnt have any event specified for animations
            // if not then use the move controller's moves event
            MovementController.CheckForCueEventOverride(parameters, behavior.movesEvent, eventPlayer);
            
        }
        else {
            //animation threshold not met
            //skip playing any animation and end the cue/event right after
            eventPlayer.SkipPlay(layerToUse, MovementController.animationPackName);
            
            //movement within threshold, just end cue
            //cue duration = 0  so if above doesnt override it just skips
        }
    }

    

    public void GoTo (Vector3 newDestination, int newSpeed = -1, MovementController.Direction newDirection = MovementController.Direction.Calculated, Action onWaypointArrive = null) {
        //Debug.Log("going to manual");

        //so change doesnt register and override cue animation
        movementController.SetSpeed(movementController.CurrentSpeedOrDefaultSpeed(newSpeed));
        movementController.SetDirection(movementController.direction);

            

        Playlist.InitializePerformance(new Cue[] { behavior.wayPointCue }, new EventPlayer[] {eventPlayer}, newDestination, Quaternion.identity, false, 0, onWaypointArrive);
/*
        InitializeMoveTo (newDestination, newSpeed, newDirection);
        if (hasDestination) {
            this.onWaypointArrive = onWaypointArrive;
        }
        else {
            if (onWaypointArrive != null) {
                onWaypointArrive();
            }   
        }        
 */
    }

    void InitializeMoveTo (Vector3 newDestination, int newSpeed, MovementController.Direction newDirection) {

        float sqrDist = Vector3.SqrMagnitude(transform.position - newDestination);

        //trigger if we're above the arrival threshold
        hasDestination = sqrDist > arrivalThreshold2;

        //trigger help slerp if we're below the ehlpd thershold and above arrival
        needsHelp = hasDestination && sqrDist <= helpArriveThreshold2;


        if (hasDestination) {
            destination = newDestination;
            movementController.turner.SetTurnTarget(destination);
            
            movementController.speed = movementController.CurrentSpeedOrDefaultSpeed(newSpeed);
        
            //calculate movement direction (or set if newDirection is not calculated type)
            movementController.direction = newDirection == MovementController.Direction.Calculated ? CalculateDirection() : newDirection;
        }
    }


    MovementController.Direction CalculateDirection () {

        if (!allowStrafe) {
            return MovementController.Direction.Forward;
        }

        Vector3 myPos = transform.position;

        Vector3 startToDest = destination - myPos;
        
        //maybe return current direction (for no sudden changes)
        float threshold2 = behavior.minStrafeDistance * behavior.minStrafeDistance;
        if (startToDest.sqrMagnitude < threshold2) {
            return MovementController.Direction.Forward;
        }
        
        startToDest.y = 0;
        Vector3 midPoint = (myPos + destination) * .5f;
        Vector3 midToInterest = facePosition - midPoint;
        midToInterest.y = 0;

        float angle = Vector3.Angle(midToInterest, startToDest);

        /*
            ideal angle for look position is 90
            
            a -------------- b
                    /
                   /
                  /
            interest point
        */

        //angle is too acute or obtuse between interest (enemy point) and destination for strafing 
        //(backwards or forwards)
        if (angle <= 45 || angle >= 135) 
            return angle >= 135 ? MovementController.Direction.Forward : MovementController.Direction.Backwards;
        
        /*
              interest point
                  \
                   \
                    \
            a -------------- b
                    |
                    | <- startToDestPerp
                    |
        */
        Vector3 startToDestPerp = Vector3.Cross(startToDest.normalized, Vector3.up);

        angle = Vector3.Angle(startToDestPerp, midToInterest);

        return angle <= 45 ? MovementController.Direction.Right : MovementController.Direction.Left;
    }
}