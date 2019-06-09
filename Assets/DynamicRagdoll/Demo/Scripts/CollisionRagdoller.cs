﻿using UnityEngine;
using System.Collections.Generic;
namespace DynamicRagdoll.Demo {

    /*
        Add this script to the main character to enable character collisions.
        
            1. checks for incoming rigidbody collisions, our when we run into something,
                with enough force, if they're above the threshold, we go ragdoll

            2. when ragdolling, we check for collisions on the bones to add bone decay
                (through the attached RagdollController) based on teh magnitude of the collisions

        objects that shoulve been hitting the bones were bouncing off teh character controller
        (and using the character controller for ragdoll deciding wasnt working, since teh capsule shape
        had empty space that was triggering ragdolls)

        for that reason we have an external collision checkers that let us know about collisions
        that are incoming, if they're eitehr the character or immenent colliding rigidbody
        are traveling fast enough, the character controller ignores collisions with it for a small
        period of time.

        that way it gives the bones a chance to collide and decide whether or not to go rigidbody

        the trigger detector mentioned above is wider and taller than the character capsule, 
        in order to avoid them bouncing off if we set the ignore too late

        then we worry about adding decay to the bones that are hit, while we're falling and 
        going ragdoll, 

        NOTE:
            using the Character Controller itself to detect the outgoing collisions was making 
            the character controller stop movement by the time we got the collision callback.
    */

    [RequireComponent(typeof(RagdollController))]
    [RequireComponent(typeof(CharacterController))]
    public class CollisionRagdoller : MonoBehaviour
    {
        [Header("Ragdoll Detection")]
        [Tooltip("When we run into something.\nHow fast do we have to be going to go ragdoll")]
        public float outgoingMagnitudeThreshold = 5;
        
        [Tooltip("When we get hit with an object.\nHow fast does it have to be going to go ragdoll")]
        public float incomingMagnitudeThreshold = 5;
        
        [Tooltip("How much space around the character controller to pre check for rigidbodies.\n\nIf the character controller is blocking rigidbody objects that should be ragdolling us, inrease this value.")]
        public float incomingCheckOffset = .5f;
        
        [Tooltip("How long teh character controller ignores rigidbody objects (travelling above 'incomingMagnitudeThreshold' velocity), to give them a chance to hit the bones")]
        public float characterControllerIgnoreTime = 2f;

        [Tooltip("Objects touching us from above, that are over this mass, will trigger ragdoll")]
        public float crushMass = 20;
        [Tooltip("How far down from the top of our head should we consider a crush contact")]
        public float crushMassTopOffset = .25f;
                
        /*
            this wide range means it doesnt necessarily decay any bones completely
            but adds some noise to the decay, that way the more we collide the more we slow down
            ...not in one fell swoop 
        */
        [Header("Bone Decay")]
        [Tooltip("Collisions magnitude range for linearly interpolating the bone decay set by collisions.\nIf magnitude == x, then bone decay = 0\nIf magnitude == y, then bone decay = 1")] 
        public Vector2 decayMagnitudeRange = new Vector2(10, 50);

        [Tooltip("Multiplier for decay set by collisions on neighbors of collided bones")]
        [Range(0,1)] public float neighborDecayMultiplier = .75f;


        /*
			Primary collison detector
            (if it registers a collision large enough, we make the controller ignore its collider)
		*/
		TriggerDetector inCollisionDetector;
        
        RagdollController ragdollController;
        CharacterController characterController;
        
        // ignore pairs for character controller ignoring rigidbodies that might hit our ragdoll bones
        List<ColliderIgnorePair> charControllerIgnorePairs = new List<ColliderIgnorePair>();
        
        //contacts generated by ragdoll collision enter
        ContactPoint[] contacts = new ContactPoint[5];
        int contactCount;

        // planar velocity (so we dont ragdoll on steps)
        Vector3 controllerVelocity {
            get {
                Vector3 velocity = characterController.velocity;
                velocity.y = 0;
                return velocity;
            }
        }

        void Awake () {
            characterController = GetComponent<CharacterController>();
            ragdollController = GetComponent<RagdollController>();
		}
        
		void Start () {
            //subscribe to receive a callback on ragdoll bone collision
			ragdollController.ragdoll.onCollisionEnter += OnRagdollCollisionEnter;
			
            //make ragdoll bones ignore character controller
            ragdollController.ragdoll.IgnoreCollisions(characterController, true);
            //initialize trigger detector
            inCollisionDetector = BuildCollisionDetector();
			
		}

        TriggerDetector BuildCollisionDetector () {
        	
            TriggerDetector detector = new GameObject(name + "_incomingCollisionDetector").AddComponent<TriggerDetector>();
			detector.onTriggerEnter += OnIncomingCollision;

            // set our transform as the parent so the detectors move with us
            detector.transform.SetParent(transform);
			detector.transform.localPosition = Vector3.zero;
			detector.transform.rotation = Quaternion.identity;

            //make ragdoll bones ignore collisions with the trigger detection capsule
			ragdollController.ragdoll.IgnoreCollisions(detector.capsule, true);

            //make character controller ignore trigger detectors
            Physics.IgnoreCollision(characterController, detector.capsule, true);
            
			return detector;
		}
                   
        void FixedUpdate () {
            
            CheckForIgnoreExpires();
            UpdateIncomingTriggerCapsule(characterController.enabled);
            
        }

        
        /*
            adjust incoming trigger chekc height and radius
        */
        void UpdateIncomingTriggerCapsule (bool enabled) {
            // check for ignore trigger only when the character controller is enabled
            inCollisionDetector.enabled = enabled;
                
            if (enabled) {
                float height = characterController.height + incomingCheckOffset;
                float radius = characterController.radius + incomingCheckOffset;

                CapsuleCollider capsule = inCollisionDetector.capsule;
                capsule.height = height;
                capsule.radius = radius;
                capsule.center = new Vector3(0, height * .5f, 0);
            }
        }

        bool ControllerIsIgnoringCollider (Collider other) {

            for (int i = 0; i < charControllerIgnorePairs.Count; i++) {
                var pair = charControllerIgnorePairs[i];
                if (pair.collider2 == other) {
                    pair.ignoreTime = Time.time;
                    return true;
                }
            }
            return false;
        }
                
        /*
            trigger checks if something invaded our space

            then we check if it's going fast enough to ragdoll us,
            make our character controller ignore it for a little bit, 
            so it has a chance to hit our bones
        */
        void OnIncomingCollision (Collider other) {
            
            // check for a rigidbody 
            // (dont want character controller to ignore static geometry)
            
            Rigidbody rigidbody = other.attachedRigidbody;
            if (rigidbody == null || rigidbody.isKinematic)
                return;
            
            //check if it's above either of our thresholds
            float incomingThreshold = incomingMagnitudeThreshold * incomingMagnitudeThreshold;
            float outgoingThreshold = outgoingMagnitudeThreshold * outgoingMagnitudeThreshold;

            if (rigidbody.velocity.sqrMagnitude < incomingThreshold && controllerVelocity.sqrMagnitude < outgoingThreshold)
                return;
            
            //already ignoring
            if (ControllerIsIgnoringCollider(other))
                return;

            charControllerIgnorePairs.Add(new ColliderIgnorePair(characterController, other));
        }       

        void CheckForIgnoreExpires () {

            // unignore the character controller with colliders that couldve ragdolled us
            for (int i = charControllerIgnorePairs.Count - 1; i >= 0; i--) {
                ColliderIgnorePair p = charControllerIgnorePairs[i];
                
                // if enough time has passed
                if (p.timeSinceIgnore >= characterControllerIgnoreTime) {
                    p.EndIgnore();
                    charControllerIgnorePairs.Remove(p);
                }
            }
        }

        bool CollisionIsAboveStepOffset (Collision collision, float stepOffset, float buffer) {
            // if it's below our step offset (plus a buffer)
            // ignore it, we can just step on top of it

            float offsetThreshold = stepOffset + buffer;
            contactCount = collision.GetContacts(contacts);

            for (int i = 0; i < contactCount; i++) {

                float yOffset = contacts[i].point.y - transform.position.y;
                if (yOffset > offsetThreshold)
                    return true;
            }
            return false;
        }

        bool CollisionIsAboveCrushOffset (Collision collision, float charHeight) {
            for (int i = 0; i < contactCount; i++) {
                float yOffset = contacts[i].point.y - transform.position.y;

                if (charHeight - yOffset < crushMassTopOffset) {
                    // if (Vector3.Dot(Vector3.down, contacts[i].normal) > .75f)
                        return true;
                }
            }
            return false;
        }
        
        bool CollisionHasCrushMass (Collision collision) {
            return collision.collider.attachedRigidbody != null && collision.collider.attachedRigidbody.mass >= crushMass;
        }
        
        /*
			callback called when ragdoll bone gets a collision
            then apply bone decay to those bones
		*/    
		void OnRagdollCollisionEnter(RagdollBone bone, Collision collision)
		{

            float stepOffset = characterController.stepOffset;
            float charHeight = characterController.height;


            //maybe add warp to master state for ragdoll check
            
            bool checkForRagdoll = ragdollController.state == RagdollControllerState.Animated || ragdollController.state == RagdollControllerState.BlendToAnimated;
            
            bool isFalling = ragdollController.state == RagdollControllerState.Falling;

			if (!isFalling && !checkForRagdoll)
				return;

            //check for and ignore self ragdoll collsion (only happens when falling)
            if (ragdollController.ragdoll.ColliderIsPartOfRagdoll(collision.collider))
                return;
            
            if (checkForRagdoll) {

                //if we're getting up, knock us out regardless of where the collision takes place
                if (!ragdollController.isGettingUp) {

                    // if it's below our step offset (plus a buffer)
                    // ignore it... we can just step on top of it
                    if (!CollisionIsAboveStepOffset(collision, stepOffset, .1f))
                        return;
                }
            }
            

            bool isCrushed = CollisionHasCrushMass(collision) && CollisionIsAboveCrushOffset(collision, charHeight);
			float collisionMagnitude2 = collision.relativeVelocity.sqrMagnitude;
            
            if (checkForRagdoll) {

                string message = "crush";
                // check if we're being crushed
                bool goRagdoll = isCrushed;

                // else check if the collision is above our incoming threhsold, (something being thrown at us)
                if (!goRagdoll) {
                    message = "incoming";
                    goRagdoll = collisionMagnitude2 >= incomingMagnitudeThreshold * incomingMagnitudeThreshold;
                }
                
                // else check if we're travelling fast enough to go ragdoll (we ran into a wall or something...)
                if (!goRagdoll){
                    message = "outgoing";
                    collisionMagnitude2 = controllerVelocity.sqrMagnitude;
                    goRagdoll = collisionMagnitude2 >= outgoingMagnitudeThreshold * outgoingMagnitudeThreshold;
                }
    
                if (!goRagdoll)
                    return;

                Debug.Log( message + "/" + bone.name + " went ragdoll cuae of " + collision.collider.name + "/" + Mathf.Sqrt(collisionMagnitude2));
                ragdollController.GoRagdoll(message);
            }


            HandleBoneDecayOnCollision(collisionMagnitude2, bone, isCrushed, collision);
		}

        void HandleBoneDecayOnCollision (float collisionMagnitude2, RagdollBone bone, bool isCrushed, Collision collision) {

            if (isCrushed) {
                //Debug.LogWarning(bone + " / " + collision.transform.name + " CrUSHED");
                ragdollController.SetBoneDecay(bone.bone, 1, neighborDecayMultiplier);
            }
            // if the magnitude is above the minimum threshold for adding decay
            else if (collisionMagnitude2 >= decayMagnitudeRange.x * decayMagnitudeRange.x) {
                float magnitude = Mathf.Sqrt(collisionMagnitude2);

                //linearly interpolate decay between 0 and 1 base on collision magnitude
                float linearDecay = (magnitude - decayMagnitudeRange.x) / (decayMagnitudeRange.y -  decayMagnitudeRange.x);
                //Debug.Log(bone + " / " + collision.transform.name + " mag: " + magnitude + " decay " + linearDecay);
                ragdollController.SetBoneDecay(bone.bone, linearDecay, neighborDecayMultiplier);
            }
        }
    }
}