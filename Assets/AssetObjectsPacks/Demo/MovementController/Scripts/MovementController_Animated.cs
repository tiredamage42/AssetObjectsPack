using UnityEngine;
using AssetObjectsPacks;

namespace Movement {

    [RequireComponent(typeof(EventPlayer))]
    public class MovementController_Animated : MonoBehaviour {

        MovementController movementController;
        EventPlayer eventPlayer;
        public int eventLayer = 0;
            
        const string speedName = "Speed", directionName = "Direction", stanceName = "Stance";


        void Awake () {     
                    
            movementController = GetComponent<MovementController>();  
            
            eventPlayer = GetComponent<EventPlayer>();  
            eventPlayer.AddParameters(
                new CustomParameter[] {
                    // paremeters linked with script properties:
                    new CustomParameter ( speedName, () => movementController.speed ),
                    new CustomParameter ( directionName, () => (int)movementController.direction ),
                    new CustomParameter ( stanceName, () => movementController.stance ),
                }
            );
        }

        void OnEnable () {
            movementController.onUpdateMovementState += UpdateLoopState;
        }
        void OnDisable () {
            movementController.onUpdateMovementState -= UpdateLoopState;
        }


        void Update () {
            movementController.EnableScriptedMove(GetInstanceID(), cueMoveOverride || eventPlayer.cueMoving);
        }

        bool cueMoveOverride;
        /*
            parameters:
                layer (internally set), override move

            overrides movement so no other movements can trigger
        */
        void EnableScriptedMovement (object[] parameters) {
            //moveEnabled = (bool)parameters[1];    
            EnableScriptedMovement((bool)parameters[1]);
        }    

        public void EnableScriptedMovement(bool enabled) {
            cueMoveOverride = enabled;
        }






            
        /*
            parameters:
                layer (internally set), override move

            overrides movement so no other movements can trigger
        */
        // void EnableScriptedMovement (object[] parameters) {
        //     movementController.EnableScriptedMovement((bool)parameters[1]);
        // }    


        /*
            parameters:
                layer (internally set)
        
            stops all movement and plays a loop animation for a single frame, 
            so whatever animaition plays next will exit into a "still" loop

            sets the player to stop the cue immediately after playing that frame
        */
        void StopMovement (object[] parameters) {

            
            movementController.StopMovementManual();

            //force change so change doesnt register and override cue animation
            // if any supplied
            bool suppliedAnimations = (bool)parameters[1];
            if (suppliedAnimations) {
                movementController.ForceNonChangeForValueChecks();
            }
            
        }


        /*
            parameters:
                layer (internally set), bool suppliedAnimations, int speed (optional), int direction (optional, current if not there)

            starts playing movement animation loop for single frame

            cue ends right after
        */
        void StartMovement(object[] parameters) {
            int l = parameters.Length;

            //unpack parameters
            int newSpeed = (l > 2) ? (int)parameters[2] : -1;
            int newDirection = (l > 3) ? ((int)parameters[3]) : -1;

            movementController.StartMovementManual(newSpeed, newDirection);
            
            // force changed so change in speed or direction doesnt register and override cue animation
            // if any supplied
            bool suppliedAnimations = (bool)parameters[1];
            if (suppliedAnimations) {
                movementController.ForceNonChangeForValueChecks();
            }
        }

        void UpdateLoopState () {
            //immediately play the loop unless we're jumping or overriding movement
            bool asInterruptor = false;// !scriptedMove;
            
            Playlist.InitializePerformance("update Loop state", movementController.speed == 0 ? movementController.behavior.stillEvents : movementController.behavior.moveEvents, eventPlayer, false, eventLayer, new MiniTransform(Vector3.zero, Quaternion.identity), forceInterrupt : asInterruptor, onEndPerformanceCallbacks : null) ;
        }
    }
}
