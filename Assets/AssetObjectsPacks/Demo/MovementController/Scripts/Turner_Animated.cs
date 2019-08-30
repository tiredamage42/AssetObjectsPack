

using UnityEngine;
using AssetObjectsPacks;
using System;

namespace Movement
{
    /*
        incorporate turning

        more advanced player utility, overrides cue ending when using in playlists

        turning is considered done afetr animation plays and slerp turns the rest of the way    
    */
    public class Turner_Animated : MovementControllerComponent
    {
        const string turnAngleName = "TurnAngle", turnRightName = "ToRight";
        public bool autoTurnAnimate;
        EventPlayer.EventPlayEnder endEventPlayerPlay;
        Turner turner;
        
        [HideInInspector] public int playerLayer;
        [HideInInspector] public bool suppliedEventsForTurn;

        protected override void Awake() {
            base.Awake();

            turner = GetComponent<Turner>();

            //add parameters checked by animation event set up to play
            //during the turn to cue
            eventPlayer.AddParameters(
                new CustomParameter[] {
                    new CustomParameter(turnAngleName, 0.0f),
                    new CustomParameter(turnRightName, false),
                }
            );
        }
        void OnEnable () {
            turner.onTurnStart += OnTurnerTurn;
        }
        void OnDisable () {
            turner.onTurnStart -= OnTurnerTurn;
        }

        public override void UpdateLoop (float deltaTime) {
            //if we're auto turning, and not animating, no need to use turn events
            if (turner.doAutoTurn && autoTurnAnimate) {
                turner.TurnToTarget(turner.turnTarget, OnEndTurn);
            }
        }    

        void OnEndTurn (bool immediate) {

            if (endEventPlayerPlay != null) {
                endEventPlayerPlay.EndPlay("turn");
                endEventPlayerPlay = null;    
            }
        }

        
        void OnTurnerTurn (Vector3 targetDirection, float angleWithMoveDirection) {
            int layerToUse = playerLayer;// suppliedEventsForTurn ? playerLayer : playerLayer+1;

            //take control of the players end play to call it when we're facing the target            
            endEventPlayerPlay = eventPlayer.OverrideEndPlay(layerToUse, OnEndPlayAttempt, "turning");

            /*
                below is only used if we're using animation events

                else:
                    eventPlayer.SkipPlay(layerToUse, MovementController.animationPackName);

                    if turner.disableSlerp == false:
                        turner is in charge of ending our turn
            */

            turner.disableSlerp = true;// inTurnAnimation;

            
            //set the angle parameter
            eventPlayer[turnAngleName].SetValue(angleWithMoveDirection);
            //check if turn is to right        
            eventPlayer[turnRightName].SetValue(Vector3.Angle(transform.right, targetDirection) <= 90);

            if (!suppliedEventsForTurn) {
                Playlist.InitializePerformance("Turn Default", behavior.defaultTurnEvents, eventPlayer, false, layerToUse, MiniTransform.zero, forceInterrupt : true, onEndPerformanceCallbacks : null) ;
            }
            
        }
        /*
            callback to give to the player to let us know when event is done playing
            
            success = wether or not an animation was found and played
        */
        void OnEndPlayAttempt (bool success) {
            turner.disableSlerp = false;
        }


        /*
            callback called by cue message

            parameters:
                layer (internally set), vector3 target, bool suppliedEventsForTurn
                
            makes the controller turn, so the forward (or move direction) faces the cue's runtime interest position

            the cue ends when this transform's forward (or move direction) is within the turn help angle 
            (relative to the direction towards the movement target)
        */
        void TurnTo (object[] parameters) {

            playerLayer = (int)parameters[0];
            suppliedEventsForTurn = (bool)parameters[2];
            
            turner.InitializeTurning((Vector3)parameters[1], OnEndTurn);
            
            // set back to default values
            playerLayer = eventLayer;
            //turning to manually, so cue doesnt supply custom events...
            suppliedEventsForTurn = false;
        }
                    
        
    }
}