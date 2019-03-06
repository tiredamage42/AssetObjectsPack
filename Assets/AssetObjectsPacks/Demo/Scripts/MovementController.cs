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
    public float[] arriveThresholds = new float[] { .1f, .15f, .2f };
    public float[] turnHelpSpeeds = new float[] { 1, 2.5f, 4 };
    public float dirTurnChangeThreshold = 45;
    public float animTurnAngleThreshold = 22.5f;
    public float turnAngleHelpThreshold = 5.0f;
    public float minStrafeDistance = 1;
    public Movement.Direction direction;
    [Range(0,2)] public int speed;
    [Range(0,1)] public int stance;
    public bool allowStrafe;

    [Header("Auto-Turn")]
    public bool doAutoTurn;
    public bool allowTurnWhenMove = false;


    [Header("Debug:")]
    public bool _;
    public bool trackingWaypoint, isTurning, doingAnimation, isJumping;
    
    const string turnAngleName = "TurnAngle", turnRightName = "ToRight";
    const string speedName = "Speed", directionName = "Direction", stanceName = "Stance";
    
    System.Action endMovementCallback;
    Vector3 interestPoint, movementTarget, attemptTurnDir;
    EventPlayer player;
    int lastSpeed = -1;
    Movement.Direction lastDirection = Movement.Direction.Backwards;
    //some animations are interruptors, but not if jumping
    bool interruptIfNotJumping { get { return !isJumping; } }


    void Awake () {
        player = GetComponent<EventPlayer>();
        player.AddParameters(
            new CustomParameter[] {
                new CustomParameter(turnAngleName, 0.0f),
                new CustomParameter(turnRightName, false),

                // paremeters linked with script properties:
                new CustomParameter ( speedName, () => speed ),
                new CustomParameter ( directionName, () => (int)direction ),
                new CustomParameter ( stanceName, () => stance ),
            }
        );
    }

    void CheckForCueEventOverride (object[] parameters, AssetObjectsPacks.Event overrideEvent) {
        /*
            messages sent from the cues set up to work with this movement controller
            have the cue itself as the first parameter
        */
        AssetObjectsPacks.Cue cue = (AssetObjectsPacks.Cue)parameters[0];
        /*
            if that cue doesn't have any animation events, we'll override the player with 
            this controller's events
        */
        if (cue.GetEventByName(animationPackName) == null) {
            player.OverrideEventToPlay(animationPackName, overrideEvent);    
        }
    }

    /*
        Message Broadcast from demo cue
        parameters:
            cue
            
        stops all movement and plays a loop animation for a single frame, 
        so whatever animaition plays next will exit into a "still" loop

        sets the player to stop the cue immediately after playing that frame
    */
    void StopMovement_Cue (object[] parameters) {
        SetDirection(Movement.Direction.Forward);
        SetSpeed(0);
        CheckForCueEventOverride (parameters, stillsEvent);       
        //set player to end cue right after playing
        player.EndAfterPlay();     
    }

    /*
        Message Broadcast from demo cue
        parameters:
            cue, vector3 target
            
        makes the controller turn, so the forward faces the cue's runtime interest position

        the cue ends when this transform's forward is within the turn help angle 
        (relative to the direction towards the movement target)
    */
    void MovementControllerTurnTo_Cue (object[] parameters) {        
        if (InitializeTurn ((Vector3)parameters[1], false)){
            CheckForCueEventOverride(parameters, turnsEvent);
        }
        else {
            //angle is below anim turn threshold
            player.SkipPlay(animationPackName);
        }

        //if above turn threshold 
        if (isTurning){
            //take control of the players end event
            //to call it when we're facing the movement target
            endMovementCallback = player.OverrideEndEvent();
        }
        else {
            //already facing just end the cue
            player.EndAfterPlay();
        }
    }

    /*
        Message Broadcast from demo cue
        parameters:
            cue, vector3 target, int speed (-1 for current), int direction (-1 for calculate)

        makes the controller set up waypoint tracking to go to the specified position
        (cue's runtime interest position)

        the cue ends when the transform is within the arrive threshold
    */
    void MovementControllerGoTo_Cue(object[] parameters) {

        //returns true if distance is above arrival threshold
        if (InitializeWayPointTracking ((Vector3)parameters[1], (int)parameters[2], (Movement.Direction)((int)parameters[3]))) {
    
            //check if the cue needs an event specified
            CheckForCueEventOverride (parameters, movesEvent);
            
            //take control of the players end event
            //to call it when we've arrived at the waypoint
            endMovementCallback = player.OverrideEndEvent();
        }
        else {
            //if we're wihtin the distance theshold skip playing any animation and end
            // the cue/event right after
            player.SkipPlay(animationPackName);
            player.EndAfterPlay();
        }
    }


    /*
        Go to manually
        plays the go to animation manually
    */
    public void GoTo (Vector3 position, bool interruptPlaylists, int newSpeed = -1, Movement.Direction newDirection = Movement.Direction.Calculated, System.Action onWayPointArrive = null) {

        //returns true if distance is above arrival threshold
        if (InitializeWayPointTracking (position, newSpeed, newDirection)){
            //manually play the event 
            //(simple since we're not tracking the event end via the player)
            bool playSimple = true;
            player.PlayEvent(movesEvent, interruptPlaylists, playSimple, interruptIfNotJumping);
            //set up the callback
            endMovementCallback = onWayPointArrive;
        }
        else {
            //if we're within the threshold skip everything and just call the callback
            if (onWayPointArrive != null) {
                onWayPointArrive();
            }
        }
    }
    /*
        manually turn to a target position
    */
    public void TurnTo (Vector3 target, bool interruptPlaylists, System.Action onTurnSuccess = null) {
        if (InitializeTurn(target, isJumping || (speed != 0 && !allowTurnWhenMove))) {
            //manually play turn event
            player.PlayEvent(turnsEvent, interruptPlaylists, true, interruptIfNotJumping);
            //set up the callback
        }
        if (isTurning) {
            endMovementCallback = onTurnSuccess;
        }
        else {
            //if we're within the threshold skip everything and just call the callback
            if (onTurnSuccess != null) {
                onTurnSuccess();
            }
        }
        
    }

    
    //set movement target to direction of intended movement (carrot on a stick)
    public void SetMovementTarget(Vector3 newTarget) {
        movementTarget = newTarget;
    }

    bool SetDirection(Movement.Direction direction) {
        bool changed = direction != lastDirection;
        this.direction = direction;
        lastDirection = this.direction;
        return changed;
    }
    bool SetSpeed(int speed) {
        bool changed = speed != lastSpeed;
        this.speed = speed;
        lastSpeed = this.speed;
        return changed;
    }

    void CheckSpeedDirectionChanges() {
        bool changedSpeed = SetSpeed(speed);
        bool changedDirection = SetDirection(direction);
        bool changed = changedSpeed || changedDirection;
        
        if (changed) {
            if (speed == 0) {
                SetDirection(Movement.Direction.Forward);
            }            
            if (interruptIfNotJumping) {
                InterruptCurrentAnimations();
            }
            player.PlayEvent(speed == 0 ? stillsEvent : movesEvent, false, true, interruptIfNotJumping);    
        }
    }

    void InterruptCurrentAnimations () {
        doingAnimation = false;
    }
    
    void OnEndJump () {
        isJumping = false;
    }

    public void TriggerJump () {
        if (!isJumping) {
            isJumping = true;
            player.SubscribeToAssetObjectUseEnd(animationPackName, OnEndJump);
            player.SubscribeToEventFail(animationPackName, OnEndJump);
            player.PlayEvent(jumpsEvent, false, false, true);
        }
    }

    

    public void SetInterestPoint (Vector3 position) {
        this.interestPoint = position;
    }

    void Update () {
        DebugLoop();
        CheckSpeedDirectionChanges();
        CheckWayPointTracking();
        CheckAutoTurn();
    }
    void LateUpdate () {
        UpdateSleprTurnHelper();
    }
    
    
    void CheckAutoTurn () {
        if (doAutoTurn && !isTurning) {
            TurnTo(movementTarget, false);
        }
    }

    
    void CheckWayPointTracking () {
        if (trackingWaypoint && !DistanceToWaypointAboveThreshold()) {
            OnMovementSuccess(ref trackingWaypoint);        
        }
    }

    bool DistanceToWaypointAboveThreshold () {
        float threshold = arriveThresholds[speed];
        return Vector3.SqrMagnitude(transform.position - movementTarget) >= threshold * threshold;
    }
    
    Movement.Direction GetDirection () {
        return (speed == 0) ? Movement.Direction.Forward : direction;
    }

    void UpdateSleprTurnHelper () {
        //update slerp turn when not animating turn
        //if (doingAnimation) return;

        //update when turn triggered or when moving (while tracking a waypoint)
        if (isTurning || trackingWaypoint){

            Vector3 targetDir = Vector3.zero;
            float angleFwd;

            bool targetAngleAboveHelpTurnThreshold = false;

            if (!doingAnimation || (doAutoTurn && isTurning)){
                targetAngleAboveHelpTurnThreshold = Movement.TargetAngleAboveTurnThreshold (GetDirection(), transform.position, transform.forward, movementTarget, turnAngleHelpThreshold, out targetDir, out angleFwd);
            }

            if (!doingAnimation) {

                //slerp if above turn threshold
                if (targetAngleAboveHelpTurnThreshold){
                    transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(targetDir), Time.deltaTime * turnHelpSpeeds[speed]);
                }
                else {
                    //if below end turn
                    OnMovementSuccess(ref isTurning);
                }
            }

            if (isTurning && doAutoTurn){
                //if direction has changed too much from last attempt
                //end the turn (retries if above threshold)
                float curAngleFromLastAttempt = Vector3.Angle(targetDir, attemptTurnDir);
                if (curAngleFromLastAttempt > dirTurnChangeThreshold) {
                    OnMovementSuccess(ref isTurning);
                }
            }
        }
    }
    
    void OnEndTurnAnimation () {
        doingAnimation = false;
    } 

    //calback for when turn is achieved or waypoint reached
    void OnMovementSuccess(ref bool check) {
        if (check) {
            OnEndMovement();
            check = false;
        }
    }

    void OnEndMovement () {
        if (endMovementCallback != null) {
            endMovementCallback();
            endMovementCallback = null;
        }
    }    

    bool InitializeWayPointTracking (Vector3 wayPointPosition, int newSpeed, Movement.Direction newDirection) {

        //set movment target
        SetMovementTarget(wayPointPosition);

        //if speed is set to 0 or using last and last is 0 
        //then automatically set walking speed
        if (newSpeed == 0 || (newSpeed < 0 && speed == 0)) {
            newSpeed = 1;
        }
        //if not negative set new speed
        if (newSpeed > 0) {
            SetSpeed(newSpeed);
        }
        
        //calculate movement direction (or set if newDirection is not -1 from cue)
        if (newDirection == Movement.Direction.Calculated) {
            direction = Movement.CalculateMovementDirection(transform.position, movementTarget, interestPoint, allowStrafe, minStrafeDistance);
        }
        else {
            direction = newDirection;
        }
        SetDirection(direction);
        
        trackingWaypoint = DistanceToWaypointAboveThreshold();
        return trackingWaypoint;
    }

    bool InitializeTurn (Vector3 lookPoint, bool disableAnim) {
        SetMovementTarget(lookPoint);

        float angleFwd;
        Vector3 targetDir;
        
        doingAnimation = Movement.TargetAngleAboveTurnThreshold (GetDirection(), transform.position, transform.forward, movementTarget, animTurnAngleThreshold, out targetDir, out angleFwd) && !disableAnim;
        
        isTurning = doingAnimation || angleFwd >= turnAngleHelpThreshold;

        if (isTurning) {
            attemptTurnDir = targetDir;
        }

        if (doingAnimation) {
            /*
                player lets us know when the animation is done lpaying
                then sets "isPlayingTurn" to false

                slerp then turn takes over and makes sure that transform's forward 
                is within the turn threshold
            */
            player.SubscribeToAssetObjectUseEnd(animationPackName, OnEndTurnAnimation);
            /*
                player lets us know if there wasnt any animations found 
                (just calls OnEndTurnAnimation for now)
            */
            player.SubscribeToEventFail(animationPackName, OnEndTurnAnimation);

            //set the angle parameter
            player[turnAngleName].SetValue(angleFwd);
            //check if turn is to right        
            player[turnRightName].SetValue(Vector3.Angle(transform.right, targetDir) <= 90);

        }
        return doingAnimation;
    }
        
}