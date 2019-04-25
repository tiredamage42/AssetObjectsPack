



// using System.Collections;
// using System.Collections.Generic;
using UnityEngine;


namespace Player {

    public class FreeCam : MonoBehaviour
    {
        public float x_rot_speed = 100, y_rot_speed = 100;

        public float fast_mult = 2.0f;

        public float move_speed = 50;

        public Vector2 x_rot_clamp = new Vector2(-90,90);
        
        float x_rot, y_rot;

        void Update()
        {
            float lookY = CustomInputManager.InputManager.GetAxis("LookVertical");

            // float mod = 0;
            // if (Input.GetKey(KeyCode.UpArrow)) {
            //     mod = 1;
            // }
            // else if (Input.GetKey(KeyCode.DownArrow)) {
            //     mod = -1;
            // }
            x_rot -= Time.deltaTime * x_rot_speed * lookY;//mod;
            x_rot = Mathf.Clamp(x_rot, x_rot_clamp.x, x_rot_clamp.y);
            

            float lookX = CustomInputManager.InputManager.GetAxis("LookHorizontal");

            // mod = 0;
            // if (Input.GetKey(KeyCode.RightArrow)) {
            //     mod = 1;
            // }
            // else if (Input.GetKey(KeyCode.LeftArrow)) {
            //     mod = -1;
            // }
            y_rot += Time.deltaTime * y_rot_speed * lookX;//mod;

            transform.rotation = Quaternion.Euler(x_rot, y_rot, 0);


            float speed = move_speed;

            if (CustomInputManager.InputManager.GetButton("Inventory")) {
            // if (Input.GetKey(KeyCode.LeftShift)) {
                speed *= fast_mult;
            }


            transform.Translate(new Vector3(
                speed * Time.deltaTime * CustomInputManager.InputManager.GetAxis("Horizontal"),
                speed * Time.deltaTime * ((CustomInputManager.InputManager.GetButton("Aim") ? -1 : 0) + (CustomInputManager.InputManager.GetButton("Fire") ? 1 : 0)),
                speed * Time.deltaTime * CustomInputManager.InputManager.GetAxis("Vertical")
            ));


            // if(Input.GetKey(KeyCode.A))
            // {
            //     transform.Translate(new Vector3(-speed * Time.deltaTime,0,0));
            // }
            // if(Input.GetKey(KeyCode.D))
            // {
            //     transform.Translate(new Vector3(speed * Time.deltaTime,0,0));
            // }
            // if(Input.GetKey(KeyCode.S))
            // {
            //     transform.Translate(new Vector3(0,0, -speed * Time.deltaTime));
            // }
            // if(Input.GetKey(KeyCode.W))
            // {
            //     transform.Translate(new Vector3(0,0, speed * Time.deltaTime));
            // }
            // if(Input.GetKey(KeyCode.E))
            // {
            //     transform.Translate(new Vector3(0,speed * Time.deltaTime,0));
            // }
            // if(Input.GetKey(KeyCode.Q))
            // {
            //     transform.Translate(new Vector3(0,-speed * Time.deltaTime,0));
            // }
            
        }
    }
}


