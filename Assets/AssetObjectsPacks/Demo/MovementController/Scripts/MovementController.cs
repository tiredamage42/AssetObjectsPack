using System.Collections;
using UnityEngine;
using AssetObjectsPacks;


//[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(EventPlayer))]
public class MovementController : MonoBehaviour {
    
    //change if you've changed the pack name or the animatins pack itself
    public const string animationPackName = "Animations";

    public MovementBehavior behavior;
    public Movement.Direction direction;
    [Range(0,2)] public int speed;
    [Range(0,1)] public int stance;

    EventPlayer eventPlayer;
    public int eventLayer = 0;

    public Vector3 moveDireciton { get { return Movement.GetRelativeTransformDirection(direction, transform); } } 
            
    int lastSpeed = -1;
    Movement.Direction lastDirection = Movement.Direction.Backwards;
    const string speedName = "Speed", directionName = "Direction", stanceName = "Stance";


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
    }
    void Update () {
        CheckSpeedDirectionChanges();
    }
    

    public bool overrideMovement { get { return overrideMove || eventPlayer.cueMoving; } }
    public bool overrideMove;
    /*
        parameters:
            layer (internally set), override move

        overrides movement so no other movements can trigger
    */
    void OverrideMovement (object[] parameters) {
        overrideMove = (bool)parameters[1];    
    }    

    /*
        parameters:
            layer (internally set)
    
        stops all movement and plays a loop animation for a single frame, 
        so whatever animaition plays next will exit into a "still" loop

        sets the player to stop the cue immediately after playing that frame
    */
    void StopMovement (object[] parameters) {
        //force change so change doesnt register and override cue animation
        SetDirection(Movement.Direction.Forward, true);
        SetSpeed(0, true);
    }

    /*
        parameters:
            layer (internally set), int speed (optional), int direction (optional, current if not there)

        starts playing movement animation loop for single frame

        cue ends right after
    */
    void StartMovement(object[] parameters) {
        int l = parameters.Length;
        //unpack parameters
        int layer = (int)parameters[0];
        int newSpeed = (l > 1) ? (int)parameters[1] : -1;
        Movement.Direction newDirection = (Movement.Direction)((l > 2) ? ((int)parameters[2]) : (int)direction);
        //force change so change doesnt register and override cue animation
        SetSpeed(newSpeed <= 0 ? CalculateSpeed(newSpeed) : newSpeed, true);
        SetDirection(newDirection, true);
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
            //Debug.Log("playign speed or direction change");
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