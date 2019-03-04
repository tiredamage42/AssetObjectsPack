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

    
    void SetMovementTarget (Vector3 dir) {
        dir.y = 0;
        dir.Normalize();
        movement.SetMovementTarget(transform.position + dir * 500);
    }

    void CheckDirectionalMovement () {
        int direction = 0;
        int leftRight = 0;

        Vector3 dir = Vector3.zero;
        if (Input.GetKey(KeyCode.A)) {
            dir -= cam.transform.right;
            leftRight -= 1;
        }
        if (Input.GetKey(KeyCode.D)) {
            dir += cam.transform.right;
            leftRight += 1;
        }
        int fwdBwd = 0;
        if (Input.GetKey(KeyCode.W)) {
            dir += cam.transform.forward;
            fwdBwd += 1;
        }
        if (Input.GetKey(KeyCode.S)) {
            dir -= cam.transform.forward;
            fwdBwd -= 1;
        }
        if (leftRight != 0) {
            direction = leftRight == -1 ? 1 : 2;
        }
        if (fwdBwd != 0) {
            direction = fwdBwd == -1 ? 3 : 0;
        }

        int speed = (dir != Vector3.zero) ? 1 : 0;

        SetMovementTarget( dir == Vector3.zero ? cam.transform.forward : dir );

        movement.speed = Input.GetKey(KeyCode.LeftShift) ? speed * 2 : speed;

        movement.direction = direction;

        if (Input.GetKeyDown(KeyCode.Space)) {
            movement.TriggerJump();
        }

    }

    // Update is called once per frame
    void Update()
    {
        CheckDirectionalMovement();
        
    }
}
