using System.Collections;
using UnityEngine;
using AssetObjectsPacks;
#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable] public class RootMotion : MovementControllerComponent
{


    #if UNITY_EDITOR
    public void OnDrawGizmos()
    {
        if (usePhysicsController) {
            if (cc != null) {
                
                Handles.color = Color.green;
                Matrix4x4 angleMatrix = Matrix4x4.TRS(transform.position + cc.center, transform.rotation, Handles.matrix.lossyScale);
                using (new Handles.DrawingScope(angleMatrix))
                {
                    float _radius = cc.radius;
                    var pointOffset = (cc.height - (_radius * 2)) / 2;
        
                    //draw sideways
                    Handles.DrawWireArc(Vector3.up * pointOffset, Vector3.left, Vector3.back, -180, _radius);
                    Handles.DrawLine(new Vector3(0, pointOffset, -_radius), new Vector3(0, -pointOffset, -_radius));
                    Handles.DrawLine(new Vector3(0, pointOffset, _radius), new Vector3(0, -pointOffset, _radius));
                    Handles.DrawWireArc(Vector3.down * pointOffset, Vector3.left, Vector3.back, 180, _radius);
                    //draw frontways
                    Handles.DrawWireArc(Vector3.up * pointOffset, Vector3.back, Vector3.left, 180, _radius);
                    Handles.DrawLine(new Vector3(-_radius, pointOffset, 0), new Vector3(-_radius, -pointOffset, 0));
                    Handles.DrawLine(new Vector3(_radius, pointOffset, 0), new Vector3(_radius, -pointOffset, 0));
                    Handles.DrawWireArc(Vector3.down * pointOffset, Vector3.back, Vector3.left, -180, _radius);
                    //draw center
                    Handles.DrawWireDisc(Vector3.up * pointOffset, Vector3.up, _radius);
                    Handles.DrawWireDisc(Vector3.down * pointOffset, Vector3.up, _radius);
        
                }
            }
        }

    }
 
    #endif

    /*
    public bool usingRootMotion = true;
    public bool calculateMoveSloped = true;
    public bool useGravity = true;
    public bool usePhysicsController = true;

    public bool grounded;    
    
    Animator anim;
    CharacterController cc;
    Vector3 animDeltaPosition, groundNormal = Vector3.up;
    Quaternion animDeltaRotation;
    float floorY, currentGravity;
    
    //const float groundCheckBuffer = .1f;
     */
     public bool usingRootMotion = true;
    public bool calculateMoveSloped = true;
    public bool useGravity = true;
    public bool usePhysicsController = true;

    //public bool grounded;    
    
    Animator anim;
    CharacterController cc;
    Vector3 animDeltaPosition;
    Quaternion animDeltaRotation;
    float currentGravity;
    
    bool skipMove { get { return eventPlayer.overrideMovement || !usingRootMotion; } }

    public void Initialize(MovementController movementController) {
        this.movementController = movementController;
        cc = movementController.GetComponent<CharacterController>();
        anim = movementController.GetComponent<Animator>();
        anim.applyRootMotion = false;
    }

    public void OnAnimatorMove () {
        
        animDeltaPosition = skipMove ? Vector3.zero : anim.deltaPosition;
        animDeltaRotation = skipMove ? Quaternion.identity : anim.deltaRotation;
    }

    public void Update () {
        CheckComponentEnabled();
    }
    public void FixedUpdate () {
        //CheckGrounded();
        UpdateLoop(AnimatorUpdateMode.AnimatePhysics, Time.fixedDeltaTime);
    }
    public void LateUpdate () {
        UpdateLoop(AnimatorUpdateMode.Normal, Time.deltaTime);
    }

    /*
        parameters:
            enabled, delaytime (optional), duration (optional)
    */
    /*
    void SetByMessage (object[] parameters, System.Action<bool, float, float> enableFN) { 
        bool enabledValue = (bool)parameters[0];

        float delayTime = 0;
        float duration = -1;
        if (parameters.Length > 1) {
            delayTime = (float)parameters[1];
        }
        if (parameters.Length > 2) {
            duration = (float)parameters[2];
        }
        enableFN(enabledValue, delayTime, duration); 
    }
     */
    public void EnableAllPhysics(object[] parameters) { SetByMessage(parameters, EnableAllPhysics); }
    public void EnableSlopeMove(object[] parameters) { SetByMessage(parameters, EnableSlopeMove); }
    public void EnableGravity(object[] parameters) { SetByMessage(parameters, EnableGravity); }
    public void EnablePhysics(object[] parameters) { SetByMessage(parameters, EnablePhysics); }
    public void EnableRootMotion(object[] parameters) { SetByMessage(parameters, EnableRootMotion); }
    

    
    
    public void EnableSlopeMove(bool enabled, float delay=0, float duration=-1) {
        EnableAfterDelay(EnableSlopeMove, enabled, delay, duration, e => calculateMoveSloped = e );
    }
    public void EnableGravity(bool enabled, float delay=0, float duration=-1) {
        EnableAfterDelay(EnableGravity, enabled, delay, duration, e => useGravity = e );
    }
    public void EnableRootMotion(bool enabled, float delay=0, float duration=-1) {
        EnableAfterDelay(EnableRootMotion, enabled, delay, duration, e => usingRootMotion = e );
    }
    public void EnablePhysics(bool enabled, float delay=0, float duration=-1) {
        EnableAfterDelay(EnablePhysics,
            enabled, delay, duration, 
            (e) => {
                usePhysicsController = e;
                cc.enabled = e;
            } 
        );
    }
    public void EnableAllPhysics(bool enabled, float delay=0, float duration=-1) {
        //if (enabled) {

        //}
        EnableAfterDelay(EnableAllPhysics,
            enabled, delay, duration,
            (e) => {
                usePhysicsController = e;
                cc.enabled = e;
                useGravity = e;
                calculateMoveSloped = e;
                if (e) {
                    //Debug.Log("Enabling cur grav: " + currentGravity + " ground: " + grounded);
                    //Debug.Break();
                }
            } 
        );
    }

    void CheckComponentEnabled () {
        if (eventPlayer.overrideMovement) {
            if (cc.enabled) {
                cc.enabled = false;
            }
        }
        else {
            if (usePhysicsController) {
                if (!cc.enabled) {
                    cc.enabled = true;
                    //Debug.Log("enabling controller");
                }
            }

        }
    }

    void RootMovementLoop (float deltaTime) {

        Vector3 rootMotion = CalculateRootMotion();
        
        //add gravity
        if (useGravity) {
            rootMotion.y = CalculateGravity(rootMotion.y, deltaTime);        
        }
        else {
            currentGravity = 0;
        }

        //if (!skipMove) {
            //use physics controller
            if (usePhysicsController && cc && cc.enabled) {
                cc.Move(rootMotion);
            }
            else { //just move transform
                
                //adjust to stay on ground if grounded
                if (useGravity && movementController.grounded) {
                    float curY = transform.position.y;
                    if (curY + rootMotion.y < movementController.floorY) {
                        Debug.Log("adjustint transform gravity");
                        rootMotion.y = movementController.floorY - curY;
                    } 
                }
                transform.position += rootMotion;
            }
        //}
    }

    void RootRotationLoop (float deltaTime) {
        //if (!skipMove) {

            transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles + animDeltaRotation.eulerAngles);
        //}
    }
    
    void UpdateLoop (AnimatorUpdateMode checkMode, float deltaTime) {
        
        
        if (!skipMove) {
            if (behavior.turnUpdate == checkMode) 
                RootRotationLoop(deltaTime);
            if (behavior.moveUpdate == checkMode) {
                RootMovementLoop(deltaTime);
                //CheckGrounded();
            }
        }
        else {
            //Debug.LogError("skippin move");
        }
    }
    
    Vector3 CalculateRootMotion() {
        
        Vector3 rootMotion = animDeltaPosition;
        //if (!skipMove) {

            if (calculateMoveSloped) {
                //sidways without y velocity
                Vector3 sidewaysRootMotion = new Vector3(rootMotion.z, 0, -rootMotion.x);
            
                //get movement relevant to ground normal (avoids skips up slopes)
                rootMotion = Vector3.Cross(sidewaysRootMotion, movementController.groundNormal);
                //add back original y velocity
                rootMotion.y += animDeltaPosition.y;
            }
       // }
        return rootMotion;
    } 
    
    float CalculateGravity(float yVelocity, float deltaTime){
        bool rootMotionUpwards = animDeltaPosition.y > 0;
        bool fallStarted = currentGravity != 0;
    
        if (movementController.grounded) {
            currentGravity = 0;
        }

        //if the animation is trying to go upwards 
        //and we havent started falling yet dont do anyting
        if (rootMotionUpwards && !fallStarted) {
            return yVelocity;
        }

        //if falling add to downward velocity
        if (!movementController.grounded) {
            currentGravity += Physics.gravity.y * deltaTime * deltaTime;

            //cap downward velocity
            if (currentGravity < behavior.minYVelocity) {
                currentGravity = behavior.minYVelocity;
            }    
        }
        

        //if grounded stick to floor, else use calculated gravity    
        return movementController.grounded ? behavior.minYVelocity : currentGravity;
    }
    /*
    void CheckGrounded () {
        float distanceCheck = groundCheckBuffer + (grounded ? behavior.groundDistanceCheckGrounded : behavior.groundDistanceCheckAir);
        Ray ray = new Ray(transform.position + Vector3.up * groundCheckBuffer, Vector3.down);
        Debug.DrawRay(ray.origin, ray.direction * distanceCheck, grounded ? Color.green : Color.red);

        grounded = false;
        groundNormal = Vector3.up;
        floorY = -999;
        RaycastHit hit;
        if (Physics.SphereCast(ray, behavior.groundRadiusCheck, out hit, distanceCheck, behavior.groundLayerMask)) {
            groundNormal = hit.normal;
            floorY = hit.point.y;
            if (Vector3.Angle(groundNormal, Vector3.up) <= behavior.maxGroundAngle) {
                grounded = true;
            }
        }
    }   
     */
}