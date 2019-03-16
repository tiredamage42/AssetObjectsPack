using System.Collections;
using UnityEngine;
using AssetObjectsPacks;

public class MovementController : MonoBehaviour {

    void OnDrawGizmos () {

        rootMotion.OnDrawGizmos();
        turner.OnDrawGizmos();

    }

    //change if you've changed the pack name or the animatins pack itself
    public const string animationPackName = "Animations";

    //messed up and made backwards last so directions are 
    //0 - fwd / 1 - left / 2 - right / 3 - back
    public enum Direction  {
        Forward=0, Backwards=3, Left=1, Right=2, Calculated=-1
    };
    
    public MovementBehavior behavior;
    public Direction direction;
    [Range(0,2)] public int speed;
    [Range(0,1)] public int stance;
    public RootMotion rootMotion = new RootMotion();
    public Mover mover = new Mover();
    public Turner turner = new Turner();
    public Jumper jumper = new Jumper();
    public Platformer platformer = new Platformer();
    
    [HideInInspector] public EventPlayer eventPlayer;
    
    int lastSpeed = -1;
    Direction lastDirection = Direction.Backwards;
    const string speedName = "Speed", directionName = "Direction", stanceName = "Stance";
    
    void OnAnimatorMove () {
        rootMotion.OnAnimatorMove();
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
        rootMotion.Initialize(this);
        jumper.Initialize(this);
        turner.Initialize(this);
        mover.Initialize(this);
        platformer.Initialize(this);
    }
    void Update () {
        CheckSpeedDirectionChanges();
        rootMotion.Update();
        //mover.Update();
        turner.Update();
        platformer.Update();
    }
    void FixedUpdate () {
        CheckGrounded();

        rootMotion.FixedUpdate();
        turner.FixedUpdate();
        mover.FixedUpdate();
    }
    void LateUpdate () {
        rootMotion.LateUpdate();
        turner.LateUpdate();
        mover.LateUpdate();
    }
    
    /*
        Messages for setting variables via cues and playlists    
    */

    /*
        parameters:
            enabled, delaytime (optional)
    */
    void EnableAllPhysics(object[] parameters) { rootMotion.EnableAllPhysics(parameters); }
    void EnableSlopeMove(object[] parameters) { rootMotion.EnableSlopeMove(parameters); }
    void EnableGravity(object[] parameters) { rootMotion.EnableGravity(parameters); }
    void EnablePhysics(object[] parameters) { rootMotion.EnablePhysics(parameters); }
    void EnableRootMotion(object[] parameters) { rootMotion.EnableRootMotion(parameters); }
    
    /*
        parameters:
            layer (internally set), cue
    
        stops all movement and plays a loop animation for a single frame, 
        so whatever animaition plays next will exit into a "still" loop

        sets the player to stop the cue immediately after playing that frame
    */
    void StopMovement_Cue (object[] parameters) {
        SetDirection(Direction.Forward);
        SetSpeed(0);
        CheckForCueEventOverride(parameters, behavior.stillsEvent, eventPlayer);
    }


     /*
        parameters:
            layer (internally set), cue, int speed (optional), int direction (optional, forward if not there)

        starts playing movement animation loop for single frame

        cue ends right after
    */
    void StartMovement_Cue(object[] parameters) {
        int l = parameters.Length;

        //int layerToUse = (int)parameters[0];
        int newSpeed = (l > 2) ? (int)parameters[2] : -1;
        MovementController.Direction newDirection = (MovementController.Direction)((l > 3) ? ((int)parameters[3]) : 0);
    
        //so change doesnt register and override cue animation
        SetSpeed(CurrentSpeedOrDefaultSpeed(newSpeed));
        SetDirection(newDirection);

        // check if cue doesnt have any event specified for animations
        // if not then use the move controller's moves event
        MovementController.CheckForCueEventOverride(parameters, behavior.movesEvent, eventPlayer);
    }
    
    public int CurrentSpeedOrDefaultSpeed (int newSpeed) {
        //if speed is set to 0 or using last and last is 0 
        //then automatically set walking speed or run 
        int currentSpeed = speed;
        if (newSpeed == 0 || (newSpeed < 0 && currentSpeed == 0)) {
            newSpeed = 1;
        }
        //if not negative set new speed
        if (newSpeed > 0) {
            return newSpeed;
            //movementController.speed = newSpeed;
        }
        else {
            return currentSpeed;
        }
    }
    





    /*
        parameters:
            layer (internally set), cue, vector3 target, int speed (optional), int direction (optional, calculated if not there)

        makes the controller set up waypoint tracking to go to the specified position
        (cue's runtime interest position)

        the cue ends when the transform is within the arrive threshold
    */
    void MovementControllerGoTo_Cue(object[] parameters) {
        mover.MovementControllerGoTo_Cue(parameters);
    }
    /*
        parameters:
            layer (internally set), cue, vector3 target
            
        makes the controller turn, so the forward faces the cue's runtime interest position

        the cue ends when this transform's forward is within the turn help angle 
        (relative to the direction towards the movement target)
    */
    void MovementControllerTurnTo_Cue (object[] parameters) {
        turner.MovementControllerTurnTo_Cue(parameters);
    }



    /*
        messages sent from the cues set up to work with this movement controller
        have the cue itself as the second parameter
        
        if that cue doesn't have any animation events, we'll override the player with 
        this controller's events
    */
    public static void CheckForCueEventOverride (object[] parameters, AssetObjectsPacks.Event overrideEvent, EventPlayer player) {
        //the layer to use is supplied by the message as the first parameter
        int playerLayerToUse = (int)parameters[0];

        AssetObjectsPacks.Cue cue = (AssetObjectsPacks.Cue)parameters[1];
        
        if (cue.GetEventByName(animationPackName) == null) {
            //use playlist layer
            //int playerLayerToUse = -1;
            //Debug.Log("overriding with" + overrideEvent);
            player.OverrideEventToPlay(playerLayerToUse, animationPackName, overrideEvent);    
        }
    }


    void CheckSpeedDirectionChanges() {
        bool changedSpeed = SetSpeed(speed);
        bool changedDirection = SetDirection(speed == 0 ? Direction.Forward : direction);
        bool changed = changedSpeed || changedDirection;

        if (changed) {
            
            //dont interrupt playlists
            int playerLayerToUse = 0;

            //if looping, player ends immediately after play 
            //(looping animation keeps playing and end move is calculated manually
            //float duration = 0;
            
            //immediately play the loop unless we're jumping
            bool asInterruptor = !jumper.isJumping;

            if (asInterruptor) {
                turner.InterruptTurn();
                eventPlayer.InterruptLayer(playerLayerToUse);
            }

            Playlist.InitializePerformance(new Cue[] { speed == 0 ? behavior.stillCue : behavior.moveCue }, new EventPlayer[] {eventPlayer}, Vector3.zero, Quaternion.identity, false, playerLayerToUse, null);

            //eventPlayer.PlayEvent(playerLayerToUse, speed == 0 ? behavior.stillsEvent : behavior.movesEvent, duration, asInterruptor);
        }
    }
    public bool SetDirection(Direction direction) {
        bool changed = direction != lastDirection;
        this.direction = direction;
        lastDirection = direction;
        return changed;
    }
    public bool SetSpeed(int speed) {
        bool changed = speed != lastSpeed;
        this.speed = speed;
        lastSpeed = speed;
        return changed;
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
}