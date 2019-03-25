using UnityEngine;
using System.Collections;
using AssetObjectsPacks;
using System;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class AIMovement : MovementControllerComponent{
    [Header("Debug")]
    public Transform debugTransformLook;



    //char specific
    void DebugLoop () {
        if (debugTransformLook) {
            SetInterestPoint(debugTransformLook.position);
        }    

        if (pathStatus != NavMeshPathStatus.PathInvalid) {
            if (path != null) {
                Color c = pathStatus == NavMeshPathStatus.PathPartial ? Color.yellow : Color.green;
                for (int i = currentPathCorner; i < path.Length - 1; i++) {
                    if (i == currentPathCorner) {
                        Debug.DrawLine(transform.position, path[i], Color.red);
                    }
                    Debug.DrawLine(path[i], path[i+1], c);
                }
            }
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
    
    }
    
    void Start () {
        //start demo playlist
        Playlist.InitializePerformance("ai demo scene", demoScene, eventPlayer, true, -1, demoScene.transform.position, demoScene.transform.rotation);
    }

    void HandleAutoPlatforming () {
        bool nextWaypointNotOnLevel = false;
        if (trackingValidPath && controller.speed > 0) {
            Vector3 nextWaypoint = path[currentPathCorner];
            nextWaypointNotOnLevel = !Platformer.SamePlatformLevels(transform.position, nextWaypoint);
            //if (nextWaypointNotOnLevel) {
            //    Debug.Log("yo");
            //}
        }
        platformer.doAutoPlatform = nextWaypointNotOnLevel;
    }
    void AdjustNavmeshAgentVariables () {
        agent.radius = agentRadius;
        agent.height = agentHeight;
        agent.nextPosition = transform.position;
    }


    public override void UpdateLoop(float deltaTime) {
        HandleAutoPlatforming();
        AdjustNavmeshAgentVariables();
        DebugLoop();
    }
    

    public void SetInterestPoint (Vector3 position) {
        facePosition = position;
    }


    public void GoTo (Vector3 destination, Action onArrive = null) {
        Playlist.InitializePerformance("navigation ai", aiBehavior.navigateToCue, eventPlayer, false, eventLayer, destination, Quaternion.identity, true, onArrive);
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
        //Debug.Log("on waypoint arrive ");
        currentPathCorner++;
        if (currentPathCorner < path.Length) {
            
            Vector3 nextWaypoint = path[currentPathCorner];

            //OffMeshLinkData d = agent.nextOffMeshLinkData;

            //Debug.LogError(d.linkType);

            //if (agent.nextOffMeshLinkData) {
            //    Debug.LogError("On Offmesh Link");
            //}

            //calculate direction for movement
            controller.SetDirection(agitated ? Movement.AI.CalculateMoveDirection(transform.position, nextWaypoint, facePosition, aiBehavior.minStrafeDistance, controller.direction) : Movement.Direction.Forward);
            
            //play the waypoint trakcker cue
            waypointTracker.GoTo(nextWaypoint, OnWaypointArrive);
            //Debug.Log("ai waypoint go to " + nextWaypoint);
            //Debug.Break();
        }
        else {
            OnDestinationArrive();
        }
    }

    void OnDestinationArrive () {
        //Debug.Log("Arriving destination");
        endEventPlayerPlay.EndPlay("end nav");
        endEventPlayerPlay = null;   
        pathStatus = NavMeshPathStatus.PathInvalid;     
    }


    bool waitingForPath { get { return agent.pathPending; } }


    IEnumerator WaitForPathCalculation () {
        while (waitingForPath) {
            yield return null;
        }
        this.path = agent.path.corners;
        this.pathStatus = agent.pathStatus;

        //skip the first one, navmesh agent path's first corner
        //is the origin position
        currentPathCorner = 0;
        //Debug.Log("path calculationdone");
        OnWaypointArrive();
    }

    
    /*
        parameters:
            layer (internally set), vector3 target
    */
    
    void NavigateTo(object[] parameters) {
        
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


