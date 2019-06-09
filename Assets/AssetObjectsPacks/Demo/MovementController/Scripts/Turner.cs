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
public class Turner : MovementControllerComponent
{
    public float turnAttemptRetryTime = 1.0f;
    public bool doAutoTurn, autoTurnAnimate;
    public bool use2d = true;
    
    public bool isTurning, inTurnAnimation, initializedTurnCue;
    EventPlayer.EventPlayEnder endEventPlayerPlay;
    Vector3 attemptTurnDir;
    float lastAttemptTurn;
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
        _turnTarget = target;
    }

        Vector3 _turnTarget;
        System.Func<Vector3> getTurnTarget;

        public Vector3 turnTarget {
            get {
                if (getTurnTarget != null) {
                    return getTurnTarget();
                }
                return _turnTarget;
            }
        }

        public void SetTurnTargetCallback (System.Func<Vector3> getTurnTarget) {
            this.getTurnTarget = getTurnTarget;
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
            if (TurnTo(turnTarget, behavior.animTurnAngleThreshold, null)) {
                // Debug.Log("auto turned");
            }
        }
    }

    void OnEndTurn () {
        inTurnAnimation = false;
        isTurning = false;
        initializedTurnCue = false;
        
        endEventPlayerPlay.EndPlay("turn");
        endEventPlayerPlay = null;    
    }


    public bool isSlerpingTransformRotation { get { return slerpingRotation; } }
    public bool slerpingRotation;
    public Vector3 targetTurnDirection { get { return _targetTurnDirection; } }
    Vector3 _targetTurnDirection;
    public float angleDifferenceWithTarget { get { return _angleDifferenceWithTarget; } }
    public float _angleDifferenceWithTarget;
    



    Vector3 DirToTarget () {
        Vector3 targetDir = turnTarget - transform.position;
        Vector3 flatTarget = new Vector3(targetDir.x, 0, targetDir.z);
        return use2d ? flatTarget : targetDir;
    }

    void FinalizeTurnHelper (float deltaTime) {

        slerpingRotation = false;
        bool doingManualTurn = isTurning && initializedTurnCue;

        if (doingManualTurn || doAutoTurn) {
            _targetTurnDirection = DirToTarget();
            
            if (doingManualTurn) {
                if (CheckForEndTurn(_targetTurnDirection)) {
                    OnEndTurn();
                    return;
                }
            }
        
            //if we're turning and animation is done, slerp to reach face target
            if (!inTurnAnimation) {
                if (!controller.scriptedMove) {
                    slerpingRotation = true;
                    transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(Movement.CalculateTargetFaceDirection(controller.direction, transform.position, turnTarget, use2d)), deltaTime * behavior.turnHelpSpeeds[controller.speed]);
                }
            }
        }
    }





    bool CheckForEndTurn (Vector3 targetDir) {
        
        //if direction has changed too much from last attempt
        //end the turn (retries if above threshold and auto turning)

        float timeSinceLastAttempt = Time.time - lastAttemptTurn;
        if (timeSinceLastAttempt >= turnAttemptRetryTime) {
            float angleFromLastAttempt = Vector3.Angle(targetDir, attemptTurnDir);
            if (angleFromLastAttempt > behavior.dirTurnChangeThreshold) {  
                return true;
            }
        }
        
        //if we're done animating
        if (!inTurnAnimation) {
            //check movement direction (or forward) angle with target direction
            _angleDifferenceWithTarget = Vector3.Angle(controller.moveDireciton, targetDir);                

            //deactivate turning (within threshold)
            if (_angleDifferenceWithTarget < behavior.turnAngleHelpThreshold){
                return true;
            }   
        }
        return false;
    }


    /*
        callback called by cue message

        parameters:
            layer (internally set), vector3 target
            
        makes the controller turn, so the forward (or move direction) faces the cue's runtime interest position

        the cue ends when this transform's forward (or move direction) is within the turn help angle 
        (relative to the direction towards the movement target)
    */
    void TurnTo (object[] parameters) {
        
        int layer = (int)parameters[0];
        _turnTarget = (Vector3)parameters[1];


        // Vector3 targetDir = Movement.CalculateTargetFaceDirection(controller.direction, transform.position, turnTarget, use2d);
        Vector3 targetDir = DirToTarget();
            
        //check movement direction (or forward) angle with target direction
        float angleWMoveDir = Vector3.Angle(controller.moveDireciton, targetDir);                

        //bool skipAnim = doAutoTurn && !autoTurnAnimate;  

        //trigger animation if angle is above anim threshold
        inTurnAnimation = angleWMoveDir > behavior.animTurnAngleThreshold;// && !skipAnim;
        
        //if its below the animation threshold but above the help threshold
        isTurning = inTurnAnimation || angleWMoveDir > behavior.turnAngleHelpThreshold;
        
        if (isTurning) {
            lastAttemptTurn = Time.time;
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

    public void TurnTo (Vector3 target, Action onTurnSuccess = null) {
        TurnTo(target, behavior.turnAngleHelpThreshold, onTurnSuccess);
    }
    bool TurnTo (Vector3 target, float threshold, Action onTurnSuccess = null) {

        if (!isTurning && !controller.scriptedMove){
                
            //calculate the target direction
            // Vector3 targetDir = Movement.CalculateTargetFaceDirection(controller.direction, transform.position, target, use2d);
            Vector3 targetDir = DirToTarget();
        
            //check movement direction (or forward) angle with target direction
            float angleWMoveDir = Vector3.Angle(controller.moveDireciton, targetDir);
            
            //if that angle is above our turn threshold:
            if (angleWMoveDir > threshold) {

                isTurning = true;                
                initializedTurnCue = false;
                Playlist.InitializePerformance("turning", behavior.turnCue, eventPlayer, false, eventLayer, new MiniTransform(target, Quaternion.identity), false, onTurnSuccess);
                return true;
            }
        }
        return false;
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
}