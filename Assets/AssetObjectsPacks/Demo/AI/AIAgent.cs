using UnityEngine;
using AssetObjectsPacks;
using Movement;


namespace Syd.AI {

    public class AIAgent : MonoBehaviour{

        [HideInInspector] public Vector3 interestPoint;
        public bool agitated;
        public Cue demoScene;
        public AIBehavior aiBehavior;    
        
        EventPlayer eventPlayer;
        MovementController movementController;

        void Awake () {
            eventPlayer = GetComponent<EventPlayer>();

            eventPlayer.AddParameter ( new CustomParameter( "Agitated", () => agitated ) );   

            movementController = GetComponent<MovementController>();
            movementController.AddChangeLoopStateValueCheck( () => agitated );
        }

        
        void Start () {
            //start demo playlist
            Playlist.InitializePerformance("ai demo scene", demoScene, eventPlayer, true, -1, new MiniTransform(demoScene.transform.position, demoScene.transform.rotation));
        }

        public void SetInterestPoint (Vector3 position) {
            interestPoint = position;
        }
            
    }
    
}











