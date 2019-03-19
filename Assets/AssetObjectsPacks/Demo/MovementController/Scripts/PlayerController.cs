using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AssetObjectsPacks;
public class PlayerController : MonoBehaviour
{

    [Range(0,1)] public float timeDilation = 1.0f;

    MovementController movement;
    EventPlayer eventPlayer;
    Camera cam;

    void Awake () {
        eventPlayer = GetComponent<EventPlayer>();        
        eventPlayer.AddParameters( new CustomParameter[] {
            new CustomParameter("Agitated", false),
        } );

        movement = GetComponent<MovementController>();
        turner = GetComponent<Turner>();
        jumper = GetComponent<Jumper>();
        platformer = GetComponent<Platformer>();

        turner.doAutoTurn = true;
        turner.checkDirectionChange = true;
        cam = Camera.main;

        InitializeTimeSlow();
    }


    
    void CalculateSpeedAndDirection () {
        Movement.Direction moveDir = Movement.Direction.Forward;
        Vector3 faceDir = Vector3.zero;

        float vertAxis = 0;
        vertAxis += Input.GetKey(KeyCode.W) ? 1 : 0;
        vertAxis += Input.GetKey(KeyCode.S) ? -1 : 0;
        faceDir += cam.transform.forward * vertAxis;

        float horizontalAxis = 0;
        horizontalAxis += Input.GetKey(KeyCode.A) ? -1 : 0;
        horizontalAxis += Input.GetKey(KeyCode.D) ? 1 : 0;
        faceDir += cam.transform.right * horizontalAxis;

        bool noInput = horizontalAxis == 0 && vertAxis == 0;

        if (noInput) faceDir = cam.transform.forward;
        faceDir.y = 0;
        faceDir.Normalize();

        turner.doAutoTurn = true;
        turner.checkDirectionChange = true;
        
        turner.SetTurnTarget(transform.position + faceDir * 500);       

        if (vertAxis != 0) {
            moveDir = vertAxis < 0 ? Movement.Direction.Backwards : Movement.Direction.Forward;
        }
        else if (horizontalAxis != 0) {
            moveDir = horizontalAxis < 0 ? Movement.Direction.Left : Movement.Direction.Right;
        }

        int speed = noInput ? 0 : (Input.GetKey(KeyCode.LeftShift) ? 2 : 1);

        movement.speed = speed;
        movement.direction = moveDir;

    }

    
    void CheckDirectionalMovement () {

        if (!movement.overrideMovement) {
            CalculateSpeedAndDirection();

            bool jumpAttempt = Input.GetKeyDown(KeyCode.Space);

            bool hasPlatform = platformer.PlatformUpUpdate(jumpAttempt);
            
            if (hasPlatform) {
                //Debug.LogError ("Has PLATFRm");
            }

            if (jumpAttempt && !hasPlatform) {            
                jumper.Jump();
            }
        }
    }

    Turner turner;
    Jumper jumper;
    Platformer platformer;




    float initialTimeScale, initialFixedDeltaTime, initialMaxDelta;
    void InitializeTimeSlow () {
        initialTimeScale = Time.timeScale;
        initialFixedDeltaTime = Time.fixedDeltaTime;
        initialMaxDelta = Time.maximumDeltaTime;
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Equals)) {
            timeDilation += .1f;
            if (timeDilation > 1) {
                timeDilation = 1;
            }
        }
        if (Input.GetKeyDown(KeyCode.Minus)) {
            timeDilation -= .1f;
            if (timeDilation < .1f) {
                timeDilation = .1f;
            }
        }
        Time.timeScale = initialTimeScale * timeDilation;
        Time.fixedDeltaTime = initialFixedDeltaTime * timeDilation;
        Time.maximumDeltaTime = initialMaxDelta * timeDilation;

        CheckDirectionalMovement();
    }
}
