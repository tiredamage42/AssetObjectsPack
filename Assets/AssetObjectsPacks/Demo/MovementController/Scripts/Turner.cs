using UnityEngine;
using AssetObjectsPacks;
using System;

/*
    incorporate turning

    more advanced player utility, overrides cue ending when using in playlists

    turning is considered done afetr animation plays and slerp turns the rest of the way    
*/
[System.Serializable] public class Turner : MovementControllerComponent
{
    
    public bool doAutoTurn;

    #if UNITY_EDITOR
    public void OnDrawGizmos()
    {
        if (isTurning) {
            Gizmos.color = Color.white;

            Gizmos.DrawWireSphere(transform.position + Vector3.up * 2, .05f);
        }

    }
 
    #endif


    public void ForceEnd () {
        isTurning = inTurnAnimation = false;
        onTurnEnd = null;
    }





    public bool isTurning, inTurnAnimation;
    Action onTurnEnd;
    Vector3 attemptTurnDir, turnTarget;    
    const string turnAngleName = "TurnAngle", turnRightName = "ToRight";

    public void SetTurnTarget (Vector3 target) {
        turnTarget = target;
    }
    public void Initialize (MovementController movementController) {
        this.movementController = movementController;
        eventPlayer.AddParameters(
            new CustomParameter[] {
                new CustomParameter(turnAngleName, 0.0f),
                new CustomParameter(turnRightName, false),
            }
        );

    }
    
    Vector3 GetTargetLookDirection(Vector3 target) {
        Vector3 startToDest = target - transform.position;
        startToDest.y = 0;

        switch (movementController.direction) {
            case MovementController.Direction.Forward: return startToDest;
            case MovementController.Direction.Backwards: return -startToDest;
            case MovementController.Direction.Left: return -Vector3.Cross(startToDest.normalized, Vector3.up);
            case MovementController.Direction.Right: return Vector3.Cross(startToDest.normalized, Vector3.up);        
        }
        return startToDest;
    }
    
    public void AutoTurnerUpdate () {
        TurnTo(turnTarget, false);
    }



    public void Update() {
    //    if (doAutoTurn) {
     //       AutoTurnerUpdate();
     //   }
    }
    public void LateUpdate () {
        if (behavior.turnUpdate == AnimatorUpdateMode.Normal) {
            FinalizeTurnHelper(Time.deltaTime);
            if (doAutoTurn) {
                AutoTurnerUpdate();
            }
        }
    }
    public void FixedUpdate () {
        if (behavior.turnUpdate == AnimatorUpdateMode.AnimatePhysics) {
            FinalizeTurnHelper(Time.fixedDeltaTime);
            if (doAutoTurn) {
                AutoTurnerUpdate();
            }
        }
    }

    public void InterruptTurn () {
        isTurning = false;
        inTurnAnimation = false;
        if (onTurnEnd != null) {
            onTurnEnd();
        }
    }


    void FinalizeTurnHelper (float deltaTime) {
        Vector3 targetDir = Vector3.zero;
        if (isTurning) {
            targetDir = GetTargetLookDirection(turnTarget);

            if (CheckForEndTurn(targetDir)) {
                //Debug.Log("DONE");
                isTurning = false;
                inTurnAnimation = false;

                if (onTurnEnd != null) {
                    //Debug.Log("ending turn callback " + onTurnEnd.ToString());
                    onTurnEnd();
                }
            }
        }
        
        if (!eventPlayer.overrideMovement) {
            //if we're turning and animation is done, slerp to reach face target
            if (isTurning && !inTurnAnimation) {
                //Debug.Log("Turning!");
                
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(targetDir), deltaTime * behavior.turnHelpSpeeds[movementController.speed]);
            }
        }
    }

    bool CheckForEndTurn (Vector3 targetDir) {
        
        //if direction has changed too much from last attempt
        //end the turn (retries if above threshold and auto turning)
        float angleFromLastAttempt = Vector3.Angle(targetDir, attemptTurnDir);
        if (angleFromLastAttempt > behavior.dirTurnChangeThreshold) {   
            //Debug.Log("end turn cause angle change too large");
            return true;
        }



        //if we're done animating
        if (!inTurnAnimation) {
            float turnAngleFwd = Vector3.Angle(transform.forward, targetDir);
            //deactivate turning (within threshold)
            if (turnAngleFwd < behavior.turnAngleHelpThreshold){
                //Debug.Log("end turn success");
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
    public void MovementControllerTurnTo_Cue (object[] parameters) {
        InitializeTurn ((Vector3)parameters[2], false);

        int layerToUse = (int)parameters[0];

        if (inTurnAnimation) {
            MovementController.CheckForCueEventOverride(parameters, behavior.turnsEvent, eventPlayer);

            // call OnEndPlay when player is done
            //eventPlayer.SubscribeToPlayEnd(layerToUse, OnEndPlay);
        }
        else {
            //animation threshold not met
            //skip playing any animation and end the cue/event right after
            eventPlayer.SkipPlay(layerToUse, MovementController.animationPackName);
        }

        if (isTurning) {
            //take control of the players end event
            //to call it when we're facing the movement target
            onTurnEnd = eventPlayer.OverrideEndEvent(layerToUse, OnEndPlay);
            //Debug.Log("set end turn as event player end");
        }
        //else movement within threshold, just end cue
        //cue duration = 0  so if above doesnt override it just skips
    }

    public void TurnTo (Vector3 target, bool interruptPlaylists, System.Action onTurnSuccess = null) {



        turnTarget = target;

        if (!isTurning && !eventPlayer.overrideMovement){  
            bool skipAnim = movementController.jumper.isJumping;
            

            
        Vector3 targetDir = GetTargetLookDirection(target);
        float angleFwd = Vector3.Angle(transform.forward, targetDir);

        //trigger animation if angle is above anim threshold
        bool needsAnim = angleFwd > behavior.animTurnAngleThreshold && !skipAnim;
        
        
        //if its below the animation threshold but above the help threshold
        bool needsTurn = needsAnim || angleFwd > behavior.turnAngleHelpThreshold;
            if (needsTurn) {
                isTurning = true;
                attemptTurnDir = targetDir;
                    
            }
    
            //if (needsAnim) {
            if (needsAnim){

                Debug.Log("need anim turn");

                Playlist.InitializePerformance(new Cue[] { behavior.turnCue }, new EventPlayer[] {eventPlayer}, target, Quaternion.identity, false, 0, onTurnSuccess);
            }
            else {
                //just do slerp turn
                if (needsTurn) {
                    //Debug.Log(needsAnim + " needs anim");
                    //Debug.Log(angleFwd + " is above " + behavior.turnAngleHelpThreshold);

                    //isTurning = true;
                    //attemptTurnDir = targetDir;
                    //Debug.Log("set turning true here");

                }

            }
            
/*
            bool skipAnim = movementController.jumper.isJumping;
            InitializeTurn(target, skipAnim);

            if (inTurnAnimation) {

                int playerLayerToUse = interruptPlaylists ? -1 : 0;

                //call OnEndPlay when player is done playing event
                eventPlayer.SubscribeToPlayEnd(playerLayerToUse, OnEndPlay);

                //give duration control to event and player
                float duration = -1;
                //will interrupt anyways since not loop
                bool interrupt = true;
                //manually play event
                eventPlayer.PlayEvent(playerLayerToUse, behavior.turnsEvent, duration, interrupt);
            }
            if (isTurning) {
                //set up the callback
                onTurnEnd = onTurnSuccess;
            }
            else {
                //if we're within the threshold skip everything and just call the callback
                if (onTurnSuccess != null) {
                    onTurnSuccess();
                }
            }
*/
        }
    }

    /*
        callback to give to the player to let us know when 
        event is done playing
        
        success = wether or not an animation was found and played
    */
    void OnEndPlay (bool success) {
        inTurnAnimation = false;
        //Debug.Log("done with animation");
    }

    void InitializeTurn (Vector3 lookPoint, bool disableAnim) {
        turnTarget = lookPoint;
        
        Vector3 targetDir = GetTargetLookDirection(lookPoint);
        float angleFwd = Vector3.Angle(transform.forward, targetDir);

        //trigger animation if angle is above anim threshold
        inTurnAnimation = angleFwd > behavior.animTurnAngleThreshold && !disableAnim;
        
        //if its below the animation threshold but above the help threshold
        isTurning = inTurnAnimation || angleFwd > behavior.turnAngleHelpThreshold;
    
        if (inTurnAnimation) {
            //set the angle parameter
            eventPlayer[turnAngleName].SetValue(angleFwd);
            //check if turn is to right        
            eventPlayer[turnRightName].SetValue(Vector3.Angle(transform.right, targetDir) <= 90);
        }
        if (isTurning) {
            attemptTurnDir = targetDir;
        }
    }
}