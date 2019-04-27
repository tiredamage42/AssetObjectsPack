using UnityEngine;
using CustomInputManager;

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
            x_rot -= Time.deltaTime * x_rot_speed * InputManager.GetAxis("LookVertical");
            x_rot = Mathf.Clamp(x_rot, x_rot_clamp.x, x_rot_clamp.y);
            
            y_rot += Time.deltaTime * y_rot_speed * InputManager.GetAxis("LookHorizontal");

            transform.rotation = Quaternion.Euler(x_rot, y_rot, 0);

            float speed = move_speed;

            if (InputManager.GetButton("Inventory")) {
                speed *= fast_mult;
            }

            transform.Translate(new Vector3(
                speed * Time.deltaTime * InputManager.GetAxis("Horizontal"),
                speed * Time.deltaTime * ((InputManager.GetButton("Aim") ? -1 : 0) + (InputManager.GetButton("Fire") ? 1 : 0)),
                speed * Time.deltaTime * InputManager.GetAxis("Vertical")
            ));
            
        }
    }
}
