

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using AssetObjectsPacks.Animations;

public class DummyAI : MonoBehaviour{
    public int stance;
    public int speed;
    public int weapon;

    public AnimationScene idle_scene;

    AnimationPlayer animator;

    void Awake () {
        animator = GetComponent<AnimationPlayer>();
    }

    void Start () {
        idle_scene.InitializePerformance(new List<AnimationPlayer> { animator }, transform.position, transform.rotation, null);
    }
}


