// using System.Collections;
// using System.Collections.Generic;
using UnityEngine;

using Movement;
namespace AssetObjectsPacks {
    public partial class EventMessagesListener : MonoBehaviour
    {

        Turner_Animated turnerAnimated;
        Turner turner;
        /*
            callback called by cue message

            parameters:
                layer (internally set), vector3 target
                
            makes the controller turn, so the forward (or move direction) faces the cue's runtime interest position

            the cue ends when this transform's forward (or move direction) is within the turn help angle 
            (relative to the direction towards the movement target)
        */
        // void TurnTo (object[] parameters) {
        //     if (turner == null) turner = GetComponent<Turner>();
        //     if (turner == null) {
        //         Debug.LogError("Cant use message TurnTo, turner is null");
        //         return;
        //     }
            
        //     if (turnerAnimated == null) 
        //         turnerAnimated = GetComponent<Turner_Animated>();
            
        //     if (turnerAnimated != null) {
        //         turnerAnimated.playerLayer = (int)parameters[0];
        //         turnerAnimated.suppliedEventsForTurn = (bool)parameters[2];
        //     }
            
            
        //     // Debug.LogError("turning from cue on layer " + playerLayer);

        //     turner.InitializeTurning((Vector3)parameters[1], turnerAnimated.OnEndTurn);


            
        // }
              
        
    }
}
