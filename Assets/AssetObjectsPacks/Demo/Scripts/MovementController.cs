using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AssetObjectsPacks;


public class MovementController : MonoBehaviour
{


    public Transform debugTransformLook;
    void DebugLoop () {
        if (debugTransformLook) {
            SetInterestPoint(debugTransformLook.position);
        }
    }
    
    public AssetObjectsPacks.Event turnsEvent;
    public AssetObjectsPacks.Event movesEvent;
    public AssetObjectsPacks.Event stillsEvent;
    public AssetObjectsPacks.Event jumpsEvent;
    
    public string animationPackName = "Animations";
    public float[] turnThresholds = new float[] { 25.5f, 10, 1 };
    public float[] arriveThresholds = new float[] { .1f, .15f, .2f };
    public float[] turnHelpSpeeds = new float[] { 1, 2.5f, 4 };
    public bool doAutoTurn;
    [Range(0,1)] public int stance;

    public float dirTurnChangeThreshold = 45;
    public float animTurnAngleThreshold = 22.5f;

    
    [Range(0,3)] public int direction;
    public bool allowStrafe;
    public float minStrafeDistance = 1;



    [Range(0,2)] public int speed;
    
    
    //[Header("Debug:")]
    public bool trackingWaypoint;
    public bool isTurning;
    public bool doingAnimation;
    const string turnAngleName = "TurnAngle";
    const string turnRightName = "ToRight";
    const string speedName = "Speed";
    const string stanceName = "Stance";
    const string directionName = "Direction";
    System.Action endEvent;
    Vector3 interestPoint;
    Vector3 movementTarget;
    bool startedStandStill;
    EventPlayer _player;
    EventPlayer player {
        get {
            if (_player == null) _player = GetComponent<EventPlayer>();
            return _player;
        }
    }   

    public void SetMovementTarget(Vector3 newTarget) {
        movementTarget = newTarget;
    }

    void OnStillAnimFail () {
        Debug.LogError("Couldnt find any still anims");
    }
    void OnMoveAnimFail () {
        Debug.LogError("Couldnt find any move anims");
    }
    void OnTurnAnimFail () {
        Debug.LogError("Couldnt find any turn anims");
        doingAnimation = false;
    }
    
    void CheckStandingStillTransition () {
        if (startedStandStill) {
            //Debug.Log("Stopped stand still");
            TriggerEndEvent();
            startedStandStill = false;
        }
    }

    void SetCurrentSpeedStanceDireciton () {
        player[speedName].SetValue(speed);
        player[stanceName].SetValue(stance);
        player[directionName].SetValue(direction);
    }


    void SetMovementControllerSpeed_Cue(object[] parameters) {

        int newSpeed = (int)parameters[1];
        speed = newSpeed;

        lastSpeed = speed;
        if (speed == 0) {
            //Debug.Log("stoping cue");
            direction = 0;

            CheckForCueEventOverride (parameters, stillsEvent);            
            
            SetCurrentSpeedStanceDireciton();
            
            player.SubscribeToEventFail(animationPackName, OnStillAnimFail);
            
            endEvent = player.OverrideEndEvent();
            startedStandStill = true;
            
        }
        else {
            //other callbacks handle animations and end events
        }
    }

    int lastSpeed = -1;
    //plain movement no destination (manual set)
    public void SetMovementControllerSpeed(int speed) {
        bool changed = speed != lastSpeed;
        lastSpeed = speed;
        this.speed = speed;

        if (changed) {
            if (speed == 0) {

                direction = 0;
                Debug.Log("stopped");
            }
            else {
                                Debug.Log("went");

            }

            
            bool asInterrupter = !isJumping;
            System.Action callback = OnMoveAnimFail;
            if (speed == 0) callback = OnStillAnimFail;
            Debug.Log("playing evnt");
            SwitchLoopState (asInterrupter, speed == 0 ? stillsEvent : movesEvent, callback);
              
        }
    }

    void CheckSpeedDirectionChanges(int speed, int direction) {
        bool changed = speed != lastSpeed || direction != lastDirection || (speed == 0 && direction != 0);
        
        lastSpeed = speed;
        this.speed = speed;

        lastDirection = direction;
        this.direction = direction;
        

        if (changed) {
            if (speed == 0) {

                lastDirection = 0;
                this.direction = 0;

                Debug.Log("stopped");
            }
            else {
                Debug.Log("went");

            }

            
            bool asInterrupter = !isJumping;


            System.Action callback = OnMoveAnimFail;
            if (speed == 0) callback = OnStillAnimFail;
            Debug.Log("playing evnt");
            SwitchLoopState (asInterrupter, speed == 0 ? stillsEvent : movesEvent, callback);
              
        }
    }

    void SwitchLoopState (bool asInterrupter, AssetObjectsPacks.Event loopEvent, System.Action failCallback) {
        if (asInterrupter) {
            doingAnimation = false;
        }
        SetCurrentSpeedStanceDireciton();
        player.SubscribeToEventFail(animationPackName, failCallback);      
        player.PlayEvent(loopEvent, false, true, asInterrupter);
    
    }

    int lastDirection = -9999;
    //plain movement no destination (manual set)
    public void SetMovementControllerDirection(int direction) {

        if (speed == 0) {
            lastDirection = 0;
            this.direction = 0;
            return;
        }

        bool changed = direction != lastDirection;
        lastDirection = direction;
        this.direction = direction;
        if (changed) {
            Debug.Log("changing loopstates for direction");
            bool asInterrupter = !isJumping;
            SwitchLoopState (asInterrupter, movesEvent, OnMoveAnimFail);
        }
    }

    void CheckForCueEventOverride (object[] parameters, AssetObjectsPacks.Event overrideEvent) {
        AssetObjectsPacks.Cue cue = (AssetObjectsPacks.Cue)parameters[0];
        if (cue.GetEventByName(animationPackName) == null) {
            //Debug.Log("no event specivied in cue, using default");
            player.OverrideEventToPlay(animationPackName, overrideEvent);    
        }
    }

    //override so no animation plays when cue plays
    //end event will happen next update since its within the threshold
    void SkipCueAnimWithinThreshold () {
        //Debug.Log("overriding null animation");
        player.OverrideEventToPlay(animationPackName, null);    
    }


    /*
        takes in:
            cue 
            vector3 target
    */
    void MovementControllerGoTo_Cue(object[] parameters) {
        CheckForCueEventOverride (parameters, movesEvent);

        Vector3 targ = (Vector3)parameters[1];
        
        SetMovementTarget(targ);

        //check threshold
        if (DistanceToWaypointAboveThreshold()) {
            PrepareForMoveAnimAttempt(false, false, true);
        }
        else {
            SkipCueAnimWithinThreshold();
        }

        trackingWaypoint = true;
        endEvent = player.OverrideEndEvent();
    }
    

    //when using a face-towards cue
    void MovementControllerTurnTo_Cue (object[] parameters) {
        CheckForCueEventOverride(parameters, turnsEvent);
        Vector3 pos = (Vector3)parameters[1];

        //Debug.Log("turning " +pos);
        SetMovementTarget(pos);
        float angleFwd;
        Vector3 targetDir;        
        if (TargetAngleAboveTurnThreshold (speed == 0 ? 0 : direction, transform.position, transform.forward, movementTarget, animTurnAngleThreshold, out targetDir, out angleFwd)) {
            PrepareForTurnAnimationAttempt (angleFwd, targetDir, false, false, true);
        }
        else {
            SkipCueAnimWithinThreshold();
            doingAnimation = false;
        }
        isTurning = true;
        endEvent = player.OverrideEndEvent();
    }

    public bool isJumping;

    void OnEndJump () {
        isJumping = false;

    }
    void OnJumpFail () {
        Debug.LogError("couldnt find jump anim");
    }

    public void TriggerJump () {
        if (!isJumping) {
            isJumping = true;
            player.SubscribeToAssetObjectUseEnd(animationPackName, OnEndJump);
            player.SubscribeToEventFail(animationPackName, OnJumpFail);
            SetCurrentSpeedStanceDireciton();
            player.PlayEvent(jumpsEvent, false, false, true);
        }
    }

    Vector3 attemptTurnDir;
    public void TurnTo (Vector3 target, bool interruptPlaylists) {
        //direction = 0;
        SetMovementTarget(target);
        float angleFwd;
        Vector3 targetDir;

        if (TargetAngleAboveTurnThreshold (
            speed == 0 ? 0 : direction, transform.position, 
            transform.forward, 
            movementTarget, 
            animTurnAngleThreshold, 
            out targetDir, out angleFwd
        ) && !isJumping) {

            endEvent = player.OverrideEndEvent();
            PrepareForTurnAnimationAttempt(angleFwd, targetDir, true, interruptPlaylists, true);
            //TriggerEndEvent();
        }
        else {
            if (angleFwd >= turnThresholds[speed]) {
                attemptTurnDir = targetDir;
                //just slerp helper
                isTurning = true;
            }

        }
    }


    void PrepareForTurnAnimationAttempt (float angleFwd, Vector3 dirToTarget, bool doAttempt, bool interruptPlaylist, bool asInterrupter) {
        doingAnimation = true;
        isTurning = true;
        attemptTurnDir = dirToTarget;

        player.SubscribeToAssetObjectUseEnd(animationPackName, OnEndTurnAnimation);
        player.SubscribeToEventFail(animationPackName, OnTurnAnimFail);
        
            
        player[turnAngleName].SetValue(angleFwd);        
        player[turnRightName].SetValue(Vector3.Angle(transform.right, dirToTarget) <= 90);
        SetCurrentSpeedStanceDireciton();

        if (doAttempt) player.PlayEvent(turnsEvent, interruptPlaylist, false, asInterrupter);
    }
    void PrepareForMoveAnimAttempt (bool doAttempt, bool interruptPlaylist, bool asInterrupter) {
        direction = GetDirection(transform.position, movementTarget, interestPoint, allowStrafe, minStrafeDistance);
        
        trackingWaypoint = true;

        player.SubscribeToEventFail(animationPackName, OnMoveAnimFail);
        
        SetCurrentSpeedStanceDireciton();
            
        if (doAttempt) player.PlayEvent(movesEvent, interruptPlaylist, false, asInterrupter);
    }


    //deal with interruptions
    public void GoTo (Vector3 position, bool interruptPlaylists, System.Action onWayPointArrive) {
        SetMovementTarget(position);

        if (DistanceToWaypointAboveThreshold()) {

            endEvent = player.OverrideEndEvent();

            bool asInterrupter = true;

            PrepareForMoveAnimAttempt(true, interruptPlaylists, asInterrupter);

            TriggerEndEvent();

            endEvent = onWayPointArrive;
        }
        else {

            onWayPointArrive();

        }
    }
    //0 fwd 1 left 2 right 3 back


    void Awake () {
        player.AddParameters(
            new CustomParameter[] {
                new CustomParameter(turnAngleName, 0.0f),
                new CustomParameter(speedName, 1),
                new CustomParameter(turnRightName, false),
                new CustomParameter(stanceName, 0),
                new CustomParameter(directionName, 0),
            }
        );
    }

    
    
    void OnWaypointArrive () {
        trackingWaypoint = false;
        TriggerEndEvent();
    }

    public void SetInterestPoint (Vector3 position) {
        this.interestPoint = position;
    }


    void Update () {
        DebugLoop();
        //in case manual speed change

        CheckSpeedDirectionChanges(speed, direction);

        //SetMovementControllerSpeed(speed);
        //SetMovementControllerDirection(direction);

        CheckStandingStillTransition();
        CheckWayPointTracking();
        CheckAutoTurn();
    }
    void FixedUpdate () {
    }
    void LateUpdate () {

        UpdateSleprTurnHelper();
    }
    
    void CheckWayPointTracking () {
        if (trackingWaypoint) {
            if (!DistanceToWaypointAboveThreshold()) {
                //Debug.Log("arrived");
                OnWaypointArrive();
            }
        }
    }
    bool DistanceToWaypointAboveThreshold () {
        Debug.DrawLine(movementTarget, transform.position, Color.green);
        return Vector3.Distance(transform.position, movementTarget) >= arriveThresholds[speed];
    }

    //set movement target to direction of intended movement (carrot on a stick)


    void UpdateSleprTurnHelper () {
        if ((isTurning || trackingWaypoint) && (!doingAnimation)){


            Vector3 targetDir;
            float angleFwd;

            bool targetAngleAboveHelpTurnThreshold = TargetAngleAboveTurnThreshold (
                speed == 0 ? 0 : direction, transform.position, transform.forward, movementTarget, turnThresholds[speed], 
                out targetDir, out angleFwd
            );

            //bool targetDirChangedFromAttempt = doAutoTurn && Vector3.Angle(targetDir, attemptTurnDir) > dirTurnChangeThreshold;

            if (targetAngleAboveHelpTurnThreshold){//} && !targetDirChangedFromAttempt) {
                
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(targetDir), Time.deltaTime * turnHelpSpeeds[speed]);
            }
            else {
                //Debug.Log("Got to turn");
                if (isTurning) 
                    OnEndTurn();
            }


            if (isTurning && doAutoTurn){


                //float angleFwd;
                //bool targetAngleAboveHelpTurnThreshold = TargetAngleAboveTurnThreshold (speed == 0 ? 0 : direction, transform.position, transform.forward, movementTarget, turnThresholds[speed], out targetDir, out angleFwd);
                bool targetDirChangedFromAttempt = Vector3.Angle(targetDir, attemptTurnDir) > dirTurnChangeThreshold;
                if (targetDirChangedFromAttempt) {
                    OnEndTurn();
                }
                
            }
        }

    }
    
    void CheckAutoTurn () {
        if (doAutoTurn && !isTurning) {
            TurnTo(movementTarget, false);
        }
    }


    //called when animation ends
    void OnEndTurnAnimation () {
        //Debug.Log("end turn anim still turn");
        doingAnimation = false;
    }   
    void OnEndTurn () {
        //Debug.Log("end turn");
        isTurning = false;
        TriggerEndEvent();
    }   
    void TriggerEndEvent () {
        //Debug.Log("Triggering end event " + endEvent);
        if (endEvent != null) {
            endEvent();
            endEvent = null;
        }
    }

    static bool TargetAngleAboveTurnThreshold (int direction, Vector3 position, Vector3 faceDir, Vector3 target, float threshold, out Vector3 dir, out float angleFwd) {
        
        dir = GetTargetLookDirection(direction, position, target);
        angleFwd = Vector3.Angle(faceDir, dir);
 
        return angleFwd >= threshold;
    }
    static Vector3 GetTargetLookDirection(int direction, Vector3 position, Vector3 target) {
        Vector3 startToDest = target - position;
        startToDest.y = 0;
        if (direction == 0) {
            return startToDest;
        }
        else if (direction == 1) {
            return -Vector3.Cross(startToDest.normalized, Vector3.up);
        }
        else if (direction == 2) {
            return Vector3.Cross(startToDest.normalized, Vector3.up);
        }
        else if (direction == 3) {
            return -startToDest;
        }
        return startToDest;
    }

        
    static int GetDirection (Vector3 startPos, Vector3 destination, Vector3 interestPoint, bool allowStrafe, float minStrafeDistance) {
        if (!allowStrafe) return 0;
        
        Vector3 startToDest = destination - startPos;
        
        //maybe return current direction (for no sudden changes)
        if (startToDest.magnitude < minStrafeDistance) {
            return 0;
        }
        startToDest.y = 0;

        //Debug.DrawLine(startPos, destination, Color.blue);

        Vector3 midPoint = (startPos + destination) * .5f;
        Vector3 midToInterest = interestPoint - midPoint;
        midToInterest.y = 0;

        //Debug.DrawLine(midPoint, interestPoint, Color.red);
        

        float angle = Vector3.Angle(midToInterest, startToDest);

        /*
            ideal  angle fr look position is 90 or -90
        
            a -------------- b
                    /
                   /
                  /
            interest point

        */

        //angle is too acute or obtuse between interest (enemy point) and destination
        //for strafing (backwards or forwards)
        if (angle <= 45 || angle >= 135) return angle >= 135 ? 0 : 3;
        

        /*
              interest point
                  \
                   \
                    \
            a -------------- b
                    |
                    | <- startToDestPerp
                    |

        */
        Vector3 startToDestPerp = Vector3.Cross(startToDest.normalized, Vector3.up);

        angle = Vector3.Angle(startToDestPerp, midToInterest);

        return angle <= 45 ? 2 : 1;
    
    }
        
}