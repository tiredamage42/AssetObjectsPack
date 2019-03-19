using System.Collections;
using UnityEngine;
using AssetObjectsPacks;


[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(EventPlayer))]
public class MovementController : MonoBehaviour {
    
    //change if you've changed the pack name or the animatins pack itself
    public const string animationPackName = "Animations";

    public MovementBehavior behavior;
    public Movement.Direction direction;
    [Range(0,2)] public int speed;
    [Range(0,1)] public int stance;

    public bool usingRootMotion = true;
    public bool calculateMoveSloped = true;
    public bool useGravity = true;
    public bool usePhysicsController = true;
    
    Animator anim;
    CharacterController cc;
    EventPlayer eventPlayer;
    Vector3 animDeltaPosition;
    Quaternion animDeltaRotation;
    float currentGravity;

    public int eventLayer = 0;



    public Vector3 moveDireciton { get { return Movement.GetRelativeTransformDirection(direction, transform); } } 
            
    

    
    int lastSpeed = -1;
    Movement.Direction lastDirection = Movement.Direction.Backwards;
    const string speedName = "Speed", directionName = "Direction", stanceName = "Stance";


    void OnDrawGizmos()
    {
        if (usePhysicsController) {
            if (cc != null) {
                Gizmos.color = Color.green;
                Gizmos.DrawWireCube(transform.position + cc.center, new Vector3(cc.radius * 2, cc.height, cc.radius * 2));
            }
        }
    }

    void Awake () {     
        eventPlayer = GetComponent<EventPlayer>();  
        eventPlayer.AddParameters(
            new CustomParameter[] {
                // paremeters linked with script properties:
                new CustomParameter ( speedName, () => speed ),
                new CustomParameter ( directionName, () => (int)direction ),
                new CustomParameter ( stanceName, () => stance ),
            }
        );
    
        cc = GetComponent<CharacterController>();
        anim = GetComponent<Animator>();
        anim.applyRootMotion = false;   
    }
    void Update () {
        CheckSpeedDirectionChanges();
    }
    void FixedUpdate () {
        CheckGrounded();
        CheckCapsuleComponentEnabled();
        UpdateLoop(AnimatorUpdateMode.AnimatePhysics, Time.fixedDeltaTime);
    }
    void LateUpdate () {
        UpdateLoop(AnimatorUpdateMode.Normal, Time.deltaTime);
    }

    void OnAnimatorMove () {
        animDeltaPosition = anim.deltaPosition;
        animDeltaRotation = anim.deltaRotation;
    }


    public bool overrideMovement { get { return overrideMove || eventPlayer.cueMoving; } }
    public bool overrideMove;
    /*
        parameters:
            layer (internally set), override move

        overrides movement so no other movements can trigger
    */
    void OverrideMovement_Cue (object[] parameters) {
        overrideMove = (bool)parameters[1];    
    }



    /*
        Messages for setting variables via cues and playlists    

        parameters:
            layer (internally set), enabled, delaytime (optional), duration (optional)
    */
    void SetByMessage (object[] parameters, System.Action<bool, float, float> enableFN) { 
        bool enabledValue = (bool)parameters[1];
        float delayTime = parameters.Length > 2 ? (float)parameters[2] : 0;
        float duration = parameters.Length > 3 ? (float)parameters[3] : -1;
        enableFN(enabledValue, delayTime, duration); 
    }
    void EnableAllPhysics(object[] parameters) { SetByMessage(parameters, EnableAllPhysics); }
    void EnableSlopeMove(object[] parameters) { SetByMessage(parameters, EnableSlopeMove); }
    void EnableGravity(object[] parameters) { SetByMessage(parameters, EnableGravity); }
    void EnablePhysics(object[] parameters) { SetByMessage(parameters, EnablePhysics); }
    void EnableRootMotion(object[] parameters) { SetByMessage(parameters, EnableRootMotion); }
    

    public void EnableSlopeMove(bool enabled, float delay=0, float duration=-1) {
        EnableAfterDelay(EnableSlopeMove, enabled, delay, duration, e => calculateMoveSloped = e );
    }
    public void EnableGravity(bool enabled, float delay=0, float duration=-1) {
        EnableAfterDelay(EnableGravity, enabled, delay, duration, e => useGravity = e );
    }
    public void EnableRootMotion(bool enabled, float delay=0, float duration=-1) {
        EnableAfterDelay(EnableRootMotion, enabled, delay, duration, e => usingRootMotion = e );
    }
    public void EnablePhysics(bool enabled, float delay=0, float duration=-1) {
        EnableAfterDelay(EnablePhysics,
            enabled, delay, duration, 
            (e) => {
                usePhysicsController = e;
                if (cc != null) {
                    cc.enabled = e;
                }
            } 
        );
    }
    public void EnableAllPhysics(bool enabled, float delay=0, float duration=-1) {
        EnableAfterDelay(EnableAllPhysics,
            enabled, delay, duration,
            (e) => {
                EnablePhysics(e);
                EnableGravity(e);
                EnableSlopeMove(e);
            } 
        );
    }

    

    IEnumerator EnableAfterDelay (System.Action<bool, float, float> self, bool enabled, float delay, float duration) {
        yield return new WaitForSeconds(delay);
        self(enabled, 0, duration);
    }

    /*
        self: the method calling this
    */
    protected void EnableAfterDelay(System.Action<bool, float, float> self, bool enabled, float delay, float duration, System.Action<bool> enableFN) {
        if (delay > 0) {
            StartCoroutine(EnableAfterDelay(self, enabled, delay, duration));
            return;
        }
        enableFN(enabled);
        if (duration >= 0) {
            StartCoroutine(EnableAfterDelay(self, !enabled, duration, -1));
        }
    }

    void CheckCapsuleComponentEnabled () {
        if (cc == null) 
            return;
        
        if (eventPlayer.cueMoving) {
            EnableCapsuleComponent(false);
        }
        else {
            if (usePhysicsController) {
                EnableCapsuleComponent(true);
            }
        }
    }
    void EnableCapsuleComponent(bool enabled) {
        if (cc.enabled != enabled) {
            cc.enabled = enabled;
        }
    }

    void RootMovementLoop (float deltaTime) {

        Vector3 rootMotion = CalculateRootMotion();
        
        //add gravity
        if (useGravity) {
            rootMotion.y = CalculateGravity(rootMotion.y, deltaTime);        
        }
        
        //use physics controller
        if (usePhysicsController && cc != null && cc.enabled) {
            cc.Move(rootMotion);
        }
        else { //just move transform
            
            //adjust to stay on ground if grounded
            if (useGravity && grounded) {
                float curY = transform.position.y;
                if (curY + rootMotion.y < floorY) {
                    rootMotion.y = floorY - curY;
                } 
            }
            transform.position += rootMotion;
        }
    }

    void UpdateLoop (AnimatorUpdateMode checkMode, float deltaTime) {
        
        if (!eventPlayer.cueMoving && usingRootMotion) {
            if (behavior.turnUpdate == checkMode) {
                transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles + animDeltaRotation.eulerAngles);
            }
            if (behavior.moveUpdate == checkMode)
                RootMovementLoop(deltaTime);
        }
    }
    
    Vector3 CalculateRootMotion() {
        
        Vector3 rootMotion = animDeltaPosition;
        if (calculateMoveSloped) {
            //sidways without y velocity
            Vector3 sidewaysRootMotion = new Vector3(rootMotion.z, 0, -rootMotion.x);
        
            //get movement relevant to ground normal (avoids skips up slopes)
            rootMotion = Vector3.Cross(sidewaysRootMotion, groundNormal);
            //add back original y velocity
            rootMotion.y += animDeltaPosition.y;
        }
        return rootMotion;
    } 
    
    float CalculateGravity(float yVelocity, float deltaTime){
        bool rootMotionUpwards = animDeltaPosition.y > 0;
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
            currentGravity += Physics.gravity.y * deltaTime * deltaTime;

            //cap downward velocity
            if (currentGravity < behavior.minYVelocity) {
                currentGravity = behavior.minYVelocity;
            }    
        }
        
        //if grounded stick to floor, else use calculated gravity    
        return grounded ? behavior.minYVelocity : currentGravity;
    }







    public bool grounded;    
    public Vector3 groundNormal = Vector3.up;
    public float floorY;
    const float groundCheckBuffer = .1f;
    void CheckGrounded () {
        float distanceCheck = groundCheckBuffer + (grounded ? behavior.groundDistanceCheckGrounded : behavior.groundDistanceCheckAir);
        Ray ray = new Ray(transform.position + Vector3.up * groundCheckBuffer, Vector3.down);
        Debug.DrawRay(ray.origin, ray.direction * distanceCheck, grounded ? Color.green : Color.red);

        grounded = false;
        groundNormal = Vector3.up;
        floorY = -999;
        RaycastHit hit;
        if (Physics.SphereCast(ray, behavior.groundRadiusCheck, out hit, distanceCheck, behavior.groundLayerMask)) {
            groundNormal = hit.normal;
            floorY = hit.point.y;
            if (Vector3.Angle(groundNormal, Vector3.up) <= behavior.maxGroundAngle) {
                grounded = true;
            }
        }
    }   









    

    /*
        parameters:
            layer (internally set), cue
    
        stops all movement and plays a loop animation for a single frame, 
        so whatever animaition plays next will exit into a "still" loop

        sets the player to stop the cue immediately after playing that frame
    */
    void StopMovement_Cue (object[] parameters) {
        SetDirection(Movement.Direction.Forward, true);
        SetSpeed(0, true);

        int layer = (int)parameters[0];
        Cue cue = (Cue)parameters[1];

        // if cue doesn't have any animation events, override the player with this controller's events
        if (!cue.GetEventByName(animationPackName)) {
            eventPlayer.OverrideEventToPlay(layer, behavior.stillsEvent);    
        }
    }

    /*
        parameters:
            layer (internally set), cue, int speed (optional), int direction (optional, current if not there)

        starts playing movement animation loop for single frame

        cue ends right after
    */
    void StartMovement_Cue(object[] parameters) {
        int l = parameters.Length;

        //unpack parameters
        int layer = (int)parameters[0];
        Cue cue = (Cue)parameters[1];
        int newSpeed = (l > 2) ? (int)parameters[2] : -1;
        Movement.Direction newDirection = (Movement.Direction)((l > 3) ? ((int)parameters[3]) : (int)direction);
    
        //so change doesnt register and override cue animation
        SetSpeed(newSpeed <= 0 ? CalculateSpeed(newSpeed) : newSpeed, true);
        SetDirection(newDirection, true);

        // if cue doesn't have any animation events, override the player with this controller's events
        if (!cue.GetEventByName(animationPackName)) {
            eventPlayer.OverrideEventToPlay(layer, behavior.movesEvent);    
        }
    }
    
    public int CalculateSpeed (int newSpeed) {
        if (newSpeed <= 0) {
            //use current or walk
            return Mathf.Max(speed, 1);
        }
        return newSpeed;
    }

    void CheckSpeedDirectionChanges() {
        bool changedSpeed = SetSpeed(speed, true);
        bool changedDirection = SetDirection(speed == 0 ? Movement.Direction.Forward : direction, true);
        bool changed = changedSpeed || changedDirection;

        if (changed) {
            //immediately play the loop unless we're jumping or overriding movement
            bool asInterruptor = !overrideMovement;
            Debug.Log("playign speed or direction change");
            Playlist.InitializePerformance("move controlelr speed change", speed == 0 ? behavior.stillCue : behavior.moveCue, eventPlayer, false, eventLayer, Vector3.zero, Quaternion.identity, asInterruptor);
        }
    }
    public bool SetDirection(Movement.Direction direction, bool forceChange=false) {
        bool changed = direction != lastDirection;
        this.direction = direction;
        if (forceChange) {
            lastDirection = direction;
        }
        return changed;
    }
    public bool SetSpeed(int speed, bool forceChange=false) {
        bool changed = speed != lastSpeed;
        this.speed = speed;
        if (forceChange) {
            lastSpeed = speed;
        }
        return changed;
    }












}