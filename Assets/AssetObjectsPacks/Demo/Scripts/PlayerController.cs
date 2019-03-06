using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AssetObjectsPacks;
public class PlayerController : MonoBehaviour
{

    MovementController movement;
    Camera cam;

    void Awake () {
        EventPlayer player = GetComponent<EventPlayer>();        
        player.AddParameters( new CustomParameter[] {
            new CustomParameter("Agitated", false),
        } );

        movement = GetComponent<MovementController>();
        movement.doAutoTurn = true;
        cam = Camera.main;
    }

    
    void CheckDirectionalMovement () {
        Movement.Direction direction = Movement.Direction.Forward;

        Vector3 dir = Vector3.zero;
        int fwdBwd = 0;
        if (Input.GetKey(KeyCode.W)) {
            dir += cam.transform.forward;
            fwdBwd += 1;
        }
        if (Input.GetKey(KeyCode.S)) {
            dir -= cam.transform.forward;
            fwdBwd -= 1;
        }
        
        int leftRight = 0;
        if (Input.GetKey(KeyCode.A)) {
            dir -= cam.transform.right;
            leftRight -= 1;
        }
        if (Input.GetKey(KeyCode.D)) {
            dir += cam.transform.right;
            leftRight += 1;
        }
        if (fwdBwd != 0) {
            direction = fwdBwd == -1 ? Movement.Direction.Backwards : Movement.Direction.Forward;
        }
        else if (leftRight != 0) {
            direction = leftRight == -1 ? Movement.Direction.Left : Movement.Direction.Right;
        }

        bool noInput = dir == Vector3.zero;
        if (noInput) dir = cam.transform.forward;


        dir.y = 0;
        dir.Normalize();
        movement.SetMovementTarget(transform.position + dir * 500);
    
        int speed = noInput ? 0 : (Input.GetKey(KeyCode.LeftShift) ? 2 : 1);
        movement.speed = speed;
        movement.direction = direction;

        if (Input.GetKeyDown(KeyCode.Space)) {
            movement.TriggerJump();
        }

    }
    void Update()
    {
        CheckDirectionalMovement();
    }
}
