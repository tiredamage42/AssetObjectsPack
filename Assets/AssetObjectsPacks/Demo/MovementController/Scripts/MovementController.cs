using UnityEngine;
// using AssetObjectsPacks;
using System.Collections.Generic;

namespace Movement {

    // [RequireComponent(typeof(EventPlayer))]
    public class MovementController : MonoBehaviour {

        public event System.Action onUpdateMovementState;
    
    //change if you've changed the pack name or the animatins pack itself
    public const string animationPackName = "Animations";

    public MovementBehavior behavior;
    public Movement.Direction direction;
    [Range(0,2)] public int speed;
    [Range(0,1)] public int stance;

    // EventPlayer eventPlayer;
    
    public Vector3 moveDireciton { get { return Movement.GetRelativeTransformDirection(direction, transform); } } 
    
    void Awake () {     
        // eventPlayer = GetComponent<EventPlayer>();  
        
        AddChangeLoopStateValueCheck ( () => speed, "Speed" );
        AddChangeLoopStateValueCheck ( () => direction, "direction" );
        AddChangeLoopStateValueCheck ( () => stance, "Stance" );

    }
    void Update () {
        CheckSpeedDirectionChanges();
    }


    HashSet<int> scriptedMoveRequests = new HashSet<int>();
    public void EnableScriptedMove (int id, bool enabled) {
        if (enabled) {
            if (!scriptedMoveRequests.Contains(id)) scriptedMoveRequests.Add(id);
        }
        else {
            scriptedMoveRequests.Remove(id);
        }
    }

    
    public bool scriptedMove { get { return scriptedMoveRequests.Count > 0; } }

    // public bool scriptedMove { get { return _scriptedMove || eventPlayer.cueMoving; } }
    // public bool _scriptedMove;
    /*
        parameters:
            layer (internally set), override move

        overrides movement so no other movements can trigger
    */
    // void EnableScriptedMovement (object[] parameters) {
    //     //moveEnabled = (bool)parameters[1];    
    //     EnableScriptedMovement((bool)parameters[1]);
    // }    


    // public void EnableScriptedMovement(bool enabled) {
    //     _scriptedMove = enabled;
    // }

    public void StopMovementManual () {
        speed = 0;
        direction = Movement.Direction.Forward;
    }

        

    public void StartMovementManual(int speed=-1, int direction=-1) {
        this.speed = speed <= 0 ? CalculateSpeed(speed) : speed;
        this.direction = direction < 0 ? this.direction : ((Movement.Direction)direction);
    }

        
    int CalculateSpeed (int newSpeed) {
        if (newSpeed <= 0) {
            //use current or walk
            return Mathf.Max(speed, 1);
        }
        return newSpeed;
    }


    HashSet<ValueTracker> valuesChangeLoopStates = new HashSet<ValueTracker>();

    /*
        add a method to get a variable, when the variable changes, the move controller will update its
        loops
            e.g. track an aiming variable, or a grounded variable
    */
    public void AddChangeLoopStateValueCheck (System.Func<object> valueGetter, string displayName) {
        valuesChangeLoopStates.Add( new ValueTracker( valueGetter, valueGetter(), displayName ) );
    }

    /*
        override the value checks so they're considered "not changed"
        used when we dont want any of the changes to switch our loop states
        
        e.g. 
            when a cue decides our loop for us
    */
    public void ForceNonChangeForValueChecks () {

        foreach (var vt in valuesChangeLoopStates) {
            vt.UpdateLastValue();
        }
    }

    bool ShouldUpdateLoops () {

        bool debug = true;
        bool shouldChange = false;

        // loop through all so they update last value
        foreach (var vt in valuesChangeLoopStates) {
            if (vt.CheckValueChange(debug)) {
                shouldChange = true;
            }
        }
        return shouldChange;
    }

    void CheckSpeedDirectionChanges() {
        if (speed == 0) {
            direction = Movement.Direction.Forward;
        }

        if (ShouldUpdateLoops()) {
            if (onUpdateMovementState != null) {
                onUpdateMovementState();
            }
        }
    }
}







}
