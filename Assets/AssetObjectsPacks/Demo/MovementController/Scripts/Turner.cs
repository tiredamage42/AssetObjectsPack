using UnityEngine;
using AssetObjectsPacks;
using System;

namespace Movement
{

    /*
        incorporate turning
        base turner class, just uses slerp turning

        more advanced player utility, overrides cue ending when using in playlists

        turning is considered done afetr animation plays and slerp turns the rest of the way    
    */
    public class Turner : MovementControllerComponent
    {
        public float turnAttemptRetryTime = 1.0f;
        public bool doAutoTurn;
        public bool use2d = true;
        
        public bool isTurning, disableSlerp;
        Vector3 attemptTurnDir;
        float lastAttemptTurn;
        
        void OnDrawGizmos()
        {
            if (isTurning || doAutoTurn) {
                Gizmos.color = disableSlerp ? Color.green : Color.white;
                Gizmos.DrawWireSphere(transform.position + Vector3.up * 2, .05f);
                Gizmos.DrawWireSphere(turnTarget, .05f);
            }
        }

        public void SetTurnTarget (Vector3 target) {
            _turnTarget = target;
        }
        public void SetTurnTargetCallback (System.Func<Vector3> getTurnTarget) {
            this.getTurnTarget = getTurnTarget;
        }

        Vector3 _turnTarget;
        Func<Vector3> getTurnTarget;
        public Vector3 turnTarget {
            get {
                if (getTurnTarget != null) {
                    return getTurnTarget();
                }
                return _turnTarget;
            }
        }

        public override void UpdateLoop (float deltaTime) {
            FinalizeTurnHelper(deltaTime);
        }    


        void OnEndTurn () {
            isTurning = false;
            
            disableSlerp = false;

            if (onTurnDone != null) {
                onTurnDone(true);
                onTurnDone = null;
            }
        }


        public bool isSlerpingTransformRotation { get { return slerpingRotation; } }
        bool slerpingRotation;

        public Vector3 targetTurnDirection { get { return _targetTurnDirection; } }
        Vector3 _targetTurnDirection;
        
        public float angleDifferenceWithTarget { get { return _angleDifferenceWithTarget; } }
        float _angleDifferenceWithTarget;
        
        public Vector3 DirToTarget () {
            Vector3 targetDir = turnTarget - transform.position;
            return use2d ? new Vector3(targetDir.x, 0, targetDir.z) : targetDir;
        }

        void FinalizeTurnHelper (float deltaTime) {

            slerpingRotation = false;
            
            if (isTurning || doAutoTurn) {
                _targetTurnDirection = DirToTarget();
                
                if (isTurning) {
                    if (CheckForEndTurn(_targetTurnDirection)) {
                        OnEndTurn();
                        return;
                    }
                }
            
                //if we're turning and animation is done, slerp to reach face target
                if (!disableSlerp) {
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

                    Debug.LogError("reattempt turn");
                    return true;
                }
            }
            
            //if we're done animating
            if (!disableSlerp) {
                //check movement direction (or forward) angle with target direction
                _angleDifferenceWithTarget = Vector3.Angle(controller.moveDireciton, targetDir); 

                // Debug.DrawRay(transform.position, targetDir.normalized * 5, Color.cyan);
                // Debug.DrawRay(transform.position, controller.moveDireciton.normalized * 5, Color.yellow);
                            
                //deactivate turning (within threshold)
                if (_angleDifferenceWithTarget < behavior.turnAngleHelpThreshold){
                    return true;
                }   
            }
            return false;
        }

        Action<bool> onTurnDone;

        public event Action<Vector3, float> onTurnStart;
        public void InitializeTurning (Vector3 target, Action<bool> onTurnDone) {
            _turnTarget = target;
            
            this.onTurnDone = onTurnDone;

            Vector3 targetDir = DirToTarget();
                
            //check movement direction (or forward) angle with target direction
            float angleWMoveDir = Vector3.Angle(controller.moveDireciton, targetDir);                

            //if it's above the help threshold
            isTurning = angleWMoveDir > behavior.turnAngleHelpThreshold;
            
            if (isTurning) {
                lastAttemptTurn = Time.time;
                attemptTurnDir = targetDir;

                if (onTurnStart != null) {
                    onTurnStart(targetDir, angleWMoveDir);
                }
            }
            
        }

        public void TurnToTarget (Vector3 target, Action<bool> onTurnSuccess = null) {
            if (!isTurning && !controller.scriptedMove){
                InitializeTurning ( target, onTurnSuccess);
            }
        }

    }
}