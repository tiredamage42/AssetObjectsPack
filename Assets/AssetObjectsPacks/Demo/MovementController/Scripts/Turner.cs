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
    public bool doAutoTurn, checkDirectionChange;
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
        
        eventPlayer.AddParameters(
            new CustomParameter[] {
                new CustomParameter(turnAngleName, 0.0f),
                new CustomParameter(turnRightName, false),
            }
        );
    }
    /*
    Vector3 GetTargetLookDirection(Vector3 target) {
        Vector3 startToDest = target - transform.position;
        startToDest.y = 0;

        switch (controller.direction) {
            case MovementController.Direction.Forward: 
                return startToDest;
            case MovementController.Direction.Backwards: 
                return -startToDest;
            case MovementController.Direction.Left: 
                return -Vector3.Cross(startToDest.normalized, Vector3.up);
            case MovementController.Direction.Right: 
                return Vector3.Cross(startToDest.normalized, Vector3.up);        
        }
        return startToDest;
    }
     */
    
    void AutoTurnerUpdate () {
        TurnTo(turnTarget);
    }

    void UpdateLoop (AnimatorUpdateMode checkMode, float deltaTime) {
        if (behavior.turnUpdate == checkMode) {
            FinalizeTurnHelper(deltaTime);
            if (doAutoTurn) {
                AutoTurnerUpdate();
            }
        }
    }

    void LateUpdate () {
        UpdateLoop(AnimatorUpdateMode.Normal, Time.deltaTime);
    }
    void FixedUpdate () {
        UpdateLoop(AnimatorUpdateMode.AnimatePhysics, Time.fixedDeltaTime);
    }

    void OnEndTurn () {
        inTurnAnimation = false;
        isTurning = false;
        initializedTurnCue = false;
        
        endEventPlayerPlay.EndPlay();
        endEventPlayerPlay = null;    
    }


    public bool FacingTarget() {
        //if (doAutoTurn) {
        //    float turnAngleFwd = Vector3.Angle(transform.forward, targetDir);
            //deactivate turning (within threshold)
          //  return turnAngleFwd < behavior.turnAngleHelpThreshold;
        //}
        return !isTurning;

    }

    
    void FinalizeTurnHelper (float deltaTime) {

        //if (!initializedTurnCue) 
         //   return;
        
        if ((isTurning && initializedTurnCue) || doAutoTurn) {
            Vector3 targetDir = Movement.CalculateTargetFaceDirection(controller.direction, transform.position, turnTarget);
            //GetTargetLookDirection(turnTarget);
            
            if (isTurning && initializedTurnCue) {
                if (CheckForEndTurn(targetDir)) {
                    OnEndTurn();
                    Debug.Log("end turn");
                    //return;
                }
            }
            
        //}

        //if (isTurning || doAutoTurn) {

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
        //if (checkDirectionChange) {
            float angleFromLastAttempt = Vector3.Angle(targetDir, attemptTurnDir);
            if (angleFromLastAttempt > behavior.dirTurnChangeThreshold) {  
                Debug.Log("end turn angle too large");
                 
                return true;
            }
        //}

        //if we're done animating
        if (!inTurnAnimation) {
            float turnAngleFwd = Vector3.Angle(transform.forward, targetDir);
            //deactivate turning (within threshold)
            if (turnAngleFwd < behavior.turnAngleHelpThreshold){
                return true;
            }   
        }
        return false;
    }


    /*
        callback called by cue message

        parameters:
            layer (internally set), cue, vector3 target
            
        makes the controller turn, so the forward faces the cue's runtime interest position

        the cue ends when this transform's forward is within the turn help angle 
        (relative to the direction towards the movement target)
    */
    void TurnTo_Cue (object[] parameters) {
        
        int layer = (int)parameters[0];
        Cue cue = (Cue)parameters[1];
        turnTarget = (Vector3)parameters[2];



        Vector3 targetDir = Movement.CalculateTargetFaceDirection(controller.direction, transform.position, turnTarget);
        //Vector3 targetDir = GetTargetLookDirection(turnTarget);

        float angleFwd = Vector3.Angle(transform.forward, targetDir);

        bool skipAnim = controller.speed != 0 && (Vector3.SqrMagnitude(turnTarget - transform.position) < behavior.turnAnimMinDistance * behavior.turnAnimMinDistance);

        //trigger animation if angle is above anim threshold
        inTurnAnimation = angleFwd > behavior.animTurnAngleThreshold && !skipAnim;
        
        //if its below the animation threshold but above the help threshold
        isTurning = (inTurnAnimation || angleFwd > behavior.turnAngleHelpThreshold);

        if (isTurning) {
            attemptTurnDir = targetDir;
            initializedTurnCue = true;
        }
        //else {
        //    Debug.Log("no turn after initialize cue");
        //}
        
        if (inTurnAnimation) {
            //set the angle parameter
            eventPlayer[turnAngleName].SetValue(angleFwd);
            //check if turn is to right        
            eventPlayer[turnRightName].SetValue(Vector3.Angle(transform.right, targetDir) <= 90);

            // if cue doesn't have any animation events, override the player with this controller's events
            if (!cue.GetEventByName(MovementController.animationPackName)) {
                eventPlayer.OverrideEventToPlay(layer, behavior.turnsEvent);    
            }        
        }
        else {
            //animation threshold not met,
            //skip playing any animation and end the cue/event right after
            eventPlayer.SkipPlay(layer, MovementController.animationPackName);
        }

        if (isTurning) {
            //take control of the players end play to call it when we're facing the target
            endEventPlayerPlay = eventPlayer.OverrideEndPlay(layer, OnEndPlayAttempt, "turning");
        
            //if (doAutoTurn) {
            //    Debug.Log("initialized cue");
            //}
        }
                
    }

    bool overrideMovement { get { return isTurning || controller.overrideMovement; } }

    public void TurnTo (Vector3 target, System.Action onTurnSuccess = null) {

        if (!overrideMovement){
            
            bool skipAnim = doAutoTurn && controller.speed != 0 && (Vector3.SqrMagnitude(turnTarget - transform.position) < behavior.turnAnimMinDistance * behavior.turnAnimMinDistance);

            float threshold = doAutoTurn ? behavior.animTurnAngleThreshold : behavior.turnAngleHelpThreshold;
            if (!skipAnim && Vector3.Angle(transform.forward, Movement.CalculateTargetFaceDirection(controller.direction, transform.position, target)) > threshold) {


                if (doAutoTurn) {
                    Debug.Log("playing auto turn thing");
                }


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
        //Debug.Log("why no end play");
        inTurnAnimation = false;
    }
}