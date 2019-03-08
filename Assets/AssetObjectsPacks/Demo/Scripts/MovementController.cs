using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AssetObjectsPacks;


public class MovementController : MonoBehaviour
{
    public bool usingRootMotion = true;

    public void EnableRootMotion (bool enabled) {
        usingRootMotion = enabled;
    }
    void EnableRootMotion_Cue (object[] parameters) {
        EnableRootMotion((bool)parameters[0]);
    }
    public AnimatorUpdateMode turnUpdate = AnimatorUpdateMode.Normal;
    public AnimatorUpdateMode moveUpdate { get { return anim.updateMode; } }
    

    Animator anim { get { return GetComponent<Animator>(); } }
    CharacterController cc { get { return GetComponent<CharacterController>(); } }

    Vector3 rootPosition;
    Quaternion rootRotation;
    
    void OnAnimatorMove () {
        rootPosition = anim.deltaPosition;
        rootRotation = anim.deltaRotation;
    }
    void FixedUpdate () {
        CheckGrounded();
        UpdateLoop(AnimatorUpdateMode.AnimatePhysics, Time.fixedDeltaTime);
    }
    
 public bool grounded;
 public float groundDistanceCheckAir = .01f;
 public float groundDistanceCheckGrounded = .25f;
 public float groundRadiusCheck = .1f;

 public LayerMask groundLayerMask;

 const float groundCheckBuffer = .25f;
 Vector3 groundNormal = Vector3.up;

    float maxGroundAngle { get { return cc.slopeLimit; } }

    //public bool ignoreGravity;

 void CheckGrounded () {
     float distanceCheck = groundCheckBuffer + (grounded ? groundDistanceCheckGrounded : groundDistanceCheckAir);

     grounded = false;
     groundNormal = Vector3.up;
     Ray ray = new Ray(transform.position + Vector3.up * groundCheckBuffer,Vector3.down);
     //Debug.DrawRay(ray.origin, ray.direction * distanceCheck, Color.red);
     RaycastHit hit;
     if (Physics.SphereCast(ray, groundRadiusCheck, out hit, distanceCheck, groundLayerMask)) {
         groundNormal = hit.normal;

         if (Vector3.Angle(groundNormal, Vector3.up) <= maxGroundAngle) {
             grounded = true;
         }
     }
}

 public float minYVelocity = -1f;



     
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
    //public bool _;
    public bool trackingWaypoint, isTurning, doingAnimation, isJumping;
    
    const string turnAngleName = "TurnAngle", turnRightName = "ToRight";
    const string speedName = "Speed", directionName = "Direction", stanceName = "Stance";
    
    System.Action endTurnCallback, reachWaypointCallback;
    Vector3 interestPoint, movementTarget, attemptTurnDir;
    EventPlayer player;
    int lastSpeed = -1;
    Movement.Direction lastDirection = Movement.Direction.Backwards;
    //some animations are interruptors, but not if jumping
    bool interruptIfNotJumping { get { return !isJumping; } }


    void Awake () {

        //cc.enabled = false;
        anim.applyRootMotion = false;//true;
            
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
            player.OverrideEventToPlay(-1, animationPackName, overrideEvent);    
        }
    }

    IEnumerator SetPhysics (object[] value, float delay) {
        yield return new WaitForSeconds(delay);
        UseGravity_Cue(value);

    }

    /* 
        Message Broadcast from demo cue

        parameters:
            useGravity, delaytime (optional)

        ignores character controller gravity for jump animations / platforming stuff
    */
    void UseGravity_Cue(object[] parameters) {
        if (parameters.Length == 2) {
            StartCoroutine(SetPhysics(new object[] { parameters[0] }, (float)parameters[1]));
        }
        else {
            cc.enabled = (bool)parameters[0];
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

        //player.EndAfterPlay();     
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
        if (InitializeTurn (-1, (Vector3)parameters[1], false)){
            CheckForCueEventOverride(parameters, turnsEvent);
        }
        else {
            //angle is below anim turn threshold
            player.SkipPlay(-1, animationPackName);
        }

        //if above turn threshold 
        if (isTurning){
            //take control of the players end event
            //to call it when we're facing the movement target
            endTurnCallback = player.OverrideEndEvent(-1);
        }
        else {
            //already facing just end the cue
            //turn to cue duration = 0  so if above doesnt override it just skips
            
            //player.EndAfterPlay();
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

        int paramSpeed = -1;
        if (parameters.Length > 2) {
            paramSpeed = (int)parameters[2];
        }
        Movement.Direction paramDir = Movement.Direction.Calculated;
        if (parameters.Length > 3) {
            paramDir = (Movement.Direction)((int)parameters[3]);
        }
            //Debug.Log("in message");
        
        foreach (object o in parameters) {
            //Debug.Log(o);
        }
        Vector3 position = (Vector3)parameters[1];

        //returns true if distance is above arrival threshold
        if (InitializeWayPointTracking (position, paramSpeed, paramDir)) {
    
            //check if the cue needs an event specified
            CheckForCueEventOverride (parameters, movesEvent);
            
            //take control of the players end event
            //to call it when we've arrived at the waypoint
            reachWaypointCallback = player.OverrideEndEvent(-1);
        }
        else {
            //if we're wihtin the distance theshold skip playing any animation and end
            // the cue/event right after
            player.SkipPlay(-1, animationPackName);

            //move to cue duration = 0  so if above doesnt override it just skips

            //player.EndAfterPlay();
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
            player.PlayEvent(interruptPlaylists ? -1 : 0, movesEvent, 0, interruptIfNotJumping); //0 duration to just end event
            //set up the callback
            reachWaypointCallback = onWayPointArrive;
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
        if (InitializeTurn(0, target, isJumping || (speed != 0 && !allowTurnWhenMove))) {

            player.PlayEvent(interruptPlaylists ? -1 : 0, turnsEvent, -1, interruptIfNotJumping);
            //manually play turn event
            //player.PlayEvent(turnsEvent, interruptPlaylists, true, interruptIfNotJumping);
        }

        if (isTurning) {
            //set up the callback
            endTurnCallback = onTurnSuccess;
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
            player.PlayEvent(0, speed == 0 ? stillsEvent : movesEvent, 0, interruptIfNotJumping);
        }
    }

    void InterruptCurrentAnimations () {
        doingAnimation = false;
    }
    
    void OnEndJump (bool success) {
        isJumping = false;
        cc.enabled = true;
    }

    public void TriggerJump () {
        if (!isJumping) {
            cc.enabled = false;
            isJumping = true;
            player.SubscribeToPlayEnd(0, OnEndJump);
            player.PlayEvent(0, jumpsEvent, -1, true);
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

    void MoveCharacterController (Vector3 originalRootMotion, float deltaTime) {

        float origYvelocity = originalRootMotion.y;

        //sideways
        Vector3 sidewaysRootMotion = new Vector3(originalRootMotion.z, 0, -originalRootMotion.x);
        //get movement relevant to ground normal (avoids skips up slopes)
        Vector3 rootMotion = Vector3.Cross(sidewaysRootMotion, groundNormal);
        //add back original y velocity
        rootMotion.y += origYvelocity;
        
        //add gravity
        rootMotion.y = CalculateGravity(rootMotion.y, Physics.gravity.y, origYvelocity, deltaTime);

        if (usingRootMotion) {
            cc.Move(rootMotion);
        }
        
    }

    float currentGravity;
    float CalculateGravity(float yVelocity, float gravity, float origYvelocity, float deltaTime){
        bool rootMotionUpwards = origYvelocity > 0;
        bool fallStarted = currentGravity != 0;
    
        if (grounded) {
            currentGravity = 0;
        }

        //if the animation is trying to go upwards 
        //and we havent started falling yet dont do anyting
        if (rootMotionUpwards && !fallStarted) {
            return yVelocity;
        }

        //if falling add to downward velocity
        if (!grounded) {
            currentGravity += gravity * deltaTime * deltaTime;

            //cap downward velocity
            if (currentGravity < minYVelocity) {
                currentGravity = minYVelocity;
            }    
        }
        //if grounded stick to floor, else use calculated gravity    
        return grounded ? minYVelocity : currentGravity;
    }

    void RootMovementLoop (float deltaTime) {
        if (cc && cc.enabled) {
            MoveCharacterController(rootPosition, deltaTime);
        }
        else {
            if (usingRootMotion) {
                transform.position += rootPosition;
            }
        }
    }
    void RootRotationLoop () {
        if (usingRootMotion) {
            transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles + rootRotation.eulerAngles);
        }
        UpdateSleprTurnHelper();
    }

    void UpdateLoop (AnimatorUpdateMode checkMode, float deltaTime) {
        if (!player.overrideMovement) {
            if (turnUpdate == checkMode) RootRotationLoop();
            if (moveUpdate == checkMode) RootMovementLoop(deltaTime);
        }
    }

    void LateUpdate () {
        UpdateLoop(AnimatorUpdateMode.Normal, Time.deltaTime);
    }
    
    void CheckAutoTurn () {
        bool movingFwd = trackingWaypoint && direction == Movement.Direction.Forward;
        //if (doAutoTurn && !isTurning ) {
        if(doAutoTurn || movingFwd) {
            if (!isTurning){  
                TurnTo(movementTarget, false);
            }
        }
    }

    void CheckWayPointTracking () {
        if (trackingWaypoint && !DistanceToWaypointAboveThreshold()) {
            OnWayPointSuccess();
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
        //update when turn triggered or when moving (while tracking a waypoint)
        if (isTurning){//} || trackingWaypoint){

            Vector3 targetDir = Vector3.zero;
            
            bool targetAngleAboveHelpTurnThreshold = false;

            bool doingAutoTurnTurn = true;// doAutoTurn && isTurning;

            if (!doingAnimation || doingAutoTurnTurn){ 
                targetAngleAboveHelpTurnThreshold = Movement.TargetAngleAboveTurnThreshold (GetDirection(), transform.position, transform.forward, movementTarget, turnAngleHelpThreshold, out targetDir, out _);

            }

            if (!doingAnimation) {

                //slerp if above turn threshold
                if (targetAngleAboveHelpTurnThreshold){

                    if (usingRootMotion){

                        //Debug.Log("turning!");

                        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(targetDir), Time.deltaTime * turnHelpSpeeds[speed]);
                    
                    }
                
                
                }
                else {
                    //if below end turn if turning
                    if (isTurning) {
                        OnTurnSuccess();
                    }
                }
            }

            if (doingAutoTurnTurn){
                //if direction has changed too much from last attempt
                //end the turn (retries if above threshold)
                float curAngleFromLastAttempt = Vector3.Angle(targetDir, attemptTurnDir);
                if (curAngleFromLastAttempt > dirTurnChangeThreshold) {
                    
                        OnTurnSuccess();
                }
            }
        }
    }


    /*
        if true:
            player lets us know when the animation is done lpaying
            then sets "isPlayingTurn" to false

            slerp then turn takes over and makes sure that transform's forward 
            is within the turn threshold

        if false:    
            player lets us know if there wasnt any animations found 

    */
    void OnEndTurnAnimation (bool success) {

        Debug.Log("on end turn anim");
        doingAnimation = false;
    } 

    //calback for when turn is achieved or waypoint reached
    void OnSuccess (ref bool check, ref System.Action callback) {
        if (check) {
            if (callback != null) {
                callback();
                callback = null;
            }
            check = false;
        }
    }

    void OnTurnSuccess() {
        OnSuccess(ref isTurning, ref endTurnCallback);
    }
    void OnWayPointSuccess() {
        OnSuccess(ref trackingWaypoint, ref reachWaypointCallback);   
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

    bool InitializeTurn (int layer, Vector3 lookPoint, bool disableAnim) {
        SetMovementTarget(lookPoint);

        float angleFwd;
        Vector3 targetDir;
        
        doingAnimation = Movement.TargetAngleAboveTurnThreshold (GetDirection(), transform.position, transform.forward, movementTarget, animTurnAngleThreshold, out targetDir, out angleFwd) && !disableAnim;
        
        isTurning = doingAnimation || angleFwd >= turnAngleHelpThreshold;

        if (isTurning) {
            attemptTurnDir = targetDir;
            //Debug.Log("trying turn");
        }

        if (doingAnimation) {
            
            //player.SubscribeToAssetObjectUseEnd(animationPackName, OnEndTurnAnimation);
            player.SubscribeToPlayEnd(layer, OnEndTurnAnimation);
            //Debug.Log("subscriped");
            
            //set the angle parameter
            player[turnAngleName].SetValue(angleFwd);
            //check if turn is to right        
            player[turnRightName].SetValue(Vector3.Angle(transform.right, targetDir) <= 90);

        }
        return doingAnimation;
    }
}