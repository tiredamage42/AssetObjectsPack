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
        
        public bool allowBackwardsMovement;
        Turner turner;
        

        void DebugLoop (float deltaTime) {
           
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
        

        Vector3 destination;
        EventPlayer.EventPlayEnder endEventPlayerPlay;
        WaypointTracker waypointTracker;

        

        //char specific
        public float agentRadius = .1f;
        public float agentHeight = 2f;
        

        NavMeshAgent agent;
        Platformer platformer;
        Vector3[] path;
        public NavMeshPathStatus pathStatus = NavMeshPathStatus.PathInvalid;
        int currentPathCorner;
        bool trackingValidPath { get { return pathStatus != NavMeshPathStatus.PathInvalid && path != null && currentPathCorner < path.Length; } }
        
        CharacterAnimatorMover animationMover;

        AIAgent aiAgent;
        
        

        /*
            if we have a long way to turn to face our target direction, just use our target direction as movement
            
            (keeps us from doing large curves while turning, preventing falling off cliffs and whatnot)
        */
    
        Vector3 ModifyCharacterMovement(Vector3 originalMovement) {
            
            if (controller.speed == 0 || !turner.isSlerpingTransformRotation) {
                return originalMovement;
            }
            if (turner.angleDifferenceWithTarget < 45){
                return originalMovement;
            }
            Debug.LogWarning("modifying move until turned");

            Vector3 newMove = turner.targetTurnDirection.normalized * originalMovement.magnitude;
            Debug.DrawLine(transform.position + Vector3.up * .25f, (transform.position + Vector3.up * .25f) + newMove.normalized * 10, Color.yellow);
            // Debug.Break();
            
            return newMove;
        }




        protected override void Awake () {
            base.Awake();
            
            aiAgent = GetComponent<AIAgent>();

            waypointTracker = GetComponent<WaypointTracker>();
            
            platformer = GetComponent<Platformer>();
            platformer.onPlatformEnd += OnPlatformEnd;
            
            agent = GetComponentInChildren<NavMeshAgent>();
            agent.updateRotation = false;
            agent.updatePosition = false;

            turner = GetComponent<Turner>();

            animationMover = GetComponent<CharacterAnimatorMover>();
            animationMover.SetMoveModifier (ModifyCharacterMovement);
        }

        
        
        void AdjustNavmeshAgentVariables () {
            agent.radius = agentRadius;
            agent.height = agentHeight;
            agent.nextPosition = transform.position;
        }


        public override void UpdateLoop(float deltaTime) {
            AdjustNavmeshAgentVariables();
            if (controller.speed == 0) {
                turner.SetTurnTarget(aiAgent.interestPoint);
                turner.doAutoTurn = true;
            }


            DebugLoop(deltaTime);
        }

        
        public void GoTo (Vector3 destination, Action onArrive = null) {
            Playlist.InitializePerformance("navigation ai", aiAgent.aiBehavior.navigateToCue, eventPlayer, false, eventLayer, new MiniTransform(destination, Quaternion.identity), true, onArrive);
        }    


                        
        /*
            makes platforms not try and double back if the jump overshoots the intended path corner
        */
        void TriggerEndWaypointAfterPlatformChange (Vector3 myPos) {
            Vector3 nextWaypoint = path[currentPathCorner];
            float triggerRadius = aiAgent.aiBehavior.platformEndWaypointTriggerRadius * aiAgent.aiBehavior.platformEndWaypointTriggerRadius;
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
                controller.direction = aiAgent.agitated ? Movement.Movement.AI.CalculateMoveDirection(transform.position, nextCorner, aiAgent.interestPoint, aiAgent.aiBehavior.minStrafeDistance, controller.direction, allowBackwardsMovement) : Movement.Movement.Direction.Forward;
                
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











