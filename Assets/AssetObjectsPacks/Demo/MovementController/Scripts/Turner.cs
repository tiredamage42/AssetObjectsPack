using UnityEngine;
using AssetObjectsPacks;
using System;

/*
    incorporate turning

    more advanced player utility, overrides cue ending when using in playlists

    turning is considered done afetr animation plays and slerp turns the rest of the way    
*/
public class Turner : MovementControllerComponent
{
    public bool doAutoTurn, autoTurnAnimate;
    public bool use2d = true;
    public bool disableAnim;


    public bool isTurning, inTurnAnimation, initializedTurnCue;
    EventPlayer.EventPlayEnder endEventPlayerPlay;
    Vector3 attemptTurnDir, turnTarget;    
    const string turnAngleName = "TurnAngle", turnRightName = "ToRight";

    void OnDrawGizmos()
    {
        if (isTurning || doAutoTurn) {
            Gizmos.color = inTurnAnimation ? Color.green : Color.white;
            Gizmos.DrawWireSphere(transform.position + Vector3.up * 2, .05f);
            Gizmos.DrawWireSphere(turnTarget, .05f);
        }
    }

    public void SetTurnTarget (Vector3 target) {
        turnTarget = target;
    }

    protected override void Awake() {
        base.Awake();
        
        //char specific

        //add parameters checked by animation event set up to play
        //during the turn to cue
        eventPlayer.AddParameters(
            new CustomParameter[] {
                new CustomParameter(turnAngleName, 0.0f),
                new CustomParameter(turnRightName, false),
            }
        );
    }
    

    public override void UpdateLoop (float deltaTime) {
        FinalizeTurnHelper(deltaTime);

        if (doAutoTurn) {
            AutoTurnerUpdate();
        }

    }    
    void AutoTurnerUpdate () {

        //if we're auto turning, and not animating, no need to use turn events
        if (autoTurnAnimate) {
            //if we're auto turning, we only call events when needing animations
            //so use anim turn threshold
            TurnTo(turnTarget, behavior.animTurnAngleThreshold, null);
        }
    }

    void OnEndTurn () {
        inTurnAnimation = false;
        isTurning = false;
        initializedTurnCue = false;
        
        endEventPlayerPlay.EndPlay("turn");
        endEventPlayerPlay = null;    
    }

    
    void FinalizeTurnHelper (float deltaTime) {

        bool doingManualTurn = isTurning && initializedTurnCue;

        if (doingManualTurn || doAutoTurn) {
            Vector3 targetDir = Movement.CalculateTargetFaceDirection(controller.direction, transform.position, turnTarget, use2d);
            
            if (doingManualTurn) {
                if (CheckForEndTurn(targetDir)) {
                    OnEndTurn();
                }
            }
        
            //if we're turning and animation is done, slerp to reach face target
            if (!inTurnAnimation) {
                if (!controller.overrideMovement) {
                    transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(targetDir), deltaTime * behavior.turnHelpSpeeds[controller.speed]);
                }
            }
        }
    }

    bool CheckForEndTurn (Vector3 targetDir) {
        
        //if direction has changed too much from last attempt
        //end the turn (retries if above threshold and auto turning)
        float angleFromLastAttempt = Vector3.Angle(targetDir, attemptTurnDir);
        if (angleFromLastAttempt > behavior.dirTurnChangeThreshold) {  
            return true;
        }
        
        //if we're done animating
        if (!inTurnAnimation) {
            //check movement direction (or forward) angle with target direction
            float angleWMoveDir = Vector3.Angle(controller.moveDireciton, targetDir);                

            //deactivate turning (within threshold)
            if (angleWMoveDir < behavior.turnAngleHelpThreshold){
                return true;
            }   
        }
        return false;
    }


    /*
        callback called by cue message

        parameters:
            layer (internally set), cue, vector3 target
            
        makes the controller turn, so the forward (or move direction) faces the cue's runtime interest position

        the cue ends when this transform's forward (or move direction) is within the turn help angle 
        (relative to the direction towards the movement target)
    */
    void TurnTo_Cue (object[] parameters) {
        
        int layer = (int)parameters[0];
        Cue cue = (Cue)parameters[1];
        turnTarget = (Vector3)parameters[2];


        Vector3 targetDir = Movement.CalculateTargetFaceDirection(controller.direction, transform.position, turnTarget, use2d);
        
        //check movement direction (or forward) angle with target direction
        float angleWMoveDir = Vector3.Angle(controller.moveDireciton, targetDir);                

        //bool skipAnim = doAutoTurn && !autoTurnAnimate;  

        //trigger animation if angle is above anim threshold
        inTurnAnimation = angleWMoveDir > behavior.animTurnAngleThreshold;// && !skipAnim;
        
        //if its below the animation threshold but above the help threshold
        isTurning = inTurnAnimation || angleWMoveDir > behavior.turnAngleHelpThreshold;

        if (isTurning) {
            attemptTurnDir = targetDir;
            initializedTurnCue = true;
        }
        
        if (inTurnAnimation) {


            //set the angle parameter
            eventPlayer[turnAngleName].SetValue(angleWMoveDir);
            //check if turn is to right        
            eventPlayer[turnRightName].SetValue(Vector3.Angle(transform.right, targetDir) <= 90);
   
        }
        else {
            //animation threshold not met,
            //skip playing any animation and end the cue/event right after
            eventPlayer.SkipPlay(layer, MovementController.animationPackName);
        }

        if (isTurning) {
            //take control of the players end play to call it when we're facing the target
            endEventPlayerPlay = eventPlayer.OverrideEndPlay(layer, OnEndPlayAttempt, "turning");
        }
                
    }

    bool overrideMovement { get { return isTurning || controller.overrideMovement; } }
    public void TurnTo (Vector3 target, Action onTurnSuccess = null) {
        TurnTo(target, behavior.turnAngleHelpThreshold, onTurnSuccess);
    }
    void TurnTo (Vector3 target, float threshold, Action onTurnSuccess = null) {

        if (!overrideMovement){
                
            //calculate the target direction
            Vector3 targetDir = Movement.CalculateTargetFaceDirection(controller.direction, transform.position, target, use2d);

            //check movement direction (or forward) angle with target direction
            float angleWMoveDir = Vector3.Angle(controller.moveDireciton, targetDir);
            
            //if that angle is above our turn threshold:
            if (angleWMoveDir > threshold) {

                isTurning = true;                
                initializedTurnCue = false;

                Playlist.InitializePerformance("turning", behavior.turnCue, eventPlayer, false, eventLayer, target, Quaternion.identity, false, onTurnSuccess);
            }
        }
    }

    /*
        callback to give to the player to let us know when 
        event is done playing
        
        success = wether or not an animation was found and played
    */
    void OnEndPlayAttempt (bool success) {
        inTurnAnimation = false;
    }
}