using UnityEngine;
using AssetObjectsPacks;
using Movement;


namespace Game.AI {

    public class AIAgent : MonoBehaviour{

        public bool paused;
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
            movementController.AddChangeLoopStateValueCheck( () => agitated, "agitated" );
        }

        void OnEndDemoScene () {
            Debug.LogError("done with demo scene");
            Debug.Break();
        }

        
        void Start () {
            // playlist performances should pause ai
            paused = true;
            //start demo playlist
            Playlist.InitializePerformance("ai demo scene", demoScene, eventPlayer, true, -1, new MiniTransform(demoScene.transform.position, demoScene.transform.rotation), true, OnEndDemoScene);
        }

        public void SetInterestPoint (Vector3 position) {
            interestPoint = position;
        }
            
    }
    
}











