using UnityEngine;
using System;
using Movement;

namespace Game.AI {

    [RequireComponent(typeof(Turner))]
    public class WaypointTracker :  MovementControllerComponent
    {
        public bool hasDestination;
        Vector3 destination;
        Turner turner;
        Action<bool> waypointArriveCallback;


        AISettings _aiSettings;
        AISettings aiSettings {
            get {
                if (_aiSettings == null) _aiSettings = GameSettings.GetGameSettings<AISettings>();
                return _aiSettings;
            }
        }
        float waypointTurnHelpMinDistance { get { return aiSettings.waypointTurnHelpMinDistance; } }
        float waypointTurnAnimMinDistance { get { return aiSettings.waypointTurnAnimMinDistance; } }
        float[] arriveThresholds { get { return aiSettings.arriveThresholds; } }
        float[] arriveHelpThresholds { get { return aiSettings.arriveHelpThresholds; } }
        float[] moveHelpSpeeds { get { return aiSettings.moveHelpSpeeds; } }
        float maxMoveHelpSpeed { get { return aiSettings.maxMoveHelpSpeed; } }
        
        protected override 
        void Awake() {
            base.Awake();
            turner = GetComponent<Turner>();
        }

        public override void UpdateLoop (float deltaTime) {    
            if (hasDestination) {
                UpdateWaypointTracking(deltaTime);
            }
        }

        public void ManuallyTriggerWaypointArrival (bool immediate=false) {
            hasDestination = false;
            turner.doAutoTurn = false;

            if (waypointArriveCallback != null) {
                waypointArriveCallback(immediate);
            }
        }

        void HandleAutoTurner (float sqrDist) {
            //if we try and turn too close to the waypoint it winds up circling it
            float turnHelpMinDistance = waypointTurnHelpMinDistance * waypointTurnHelpMinDistance;
            turner.doAutoTurn = sqrDist >= turnHelpMinDistance;  
            
            //trying to animate turns too close to waypoint triggers too many within
            //a small amount of time
            float turnAnimMinDistance = waypointTurnAnimMinDistance * waypointTurnAnimMinDistance;
            turner.GetComponent<Turner_Animated>().autoTurnAnimate = sqrDist >= turnAnimMinDistance;
        }
        bool CheckForArrival (float sqrDist) {
            float arrivalThreshold = arriveThresholds[controller.speed] * arriveThresholds[controller.speed];
            if (sqrDist <= arrivalThreshold) {
                ManuallyTriggerWaypointArrival(false);
                return true;
            }
            return false;
        }
        void HandleArrivalHelp (float sqrDist, Vector3 myPos, float deltaTime) {
            float helpArriveThreshold = arriveHelpThresholds[controller.speed] * arriveHelpThresholds[controller.speed];
        
            //trigger help slerp if we're below the ehlpd thershold and above arrival
            if (sqrDist <= helpArriveThreshold) {
                    
                float baseSpeed = moveHelpSpeeds[controller.speed];
                float maxSpeed = maxMoveHelpSpeed;
                
                //move faster towards destination closer you are
                float speed = Mathf.Lerp(baseSpeed, maxSpeed, 1 - (sqrDist / helpArriveThreshold));

                transform.position = Vector3.Lerp(myPos, destination, deltaTime * speed);
            }
        }

        void UpdateWaypointTracking(float deltaTime)
        {

            Vector3 myPos = transform.position;
            Debug.DrawLine(myPos, destination, Color.red);
            
            float sqrDist = Vector3.SqrMagnitude(myPos - destination);
            HandleAutoTurner(sqrDist);
            if (CheckForArrival(sqrDist)) {
                return;
            }
            if (!controller.scriptedMove) {
                HandleArrivalHelp(sqrDist, myPos, deltaTime);
            }
        }
        
        public void GoTo (Vector3 newDestination, Action<bool> onWaypointArrive = null) {
            controller.StartMovementManual();
            GoToWaypointManual(newDestination, onWaypointArrive);
        }

        bool DecideIfDestinationFarEnough () {
            float sqrDist = Vector3.SqrMagnitude(transform.position - destination);
            
            //trigger if we're above the arrival threshold
            float arrivalThreshold2 = arriveThresholds[controller.speed] * arriveThresholds[controller.speed];
            return sqrDist > arrivalThreshold2;
        }
        public void GoToWaypointManual(Vector3 waypoint, Action<bool> onWaypointArrive = null) {

            destination = waypoint;

            hasDestination = DecideIfDestinationFarEnough();
                    
            this.waypointArriveCallback = onWaypointArrive;
            
            if (hasDestination) {
                turner.SetTurnTarget(destination);
            }
            else {
                // turner.doAutoTurn = false;
                ManuallyTriggerWaypointArrival(true);
            }
        }    
    }
}
