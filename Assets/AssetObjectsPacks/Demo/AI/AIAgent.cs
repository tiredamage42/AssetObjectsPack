// using System.Collections;
// using System.Collections.Generic;
using UnityEngine;

using AssetObjectsPacks;
// using System;
// using UnityEngine.AI;
// using System.Linq;
using Movement;
//using Combat;


namespace Syd.AI {

    public class AIAgent : MonoBehaviour{

        [HideInInspector] public Vector3 interestPoint;
        public bool agitated;
        public Cue demoScene;
        public AIBehavior aiBehavior;    
        
        // ValueTracker<bool> agitatedTracker = new ValueTracker<bool>(false);
        // void CheckForLoopStateChange () {
        //     if (agitatedTracker.CheckValueChange(agitated)) {
        //         controller.UpdateLoopState();
        //     }
        // }

        EventPlayer eventPlayer;
        MovementController movementController;

        void Awake () {
            
            eventPlayer = GetComponent<EventPlayer>();

            eventPlayer.AddParameters ( 
                new CustomParameter[] {
                    //linked with agitated
                    new CustomParameter( "Agitated", () => agitated ), 
                } 
            );   

            movementController = GetComponent<MovementController>();
            movementController.AddChangeLoopStateValueCheck( () => agitated );

            
        }

        
        void Start () {
            //start demo playlist
            Playlist.InitializePerformance("ai demo scene", demoScene, eventPlayer, true, -1, new MiniTransform(demoScene.transform.position, demoScene.transform.rotation));
        }

        

        // public override void UpdateLoop(float deltaTime) {
        //     // CheckForLoopStateChange();
        // }

        public void SetInterestPoint (Vector3 position) {
            interestPoint = position;
        }
             

    }
    
}











