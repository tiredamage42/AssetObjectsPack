

using UnityEngine;
using AssetObjectsPacks;
using System;

using Movement;

namespace Game.AI {


[RequireComponent(typeof(WaypointTracker))]
public class WaypointTracker_Events : MonoBehaviour// MovementControllerComponent
{
    WaypointTracker waypointTracker;
    EventPlayer.EventPlayEnder waypoint_endEventPlayerPlay, nav_endEventPlayerPlay;
    
    int workingLayer;

    // MovementController controller;
    EventPlayer eventPlayer;
    
    // protected override 
    void Awake() {
        // base.Awake();
        // controller = GetComponent<MovementController>();
        eventPlayer = GetComponent<EventPlayer>();
    
        waypointTracker = GetComponent<WaypointTracker>();
    }

    

    void OnWaypointArrive (bool immediate) {
        if (immediate) {
            eventPlayer.SkipPlay(workingLayer, MovementController.animationPackName);
        }
        else {
            if (waypoint_endEventPlayerPlay != null) {
                waypoint_endEventPlayerPlay.EndPlay("end waypoint ");// + reason);
                waypoint_endEventPlayerPlay = null;        
            }
        }

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

        Debug.Log("going to waypoint in tracker event");
        int l = parameters.Length;

        //unpack parameters
        // int layer 
        workingLayer = (int)parameters[0];


        waypointTracker.GoToWaypointManual((Vector3)parameters[1], OnWaypointArrive);

        if (waypointTracker.hasDestination) {
            //take control of the player's end play callback, to call it when arriving at waypoint
            waypoint_endEventPlayerPlay = eventPlayer.OverrideEndPlay(workingLayer, null, "Go to waypoint");
            Debug.Log("going to waypoint in tracker 2");
        }
        // else {
        //     //movement within threshold, skip playing any animation and end the cue/event right after
        //     eventPlayer.SkipPlay(workingLayer, MovementController.animationPackName);
        // }
    }


    
        /*
            parameters:
                layer (internally set), vector3 target
        */
        
        void NavigateTo(object[] parameters) {

            
            // Debug.Log("navigating to");
            //unpack parameters
            int layer = (int)parameters[0];
            // destination = (Vector3)parameters[1];
            
            //take control of the player's end play callback, to call it when arriving at destination
            nav_endEventPlayerPlay = eventPlayer.OverrideEndPlay(layer, null, "pathfinder");


            GetComponent<AIMovement>().NavigateToManual((Vector3)parameters[1], OnDestinationArrive);
            
            //calculate path
            // agent.nextPosition = transform.position;
            // agent.SetDestination(destination);
            
            // StartCoroutine(WaitForPathCalculation());
        }

        void OnDestinationArrive () {
            nav_endEventPlayerPlay.EndPlay("end nav");
            nav_endEventPlayerPlay = null;   
            
        }

}
}
