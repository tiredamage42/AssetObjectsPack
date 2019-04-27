using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using AssetObjectsPacks;
using System;
using UnityEngine.AI;
using System.Linq;
using Movement.Platforms;
using Movement;
using Combat;



namespace Syd.AI {


    [RequireComponent(typeof(NavMeshAgent))]
    public class AIMovement : MovementControllerComponent{
        [Header("Debug")]
        public Transform aimDebug;
        CharacterCombat characterCombat;


        Turner turner;
        int debugPhase;
        float debugTimer;
        public float debugPhaseTime = 5.0f;




        bool attemptAim;
        bool attemptFiring;




        //char specific

        void EndDebugPhase () {
            debugPhase++;
            debugTimer = 0;
        }
        void DebugLoop (float deltaTime) {
            if (aimDebug) {
                SetInterestPoint(aimDebug.position);
                characterCombat.SetAimTarget(aimDebug.position);

                if (controller.speed == 0) {
                    turner.SetTurnTarget(aimDebug.position);
                    turner.doAutoTurn = true;
                }



                characterCombat.isAiming = characterCombat.currentGun && !controller.overrideMovement && attemptAim;
                if (characterCombat.currentGun) {
                    characterCombat.currentGun.isFiring = characterCombat.aimPercent >= .9f && attemptFiring;
                }

                // debugTimer += deltaTime;
                // if (debugTimer > debugPhaseTime) {
                //     if (debugPhase == 0) {
                //         attemptAim = true;
                //         debugPhase++;
                //     }
                //     else if (debugPhase == 1) {
                //         attemptFiring = true;
                //         debugPhase++;
                //     }
                //     else {
                //         attemptFiring = attemptAim = false;
                //         debugPhase = 0;
                //     }
                //     debugTimer = 0;
                // }
            }
            else {
                // characterCombat.isAiming = false;
                // characterCombat.currentGun.isFiring = false;
            }


            if (pathStatus != NavMeshPathStatus.PathInvalid) {
                //if (path != null) {
                    Color c = pathStatus == NavMeshPathStatus.PathPartial ? Color.yellow : Color.green;
                    for (int i = currentPathCorner; i < path.Length - 1; i++) {
                        if (i == currentPathCorner) {
                            Debug.DrawLine(transform.position, path[i], Color.red);
                        }
                        Debug.DrawLine(path[i], path[i+1], c);
                    }
                //}
            }
        }
        

        public bool agitated;

        
        public Cue demoScene;
        Vector3 destination;
        EventPlayer.EventPlayEnder endEventPlayerPlay;
        WaypointTracker waypointTracker;

        

        //char specific
        public float agentRadius = .1f;
        public float agentHeight = 2f;
        

        NavMeshAgent agent;
        Platformer platformer;
        Vector3 facePosition;
        public AIBehavior aiBehavior;    
        Vector3[] path;
        public NavMeshPathStatus pathStatus = NavMeshPathStatus.PathInvalid;
        int currentPathCorner;
        bool trackingValidPath { get { return pathStatus != NavMeshPathStatus.PathInvalid && path != null && currentPathCorner < path.Length; } }
        

        ValueTracker<bool> agitatedTracker = new ValueTracker<bool>(false);
        void CheckForLoopStateChange () {
            if (agitatedTracker.CheckValueChange(agitated)) {
                controller.UpdateLoopState();
            }
        }


        protected override void Awake () {
            base.Awake();
            eventPlayer.AddParameters ( 
                new CustomParameter[] {
                    //linked with agitated
                    new CustomParameter( "Agitated", () => agitated ), 
                } 
            );
            
            waypointTracker = GetComponent<WaypointTracker>();
            
            platformer = GetComponent<Platformer>();
            platformer.SetCallback (OnPlatformEnd);

            agent = GetComponentInChildren<NavMeshAgent>();
            agent.updateRotation = false;
            agent.updatePosition = false;


            characterCombat = GetComponent<CharacterCombat>();
            turner = GetComponent<Turner>();
        
        }
        
        void Start () {
            //start demo playlist
            Playlist.InitializePerformance("ai demo scene", demoScene, eventPlayer, true, -1, new MiniTransform(demoScene.transform.position, demoScene.transform.rotation));
        }

        void AdjustNavmeshAgentVariables () {
            agent.radius = agentRadius;
            agent.height = agentHeight;
            agent.nextPosition = transform.position;
        }


        public override void UpdateLoop(float deltaTime) {
            CheckForLoopStateChange();
            AdjustNavmeshAgentVariables();
            DebugLoop(deltaTime);
        }

        public void SetInterestPoint (Vector3 position) {
            facePosition = position;
        }

        public void GoTo (Vector3 destination, Action onArrive = null) {
            Playlist.InitializePerformance("navigation ai", aiBehavior.navigateToCue, eventPlayer, false, eventLayer, new MiniTransform(destination, Quaternion.identity), true, onArrive);
        }    


                        
        /*
            makes platforms not try and double back if the jump overshoots the intended path corner
        */
        void TriggerEndWaypointAfterPlatformChange (Vector3 myPos) {
            Vector3 nextWaypoint = path[currentPathCorner];
            float triggerRadius = aiBehavior.platformEndWaypointTriggerRadius * aiBehavior.platformEndWaypointTriggerRadius;
            //if we're within trigger end radius, 
            if (Vector3.SqrMagnitude(nextWaypoint - myPos) < triggerRadius) {
                //and the next target path corner is on our 'platform level'
                if (Platformer.SamePlatformLevels(myPos, nextWaypoint)){
                    //manually trigger waypoint arrival, 
                    // Debug.LogError("manually triggered after platform");
                    waypointTracker.ManuallyTriggerWaypointArrival("platform change");
                }
            }   
        }

        void OnPlatformEnd () {
            Vector3 myPos = transform.position;
            if (trackingValidPath) {
                TriggerEndWaypointAfterPlatformChange(myPos);
            }
            //agent next position only handles movement on navmesh
            //platforming uses off mesh links, so warp the agent to the new navmesh surface
            agent.Warp(myPos);
        }


        
        void OnWaypointArrive () {
            Vector3 lastCorner = path[currentPathCorner];
            currentPathCorner++;
            if (currentPathCorner < path.Length) {
                
                Vector3 nextCorner = path[currentPathCorner];

                platformer.TriggerPlatform(lastCorner, nextCorner);

                //calculate direction for movement
                controller.direction = agitated ? Movement.Movement.AI.CalculateMoveDirection(transform.position, nextCorner, facePosition, aiBehavior.minStrafeDistance, controller.direction) : Movement.Movement.Direction.Forward;
                
                // Debug.Log("waypoint go to");
                waypointTracker.GoTo(nextCorner, OnWaypointArrive);
            }
            else {

                // Debug.Log("destination arrive");
                OnDestinationArrive();
            }
        }

        void OnDestinationArrive () {
            endEventPlayerPlay.EndPlay("end nav");
            endEventPlayerPlay = null;   
            pathStatus = NavMeshPathStatus.PathInvalid;     
        }


        bool waitingForPath { get { return agent.pathPending; } }

        IEnumerator WaitForPathCalculation () {
            while (waitingForPath) {
                yield return null;
            }

            // Debug.Log("calculated path");
            this.path = agent.path.corners;
            this.pathStatus = agent.pathStatus;


            //skip the first one, navmesh agent path's first corner
            //is the origin position
            currentPathCorner = 0;

            OnWaypointArrive();
        }

        
        /*
            parameters:
                layer (internally set), vector3 target
        */
        
        void NavigateTo(object[] parameters) {

            
            // Debug.Log("navigating to");
            //unpack parameters
            int layer = (int)parameters[0];
            destination = (Vector3)parameters[1];
            
            //take control of the player's end play callback, to call it when arriving at destination
            endEventPlayerPlay = eventPlayer.OverrideEndPlay(layer, null, "pathfinder");
            
            //calculate path
            agent.nextPosition = transform.position;
            agent.SetDestination(destination);
            
            StartCoroutine(WaitForPathCalculation());
        }

    }
    
}











