using UnityEngine;
using AssetObjectsPacks;
using System;

/* 
    Incorporate a simple one shot jump animation
*/
public class Jumper : MovementControllerComponent
{
    bool overrideMovement { get { return !controller.grounded || controller.overrideMovement; } }
    
    
    /*
        callback called by cue message

        parameters:
            layer (internally set), cue, 
            
        makes the controller jump
    */
    void Jump_Cue (object[] parameters) {
        int layer = (int)parameters[0];
        Cue cue = (Cue)parameters[1];
        // if cue doesn't have any animation events, override the player with this controller's events
        if (!cue.GetEventByName(MovementController.animationPackName)) {
            eventPlayer.OverrideEventToPlay(layer, behavior.jumpsEvent);    
        }
    }

    public void Jump (Action onJumpDone = null) {
        if (overrideMovement) return;
        Playlist.InitializePerformance("jumper", behavior.jumpCue, eventPlayer, false, eventLayer, Vector3.zero, Quaternion.identity, true, onJumpDone);
    }
}