using UnityEngine;
using AssetObjectsPacks;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class DummyAI : MonoBehaviour{
    [Header("Debug")]
    public Transform debugTransformLook;
    void DebugLoop () {
        if (debugTransformLook) {
            SetInterestPoint(debugTransformLook.position);
        }
        
        if (path != null) {

            if (currentPathCorner < path.Length) {
                Debug.DrawLine(transform.position, path[currentPathCorner], Color.red);
            }
            for (int i = currentPathCorner; i < path.Length - 1; i++) {
                Debug.DrawLine(path[i], path[i+1], Color.green);
            }
        }
    }
    
    public bool agitated;
    public Cue demoScene;

    EventPlayer eventPlayer;
    MovementController moveController;
    NavMeshAgent agent;

    Turner turner;
    Platformer platformer;

    
    void Awake () {
        moveController = GetComponent<MovementController>();
        waypointTracker = GetComponent<WaypointTracker>();
        turner = GetComponent<Turner>();
        platformer = GetComponent<Platformer>();
        platformer.SetCallback (OnPlatformEnd);


        eventPlayer = GetComponent<EventPlayer>();        
        eventPlayer.AddParameters ( 
            new CustomParameter[] {
                new CustomParameter( "Agitated", () => agitated ), 
            } 
        );

        agent = GetComponentInChildren<NavMeshAgent>();
        agent.updateRotation = false;
        agent.updatePosition = false;
    
    }
    
    void Start () {
        Playlist.InitializePerformance("ai demo scene", demoScene, eventPlayer, true, -1, demoScene.transform.position, demoScene.transform.rotation);
    }

    void Update () {
        DebugLoop();
        //agent.Warp(transform.position);
        //agent.nextPosition = transform.position;


    }
    void LateUpdate () {
        agent.nextPosition = transform.position;

    }
    void FixedUpdate () {
        bool facingTarget = false;
        bool nextWaypointNotOnLevel = false;
        if (path != null && currentPathCorner < path.Length) {
            if (moveController.speed > 0) {
                Vector3 nextWaypoint = path[currentPathCorner];
            
                Vector3 dir = nextWaypoint - transform.position;
                dir.y = 0;
                float angle = Vector3.Angle(dir, transform.forward);
                facingTarget = angle <= 22.5f;


                if (Mathf.Abs(nextWaypoint.y - transform.position.y) >= (Platformer.smallPlatformSize - .1f)) {
                    nextWaypointNotOnLevel = true;
                }

            }
        }
        platformer.doAutoPlatform = facingTarget && nextWaypointNotOnLevel;// moveController.speed > 0;// && turner.FacingTarget();
    }

    Vector3 interestPoint, destination;
    EventPlayer.EventPlayEnder endEventPlayerPlay;
    
    
    public void SetInterestPoint (Vector3 position) {
        interestPoint = position;
    }

    public void GoTo (Vector3 newDestination, System.Action onArrive = null) {
        Playlist.InitializePerformance("navigation ai", behavior.navigateToCue, eventPlayer, false, eventLayer, newDestination, Quaternion.identity, true, onArrive);
    }

    void OnPlatformEnd (bool isDown) {
        if (path != null && currentPathCorner < path.Length) {

            Vector3 nextWaypoint = path[currentPathCorner];

            float triggerRadius = behavior.platformEndWaypointTriggerRadius * behavior.platformEndWaypointTriggerRadius;
            Vector3 myPos = transform.position;

            if (Vector3.SqrMagnitude(nextWaypoint - myPos) < triggerRadius) {

                if (Mathf.Abs(nextWaypoint.y - myPos.y) < (Platformer.smallPlatformSize - .1f)) {
                
                    waypointTracker.ManuallyTriggerWaypointArrival();

                
                }
            }



            
        }
        


  //      if (isDown) {
//            waypointTracker.ManuallyTriggerWaypointArrival();
    //    }
        agent.Warp(transform.position);
    }
    void OnPlatformStart (bool isDown) {
        //if (isDown) {
        //    waypointTracker.ManuallyTriggerWaypointArrival();
        //}
    }


    
    Vector3[] path;
    int currentPathCorner;

    public int eventLayer;
    WaypointTracker waypointTracker;
    public AIBehavior behavior;

    void OnWaypointArrive () {
        currentPathCorner++;
        if (currentPathCorner < path.Length) {
            
            //Debug.Log("waypoint " + (currentPathCorner+1) + " / " + path.Length);
            Vector3 nextWaypoint = path[currentPathCorner];

            //calculate direction for movement
            moveController.SetDirection(agitated ? Movement.AI.CalculateMoveDirection(transform.position, nextWaypoint, interestPoint, behavior.minStrafeDistance) : Movement.Direction.Forward);
            
            //play the waypoint trakcker cue

            Debug.Log("playing waypoint performace ai");
            Playlist.InitializePerformance("waypoint ai", moveController.behavior.wayPointCue, eventPlayer, false, waypointTracker.eventLayer, nextWaypoint, Quaternion.identity, false, OnWaypointArrive );
        }
        else {
            OnDestinationArrive();
        }
    }

    void OnDestinationArrive () {
        //Debug.Log("Arriving destination");
        endEventPlayerPlay.EndPlay();
        endEventPlayerPlay = null;        
    }



    /*
        parameters:
            layer (internally set), vector3 target
    */

    void GoTo_Cue(object[] parameters) {
        
        //unpack parameters
        int layer = (int)parameters[0];
        destination = (Vector3)parameters[1];
        
        
        //agent.Warp(transform.position);
        agent.nextPosition = transform.position;

        
        //calculate path
        agent.SetDestination(destination);
        var path = agent.path;
        this.path = path.corners;
        //Debug.Log("calculating path " + this.path.Length);
    
        //take control of the player's end play callback, to call it when arriving at destination
        endEventPlayerPlay = eventPlayer.OverrideEndPlay(layer, null, "pathfinder");

        //Debug.Log("overrode event end");
        

        currentPathCorner = 0;
        OnWaypointArrive();
    }
    


}


