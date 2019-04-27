using System.Collections;
using UnityEngine;
using AssetObjectsPacks;
using System;

namespace Movement {

    public class CharacterMovement : MovementControllerComponent
    {
        // Animator anim;
        Vector3 moveDelta, eulerDelta;
        
        public void SetRotationDelta (Vector3 eulerDelta) {
            this.eulerDelta = eulerDelta;
        }
        public void SetMoveDelta (Vector3 moveDelta) {
            this.moveDelta = moveDelta;
        }


        [Range(0,5)] public float gravityModifier = 1.0f;
        
        public float characterRadius = .1f;
        public float characterHeight = 2f;
        

        public bool calculateMoveSloped = true;
        public bool useGravity = true;
        public bool usePhysicsController = true;

        public bool transformGroundFix = true;
        

        // float jumpSpeed;
        public void JumpRaw (float speed) {
            if (grounded && !controller.overrideMovement) {

                currentGravity = speed * Time.timeScale;
                // jumpSpeed = speed * Time.timeScale;// * Time.deltaTime;
            }

            // currentGravity = speed;
        }
        
        CharacterController cc;
        public bool grounded;    
        Vector3 groundNormal = Vector3.up;
        float floorY, currentGravity;
        const float groundCheckBuffer = .1f;


        // ValueTracker<bool> groundChangeTracker = new ValueTracker<bool>(true);
        


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
        protected override void Awake () {     
            base.Awake();
            cc = GetComponent<CharacterController>();
            eventPlayer.AddParameter( new CustomParameter ( "Grounded", () => grounded ) );

            controller.AddChangeLoopStateValueCheck( () => grounded );

        }
        protected override void FixedUpdate() {
            base.FixedUpdate();
            // CheckGrounded();
        }
        public override void UpdateLoop (float deltaTime) {

            CheckCapsuleComponentEnabled();
            
            if (!eventPlayer.cueMoving){
                
                
                CheckGrounded();

                // if (groundChangeTracker.CheckValueChange(grounded)) {
                //     // Debug.Log("changin loop state because aim is " + isAiming);
                //     controller.UpdateLoopState();
                // }
            
                
                //handle rotation
                transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles + eulerDelta);
                
                //handle position
                RootMovementLoop(deltaTime);
            }
        }


        void RootMovementLoop (float deltaTime) {

            Vector3 rootMotion = CalculateRootMotion();


            // rootMotion.y += jumpSpeed;
            
            //add gravity
            if (useGravity) {
                rootMotion.y = CalculateGravity(rootMotion.y, deltaTime);        
                
                // rootMotion.y += jumpSpeed;
            }
            
            //use physics controller
            if (usePhysicsController && cc != null && cc.enabled) {
                cc.Move(rootMotion);
            }
            else { //just move transform

                if (transformGroundFix) {

                    //adjust to stay on ground if grounded
                    //if (useGravity && grounded) {
                    if (grounded) {
                        


                        float curY = transform.position.y;
                        if (curY + rootMotion.y < floorY) {
                            rootMotion.y = floorY - curY;
                        } 
                    }
                }
                
                transform.position += rootMotion;
            }
        }

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

            //if falling add to downward velocity
            // if (!grounded) {

                // if (jumpSpeed != 0) {
                //     currentGravity = jumpSpeed;// * deltaTime;
                //     jumpSpeed = 0;
                // }

                currentGravity += Physics.gravity.y * gravityModifier * deltaTime * deltaTime;
                

                //cap downward velocity
                if (currentGravity < behavior.minYVelocity) {
                    currentGravity = behavior.minYVelocity;
                }    

            // }

            
            //if grounded stick to floor, else use calculated gravity    
            return currentGravity;//grounded ? behavior.minYVelocity : currentGravity;
            // return grounded ? behavior.minYVelocity : currentGravity;
        }






        void CheckGrounded () {
            float distanceCheck = groundCheckBuffer + (grounded ? behavior.groundDistanceCheckGrounded : behavior.groundDistanceCheckAir);
            Ray ray = new Ray(transform.position + Vector3.up * groundCheckBuffer, Vector3.down);
            Debug.DrawRay(ray.origin, ray.direction * distanceCheck, grounded ? Color.green : Color.red);

            grounded = false;
            groundNormal = Vector3.up;
            floorY = -999;
            
            if (currentGravity <= 0) {

                RaycastHit hit;
                if (Physics.SphereCast(ray, behavior.groundRadiusCheck, out hit, distanceCheck, behavior.groundLayerMask)) {
                    groundNormal = hit.normal;
                    floorY = hit.point.y;
                    if (Vector3.Angle(groundNormal, Vector3.up) <= behavior.maxGroundAngle) {
                        grounded = true;
                    }
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
