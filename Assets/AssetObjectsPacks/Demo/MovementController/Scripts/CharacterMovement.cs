using System.Collections;
using UnityEngine;
using AssetObjectsPacks;
using System;

namespace Movement {

    public class CharacterMovement : MovementControllerComponent
    {

        void OnDrawGizmos()
        {
            if (usePhysicsController) {
                if (cc != null) {
                    Gizmos.color = Color.green;
                    Gizmos.DrawWireCube(transform.position + cc.center, new Vector3(cc.radius * 2, cc.height, cc.radius * 2));
                }
            }
            else {
                if (transformGroundFix) {
                    Gizmos.color = Color.blue;
                    Gizmos.DrawWireSphere(transform.position + Vector3.up * .1f, .1f);
                }
                if (useGravity) {
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawWireSphere(transform.position + Vector3.up * .3f, .1f);
                }

            }
        }
        





        Vector3 moveDelta, eulerDelta;
        
        public void SetRotationDelta (Vector3 eulerDelta) {

            if (!enableMovement) {
                this.eulerDelta = Vector3.zero;
                return;
            }

            this.eulerDelta = eulerDelta;
        }
        public void SetMoveDelta (Vector3 moveDelta) {
            if (!enableMovement) {
                this.moveDelta = Vector3.zero;
                return;
            }
            
            this.moveDelta = moveDelta;
        }


        [Range(0,5)] public float gravityModifier = 1.0f;
        
        public float characterRadius = .1f;
        public float characterHeight = 2f;
        

        public bool calculateMoveSloped = true;
        public bool useGravity = true;
        public bool usePhysicsController = true;

        public bool transformGroundFix = true;


        bool inCoyoteHang;
		float lastGroundHitTime, lastGroundTime;
		
        [Tooltip("How much time to hang in the air and extend being 'grounded'")]
		public float coyoteTime = .2f;

		[Header("Free Falling")]
		[Tooltip("How far we have to drop when not gorunded in order to go ragdoll from a fall")]
		public float fallDistance = 3f;

		[Tooltip("How much time to wait until initiating ragdoll after not being grounded and falling from high enough")]
		public float fallDelayTime = .2f;

        

        public void JumpRaw (float speed) {
            if (grounded && !controller.scriptedMove) {
                currentGravity = speed * Time.timeScale;
            }
        }

        public float stepOffset = .25f;
        CharacterController cc;
        public bool grounded, freeFall;    
        Vector3 groundNormal = Vector3.up;
        public float floorY, currentGravity;
        //const float groundCheckBuffer = .1f;

        public bool enableMovement = true;


        protected override void Awake () {     
            base.Awake();
            cc = GetComponent<CharacterController>();
            eventPlayer.AddParameter( new CustomParameter ( "Grounded", () => grounded ) );

            controller.AddChangeLoopStateValueCheck( () => grounded, "grounded" );
        }
        
        public override void UpdateLoop (float deltaTime) {

            CheckCapsuleComponentEnabled();
            
            if (!eventPlayer.cueMoving){
                
                Ray groundRay = new Ray(transform.position + Vector3.up * stepOffset, Vector3.down);
            

                CheckGrounded(groundRay);
             
                //check for a big fall
                freeFall = CheckForFall(groundRay, stepOffset);

                
                
			
                //handle rotation
                transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles + eulerDelta);
                
                //handle position
                RootMovementLoop(deltaTime);
            }
        }

        bool CheckForFall (Ray groundRay, float rayDistanceBuffer) {
			if (grounded)
                return false;
            
            if (freeFall)
                return true;
            
			//check if we've spend enough time not grounded
			if (Time.time - lastGroundTime >= fallDelayTime) {

				//if we have and the drop is high enough, go ragdoll
				if (!Physics.Raycast(groundRay, fallDistance + rayDistanceBuffer, behavior.groundLayerMask, QueryTriggerInteraction.Ignore)) {
					return true;
				}
			}
			return false;
		}



        void RootMovementLoop (float deltaTime) {

            Vector3 rootMotion = CalculateRootMotion();

            //add gravity
            if (useGravity) {
                rootMotion.y = CalculateGravity(rootMotion.y, deltaTime);                        
            }


            if (!enableMovement) 
                return;
            



            
            //use physics controller
            if (usePhysicsController && cc != null && cc.enabled) {
                cc.Move(rootMotion);
            }
            else { //just move transform

                if (transformGroundFix) {

                    //adjust to stay on ground if grounded
                    
                    //if (useGravity && grounded) {
                    if (grounded) {

                        // Debug.Log("GROUNDED");
                        // Debug.Break();

                        
                        // keep us above ground (if we're going below) ...should take care of step offset...
                        float curY = transform.position.y;
                        if (curY + rootMotion.y < floorY) {
                            rootMotion.y = floorY - curY;
                        } 
                    }
                }



                transform.position += rootMotion;
            }
        }
        public bool preventUpwardsMotion;





        void CheckCapsuleComponentEnabled () {
            if (cc == null) 
                return;

            if (eventPlayer.cueMoving) {
                EnableCapsuleComponent(false);
            }
            else {
                if (usePhysicsController) {
                    EnableCapsuleComponent(true);
                    cc.radius = characterRadius;
                    cc.height = characterHeight;
                }
            }
        }

        void EnableCapsuleComponent(bool enabled) {
            if (cc.enabled != enabled) {
                cc.enabled = enabled;
            }
        }

        
        Vector3 CalculateRootMotion() {
            
            Vector3 rootMotion = moveDelta;

            if (calculateMoveSloped) {
                //sidways without y velocity
                Vector3 sidewaysRootMotion = new Vector3(rootMotion.z, 0, -rootMotion.x);
            
                //get movement relevant to ground normal (avoids skips up slopes)
                rootMotion = Vector3.Cross(sidewaysRootMotion, groundNormal);

                //add back original y velocity
                rootMotion.y += moveDelta.y;
            }
            return rootMotion;
        } 


        
        float CalculateGravity(float yVelocity, float deltaTime){
            bool rootMotionUpwards = moveDelta.y > 0;
            
            //bool fallStarted = currentGravity != 0;
            if (grounded) {
                currentGravity = 0;
            }

            //if the animation is trying to go upwards 
            //and we havent started falling yet dont do anyting
            if (rootMotionUpwards){//} && !fallStarted) {
                return yVelocity;
            }

            if (grounded && inCoyoteHang) {
                return currentGravity;
            }

            //if falling add to downward velocity
            // if (!grounded) {

                currentGravity += Physics.gravity.y * gravityModifier * deltaTime * deltaTime;
                
                //cap downward velocity
                if (currentGravity < behavior.minYVelocity) {
                    currentGravity = behavior.minYVelocity;
                }    

            // }

            //if grounded stick to floor, else use calculated gravity    
            return currentGravity;//grounded ? behavior.minYVelocity : currentGravity;
        }




        void CheckGrounded (Ray groundRay) {
            float distanceCheck = stepOffset + (grounded ? behavior.groundDistanceCheckGrounded : behavior.groundDistanceCheckAir);
            Debug.DrawRay(groundRay.origin, groundRay.direction * distanceCheck, grounded ? Color.green : Color.red);

            bool wasGrounded = grounded;
            grounded = false;
            
            inCoyoteHang = false;
			
            
            groundNormal = Vector3.up;

            //floorY = -999;
            
            if (currentGravity <= 0) {

                RaycastHit hit;
                if (Physics.SphereCast(groundRay, behavior.groundRadiusCheck, out hit, distanceCheck, behavior.groundLayerMask)) {
                    
                    // if we're falling, dont let us go "up"
                    // ragdoll was falling up stairs...
                    //bool skipFloorSet = ragdollController.state == RagdollControllerState.Falling && hit.point.y > floorY;
                    
                    if (!preventUpwardsMotion || hit.point.y <= floorY){
                        floorY = hit.point.y;
                    }
                    // floorY = hit.point.y;

                    
                    groundNormal = hit.normal;
                    if (Vector3.Angle(groundNormal, Vector3.up) <= behavior.maxGroundAngle) {
                        
                        grounded = true;

                        freeFall = false;

                        lastGroundHitTime = Time.time;

                    }
                }
                //stay grounded if we just left the ground (like wile e coyote)
				if (wasGrounded) {
					grounded = Time.time - lastGroundHitTime <= coyoteTime;
					inCoyoteHang = grounded;
				}
                if (grounded) {
                    lastGroundTime = Time.time;
                }
            }
        }   


        /*
            Messages for setting variables via cues and playlists    

            parameters:
                layer (internally set), enabled, delaytime (optional), duration (optional)
        */
        void SetByMessage (object[] parameters, System.Action<bool, float, float> enableFN) { 
            bool enabledValue = (bool)parameters[1];
            float delayTime = parameters.Length > 2 ? (float)parameters[2] : 0;
            float duration = parameters.Length > 3 ? (float)parameters[3] : -1;
            enableFN(enabledValue, delayTime, duration); 
        }
        void EnableAllPhysics(object[] parameters) { SetByMessage(parameters, EnableAllPhysics); }
        void EnableSlopeMove(object[] parameters) { SetByMessage(parameters, EnableSlopeMove); }
        void EnableGravity(object[] parameters) { SetByMessage(parameters, EnableGravity); }
        void EnablePhysicsController(object[] parameters) { SetByMessage(parameters, EnablePhysicsController); }
        void EnableTransformGroundFix(object[] parameters) { SetByMessage(parameters, EnableTransformGroundFix); }

        public void EnableTransformGroundFix(bool enabled, float delay=0, float duration=-1) {
            EnableAfterDelay(EnableTransformGroundFix, enabled, delay, duration, e => transformGroundFix = e );
        }
        public void EnableSlopeMove(bool enabled, float delay=0, float duration=-1) {
            EnableAfterDelay(EnableSlopeMove, enabled, delay, duration, e => calculateMoveSloped = e );
        }
        public void EnableGravity(bool enabled, float delay=0, float duration=-1) {
            EnableAfterDelay(EnableGravity, enabled, delay, duration, (e) => { useGravity = e; currentGravity = 0; } );
        }
        public void EnablePhysicsController(bool enabled, float delay=0, float duration=-1) {
            EnableAfterDelay(EnablePhysicsController,
                enabled, delay, duration, 
                (e) => {
                    usePhysicsController = e;
                    if (cc != null) {
                        cc.enabled = e;
                    }
                } 
            );
        }
        public void EnableAllPhysics(bool enabled, float delay=0, float duration=-1) {
            EnableAfterDelay(EnableAllPhysics,
                enabled, delay, duration,
                (e) => {
                    EnablePhysicsController(e);
                    EnableGravity(e);
                    EnableSlopeMove(e);
                } 
            );
        }


        /*
            self: the method calling this
        */
        IEnumerator EnableAfterDelay (Action<bool, float, float> self, bool enabled, float delay, float duration) {
            yield return new WaitForSeconds(delay);
            self(enabled, 0, duration);
        }
        void EnableAfterDelay(Action<bool, float, float> self, bool enabled, float delay, float duration, Action<bool> enableFN) {
            if (delay > 0) {
                StartCoroutine(EnableAfterDelay(self, enabled, delay, duration));
                return;
            }
            enableFN(enabled);
            if (duration >= 0) {
                StartCoroutine(EnableAfterDelay(self, !enabled, duration, -1));
            }
        }
    }
}
