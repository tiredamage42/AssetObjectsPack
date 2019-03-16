using UnityEngine;
using AssetObjectsPacks;
using System;

/* 
    Incorporate a simple one shot jump animation
*/
[System.Serializable] public class Jumper : MovementControllerComponent
{
    public bool isJumping;
    Action onJumpDone;

    /*
        callback to give to the player to let us know when 
        event is done playing
        
        success = wether or not an animation was found and played
    */
    void OnEndPlay (bool success) {
        isJumping = false;

        //enable character controller
        movementController.rootMotion.EnablePhysics(true);

        //call calback if any
        if (onJumpDone != null) {
            onJumpDone();
            onJumpDone = null;
        }
    }
    public void Jump (System.Action onJumpDone = null) {
        if (!isJumping && movementController.grounded) {
            //disable physics characer controller
            movementController.rootMotion.EnablePhysics(false);


            //dont interrupt playlists
            int playerLayerToUse = 0; 
            //give duration control to the event
            float duration = -1;
            //will interrupt anyways, since it shouldnt be a loop
            bool asInterrupter = true; 
            
            //call OnEndPlay when player is done playing event
            eventPlayer.SubscribeToPlayEnd(playerLayerToUse, OnEndPlay);
            //play event
            eventPlayer.PlayEvent(playerLayerToUse, behavior.jumpsEvent, duration, asInterrupter);
            
            //set up the callback
            this.onJumpDone = onJumpDone;   

            isJumping = true;
        }
    }
}