using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AssetObjectsPacks;

using Movement;
using Movement.Platforms;
using Combat;

using DynamicRagdoll;


namespace Player {

public class PlayerController : MonoBehaviour
{
    public float jumpSpeed = 5.0f;
    public float runSpeed = 5.0f;
    public float walkSpeed = 1.0f;
    public float runAxisMinMagnitude = .25f;
    MovementController movement;
    EventPlayer eventPlayer;
    CharacterCombat combat;
    Camera cam;
    Turner turner;
    //Jumper jumper;
    Platformer platformer;

    CharacterAnimatorMover charAnimationMover;
    CharacterMovement characterMovement;
    RagdollController ragdollController;

    void Awake () {
        eventPlayer = GetComponent<EventPlayer>();        
        eventPlayer.AddParameters( new CustomParameter[] {
            new CustomParameter("Agitated", false),
        } );

        ragdollController = GetComponent<RagdollController>();

        

        movement = GetComponent<MovementController>();
        turner = GetComponent<Turner>();
        //jumper = GetComponent<Jumper>();
        platformer = GetComponent<Platformer>();
        combat = GetComponent<CharacterCombat>();

        charAnimationMover = GetComponent<CharacterAnimatorMover>();
        characterMovement = GetComponent<CharacterMovement>();

        turner.doAutoTurn = true;
        cam = Camera.main;

        combat.SetAimTargetCallback( () => cam.transform.position + cam.transform.forward * 500);
        
        turner.SetTurnTargetCallback( () => CalculateFaceDir() );       
    }

    Vector2 lastRawMove;
    Vector3 CalculateFaceDir () {

        Vector3 faceDir = Vector3.zero;

        faceDir += cam.transform.forward * lastRawMove.y;
        faceDir += cam.transform.right * lastRawMove.x;// horizontalAxis;
        bool noInput = lastRawMove == Vector2.zero;// horizontalAxis == 0 && vertAxis == 0;
        if (noInput) faceDir = cam.transform.forward;
        faceDir.y = 0;
        faceDir.Normalize();

        return transform.position + faceDir * 500;
    }




    
    void CalculateSpeedAndDirection () {
        Movement.Movement.Direction moveDir = Movement.Movement.Direction.Forward;
        
        Vector2 rawMove = new Vector2(CustomInputManager.InputManager.GetAxisRaw("Horizontal"), CustomInputManager.InputManager.GetAxisRaw("Vertical"));
        Vector2 move = new Vector2(CustomInputManager.InputManager.GetAxis("Horizontal"), CustomInputManager.InputManager.GetAxis("Vertical"));
        lastRawMove = rawMove;


        bool noInput = rawMove == Vector2.zero;

        turner.doAutoTurn = true;
        
        if (rawMove.y != 0) {
            moveDir = rawMove.y < 0 ? Movement.Movement.Direction.Backwards : Movement.Movement.Direction.Forward;
        }
        else if (rawMove.x != 0) {
            moveDir = rawMove.x < 0 ? Movement.Movement.Direction.Left : Movement.Movement.Direction.Right;
        }

        int speed = 0;

        if (CustomInputManager.InputManager.Gamepad.GamepadIsConnected(0)) {
            HandleRunModController(move);

            if (noInput == false) {
                speed = runModController ? 2 : 1;
            }
        }
        else {
            if (noInput == false) {
                speed =  CustomInputManager.InputManager.GetButton("Run") ? 2 : 1;
            }
        }

        movement.speed = speed;
        movement.direction = moveDir;

        turner.autoTurnAnimate = speed == 0;

        if (usingAnimationMovement) {
            if (speed > 0 && !movement.scriptedMove) {
                SwitchAnimationMovement(false);
            }
        }
        else {
            if (speed == 0 || movement.scriptedMove) {
                SwitchAnimationMovement(true);
            }
            else {
                move = move.normalized * (speed == 2 ? runSpeed : walkSpeed) * Time.deltaTime;
                characterMovement.SetMoveDelta(transform.TransformDirection( new Vector3(move.x, 0.0f, move.y) ));
            }
        }
    }
    bool runModController;

    void HandleRunModController (Vector2 moveVector) {
        float mag2 = moveVector.sqrMagnitude;

        if (mag2 < runAxisMinMagnitude * runAxisMinMagnitude) {
            runModController = false;
        }
        else {
            if (!runModController) {
                if (CustomInputManager.InputManager.GetButtonDown("Run")) {
                    runModController = true;
                }
            }
        }
    }

    bool usingAnimationMovement;
    void SwitchAnimationMovement (bool enabled) {
        
        usingAnimationMovement = enabled;
        charAnimationMover.setPosition = enabled;
    }
    
    void CheckDirectionalMovement () {

        if (!movement.scriptedMove) {

            bool jumpAttempt = CustomInputManager.InputManager.GetButtonDown("Jump");

            if (jumpAttempt) {

                bool hasPlatform = platformer.PlatformUpUpdate(jumpAttempt);
                if (hasPlatform) {
                    combat.isAiming = false;
                    SwitchAnimationMovement(true);
                }
                else {
                    characterMovement.JumpRaw(jumpSpeed);
                    //jumper.Jump();
                    //SwitchAnimationMovement(true);
                }
            }

            CalculateSpeedAndDirection();
        }
        // else {
        //     Debug.LogError("overridden");
        // }
    }

    void Update()
    {
        if (CustomInputManager.InputManager.GetKeyDown(KeyCode.R)) {

                    ragdollController.GoRagdoll("manual");


        }

        if (CustomInputManager.InputManager.GetButtonDown("UI_Up")) {
            Game.timeDilation += .1f;
        }
        if (CustomInputManager.InputManager.GetButtonDown("UI_Down")) {
            Game.timeDilation -= .1f;
        }
        
        turner.autoTurnAnimate = movement.speed == 0;

        combat.SetAiming(CustomInputManager.InputManager.GetButton("Aim"));
        combat.SetFiring(CustomInputManager.InputManager.GetButton("Fire"));

        CheckDirectionalMovement();
    }
    public Texture crosshairTexture;
        


      /*
			GUI STUFF
		*/
		void OnGUI () {
			DrawCrosshair();
		}

		void DrawCrosshair () {
			float crossHairSize = 40;
			float halfSize = crossHairSize / 2;
			Rect crosshairRect = new Rect(0 - halfSize, Screen.height - 0 - halfSize, crossHairSize, crossHairSize);
			GUI.DrawTexture(crosshairRect, crosshairTexture, ScaleMode.ScaleToFit, true);
		}
}

}
