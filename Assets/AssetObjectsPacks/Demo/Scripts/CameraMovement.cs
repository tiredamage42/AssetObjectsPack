﻿



using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{

    public float x_rot_speed = 100, y_rot_speed = 100;

    public float fast_mult = 2.0f;

    public float move_speed = 50;

    public Vector2 x_rot_clamp = new Vector2(-90,90);
    // Start is called before the first frame update
    void Start()
    {
        
    }

    float x_rot, y_rot;

    // Update is called once per frame
    void Update()
    {
        float mod = 0;
        if (Input.GetKey(KeyCode.UpArrow)) {
            mod = 1;
        }
        else if (Input.GetKey(KeyCode.DownArrow)) {
            mod = -1;
        }
        x_rot -= Time.deltaTime * x_rot_speed * mod;
        x_rot = Mathf.Clamp(x_rot, x_rot_clamp.x, x_rot_clamp.y);
        
        mod = 0;
        if (Input.GetKey(KeyCode.RightArrow)) {
            mod = 1;
        }
        else if (Input.GetKey(KeyCode.LeftArrow)) {
            mod = -1;
        }
        y_rot += Time.deltaTime * y_rot_speed * mod;

        transform.rotation = Quaternion.Euler(x_rot, y_rot, 0);


        float speed = move_speed;
        if (Input.GetKey(KeyCode.LeftShift)) {
            speed *= fast_mult;
        }

        if(Input.GetKey(KeyCode.A))
        {
            transform.Translate(new Vector3(-speed * Time.deltaTime,0,0));
        }
        if(Input.GetKey(KeyCode.D))
        {
            transform.Translate(new Vector3(speed * Time.deltaTime,0,0));
        }
        if(Input.GetKey(KeyCode.S))
        {
            transform.Translate(new Vector3(0,0, -speed * Time.deltaTime));
        }
        if(Input.GetKey(KeyCode.W))
        {
            transform.Translate(new Vector3(0,0, speed * Time.deltaTime));
        }
        if(Input.GetKey(KeyCode.E))
        {
            transform.Translate(new Vector3(0,speed * Time.deltaTime,0));
        }
        if(Input.GetKey(KeyCode.Q))
        {
            transform.Translate(new Vector3(0,-speed * Time.deltaTime,0));
        }
        
    }
}
