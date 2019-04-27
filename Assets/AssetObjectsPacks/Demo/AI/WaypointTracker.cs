using UnityEngine;
using AssetObjectsPacks;
using System;


using Movement;

namespace Syd.AI {


[RequireComponent(typeof(Turner))]
public class WaypointTracker : MovementControllerComponent
{
    public Cue waypointCue;
    public bool hasDestination;
    Vector3 destination;
    EventPlayer.EventPlayEnder endEventPlayerPlay;
    Turner turner;
    
    protected override void Awake() {
        base.Awake();
        turner = GetComponent<Turner>();
    }

    public override void UpdateLoop (float deltaTime) {    
        if (hasDestination) {
            UpdateWaypointTracking(deltaTime);
        }
    }

    public void ManuallyTriggerWaypointArrival (string reason) {
        hasDestination = false;
        turner.doAutoTurn = false;

        if (endEventPlayerPlay != null) {
            
            endEventPlayerPlay.EndPlay("end waypoint " + reason);
            endEventPlayerPlay = null;        
        }
    }

    void HandleAutoTurner (float sqrDist) {
        //if we try and turn too close to the waypoint it winds up circling it
        float turnHelpMinDistance = behavior.waypointTurnHelpMinDistance * behavior.waypointTurnHelpMinDistance;
        turner.doAutoTurn = sqrDist >= turnHelpMinDistance;  
        
        //trying to animate turns too close to waypoint triggers too many within
        //a small amount of time
        float turnAnimMinDistance = behavior.waypointTurnAnimMinDistance * behavior.waypointTurnAnimMinDistance;
        turner.autoTurnAnimate = sqrDist >= turnAnimMinDistance;
    }
    bool CheckForArrival (float sqrDist) {
        float arrivalThreshold = behavior.arriveThresholds[controller.speed] * behavior.arriveThresholds[controller.speed];
        if (sqrDist <= arrivalThreshold) {
            ManuallyTriggerWaypointArrival("audto");
            return true;
        }
        return false;
    }
    void HandleArrivalHelp (float sqrDist, Vector3 myPos, float deltaTime) {
        float helpArriveThreshold = behavior.arriveHelpThresholds[controller.speed] * behavior.arriveHelpThresholds[controller.speed];
    
        //trigger help slerp if we're below the ehlpd thershold and above arrival
        if (sqrDist <= helpArriveThreshold) {
                
            float baseSpeed = behavior.moveHelpSpeeds[controller.speed];
            float maxSpeed = behavior.maxMoveHelpSpeed;
            
            //move faster towards destination closer you are
            float speed = Mathf.Lerp(baseSpeed, maxSpeed, 1 - (sqrDist / helpArriveThreshold));

            transform.position = Vector3.Lerp(myPos, destination, deltaTime * speed);
        }
    }

    void UpdateWaypointTracking(float deltaTime)
    {
        Vector3 myPos = transform.position;
        float sqrDist = Vector3.SqrMagnitude(myPos - destination);
        HandleAutoTurner(sqrDist);
        if (CheckForArrival(sqrDist)) {
            return;
        }
        if (!controller.overrideMovement) {
            HandleArrivalHelp(sqrDist, myPos, deltaTime);
        }
    }
    
    public void GoTo (Vector3 newDestination, Action onWaypointArrive = null) {
        Playlist.InitializePerformance("nav ai manual", waypointCue, eventPlayer, false, eventLayer, new MiniTransform(newDestination, Quaternion.identity), false, onWaypointArrive);
    }


    /*
        parameters:
            layer (internally set), vector3 target
            
        makes the controller set up waypoint tracking to go to the specified position
        (cue's runtime interest position)

        the cue ends when the transform is within the arrive threshold

        waypoint cue hiearchy:
            base
                start movement cue
                waypoint cue <-- cue calls method below

        so movement should already have been started by the time this method is called
    */
    void GoToWaypoint(object[] parameters) {

        Debug.Log("going to waypoint in tracker");
        int l = parameters.Length;

        //unpack parameters
        int layer = (int)parameters[0];
        Vector3 newDestination = (Vector3)parameters[1];

        
        float sqrDist = Vector3.SqrMagnitude(transform.position - newDestination);

        //trigger if we're above the arrival threshold
        float arrivalThreshold = behavior.arriveThresholds[controller.speed] * behavior.arriveThresholds[controller.speed];

        hasDestination = sqrDist > arrivalThreshold;
        destination = newDestination;

        if (hasDestination) {
            
            turner.SetTurnTarget(destination);

            //take control of the player's end play callback, to call it when arriving at waypoint
            endEventPlayerPlay = eventPlayer.OverrideEndPlay(layer, null, "Go to waypoint");


            Debug.Log("going to waypoint in tracker 2");
        
        }
        else {
            //movement within threshold, skip playing any animation and end the cue/event right after
            eventPlayer.SkipPlay(layer, MovementController.animationPackName);
        }
    }
}
}
