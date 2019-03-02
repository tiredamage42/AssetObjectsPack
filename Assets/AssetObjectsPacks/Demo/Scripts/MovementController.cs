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
    
    public string animationPackName = "Animations";
    public float[] turnThresholds = new float[] { 25.5f, 10, 1 };
    public float[] arriveThresholds = new float[] { .1f, .15f, .2f };
    public float[] turnHelpSpeeds = new float[] { 1, 2.5f, 4 };
    public bool doAutoTurn;
    [Range(0,1)] public int stance;

    
    [Range(0,2)] public int direction;
    public bool allowStrafe;
    public float minStrafeDistance = 1;



    [Range(0,2)] public int speed;
    
    
    //[Header("Debug:")]
    bool trackingWaypoint;
    bool isTurning;
    bool doingAnimation;
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
            direction = 0;
            
            CheckForCueEventOverride (parameters, stillsEvent);
            
            SetCurrentSpeedStanceDireciton();
            
            player.SubscribeToEventFail(animationPackName, OnStillAnimFail);
            
            endEvent = player.OverrideEndEvent();
            //Debug.Log("stoping cue");
            
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
            }

            SetCurrentSpeedStanceDireciton();

            bool hasEventEndAlready = endEvent != null;
            if (!hasEventEndAlready) endEvent = player.OverrideEndEvent();
            else {
                //just so it doesnt try doing it itself
                player.OverrideEndEvent(); 
            }
            
            if (speed == 0) player.SubscribeToEventFail(animationPackName, OnStillAnimFail);
            else player.SubscribeToEventFail(animationPackName, OnMoveAnimFail);      
            
            player.PlayEvent(speed == 0 ? stillsEvent : movesEvent, false);

            //so it's ready to play another event wihtout complications (these are looped anywyas)
            if (!hasEventEndAlready) TriggerEndEvent();
            
        }
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
            
            SetCurrentSpeedStanceDireciton();

            bool hasEventEndAlready = endEvent != null;
            if (!hasEventEndAlready) endEvent = player.OverrideEndEvent();
            else {
                //just so it doesnt try doing it itself
                player.OverrideEndEvent(); 
            }
            
            player.SubscribeToEventFail(animationPackName, OnMoveAnimFail);      
            
            player.PlayEvent(movesEvent, false);

            //so it's ready to play another event wihtout complications (these are looped anywyas)
            if (!hasEventEndAlready) TriggerEndEvent();
            
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
            PrepareForMoveAnimAttempt(false, false);
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
        if (TargetAngleAboveTurnThreshold (false, speed == 0 ? 0 : direction, transform.position, transform.forward, movementTarget, turnThresholds[speed], out targetDir, out angleFwd)) {
            PrepareForTurnAnimationAttempt (angleFwd, targetDir, false, false);
        }
        else {
            SkipCueAnimWithinThreshold();
            doingAnimation = false;
        }
        isTurning = true;
        endEvent = player.OverrideEndEvent();
    }

    public void TurnTo (Vector3 target, bool interruptPlaylists) {
        //direction = 0;
        SetMovementTarget(target);
        float angleFwd;
        Vector3 targetDir;

        if (TargetAngleAboveTurnThreshold (false, speed == 0 ? 0 : direction, transform.position, transform.forward, movementTarget, turnThresholds[speed], out targetDir, out angleFwd)) {
            endEvent = player.OverrideEndEvent();
            PrepareForTurnAnimationAttempt(angleFwd, targetDir, true, interruptPlaylists);
            TriggerEndEvent();
        }
    }


    void PrepareForTurnAnimationAttempt (float angleFwd, Vector3 dirToTarget, bool doAttempt, bool interruptPlaylist) {
        doingAnimation = true;
        isTurning = true;

        player.SubscribeToEventEnd(animationPackName, OnEndTurnAnimation);
        player.SubscribeToEventFail(animationPackName, OnTurnAnimFail);
        
        SetCurrentSpeedStanceDireciton();
            
        player[turnAngleName].SetValue(angleFwd);
        float angleRight = Vector3.Angle(transform.right, dirToTarget);
        player[turnRightName].SetValue(angleRight <= 90);

        if (doAttempt) player.PlayEvent(turnsEvent, interruptPlaylist);
    }
    void PrepareForMoveAnimAttempt (bool doAttempt, bool interruptPlaylist) {
        trackingWaypoint = true;

        player.SubscribeToEventFail(animationPackName, OnMoveAnimFail);
        direction = GetDirection(transform.position, movementTarget, interestPoint, allowStrafe, minStrafeDistance);
        SetCurrentSpeedStanceDireciton();
            
        if (doAttempt) player.PlayEvent(movesEvent, interruptPlaylist);
    }


    //deal with interruptions
    public void GoTo (Vector3 position, bool interruptPlaylists, System.Action onWayPointArrive) {
        SetMovementTarget(position);

        if (DistanceToWaypointAboveThreshold()) {

            endEvent = player.OverrideEndEvent();

            PrepareForMoveAnimAttempt(true, interruptPlaylists);

            TriggerEndEvent();

            endEvent = onWayPointArrive;
        }
        else {

            onWayPointArrive();

        }
    }
    //0 fwd 1 left 2 right


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
        SetMovementControllerDirection(direction);
        SetMovementControllerSpeed(speed);

        CheckStandingStillTransition();
        CheckWayPointTracking();
        CheckAutoTurn();
    }
    void FixedUpdate () {
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
        if ((isTurning || trackingWaypoint) && !doingAnimation){


            Vector3 targetDir;
            float angleFwd;
            if (TargetAngleAboveTurnThreshold (isTurning, speed == 0 ? 0 : direction, transform.position, transform.forward, movementTarget, turnThresholds[speed], out targetDir, out angleFwd)) {
                
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(targetDir), Time.deltaTime * turnHelpSpeeds[speed]);
            }
            else {
                //Debug.Log("Got to turn");
                if (isTurning) 
                    OnEndTurn();
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

    static bool TargetAngleAboveTurnThreshold (bool debug, int direction, Vector3 position, Vector3 faceDir, Vector3 target, float threshold, out Vector3 dir, out float angleFwd) {
        
        dir = GetTargetLookDirection(direction, position, target);
        angleFwd = Vector3.Angle(faceDir, dir);

        if (debug) {

            //Debug.DrawRay(position, dir, Color.green);
        
            //Debug.DrawRay(position, faceDir, Color.red);

            //Debug.LogError(angleFwd + "/ " + (angleFwd >= threshold));

            //Debug.Break();
        }
        
        
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
        
            a -------------- b
                    |
                    |
                    |
                interest point

            ideal  angle fr look position is 90 or -90
        */

        if (angle <= 45 || angle >= 135) {
            //angle is too acute or obtuse between interest (enemy point) and destination
            //for strafing

            //Debug.LogError ("angle is too acute or obtuse");
            //Debug.Break ();
            return 0;
        }

        /*
                interest point
                    |
                    |
                    |
            a -------------- b
                    |
                    | <- startToDestPerp
                    |

        */
        Vector3 startToDestPerp = Vector3.Cross(startToDest.normalized, Vector3.up);

        //Debug.DrawRay(midPoint, startToDestPerp.normalized, Color.green, startToDestPerp.magnitude);
        
        angle = Vector3.Angle(startToDestPerp, midToInterest);

        if (angle <= 45) {
            //Debug.LogError ("strafing right towards destination");
            //Debug.Break ();
            return 2;
        }
        else {
            //Debug.LogError ("strafing left towards destination");   
            //Debug.Break ();
            return 1;
        }
    }
        
}