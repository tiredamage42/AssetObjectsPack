

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using AssetObjectsPacks;
using AssetObjectsPacks.Animations;

public class DummyAI : MonoBehaviour{
    public int stance;
    public int speed;
    public int weapon;

    public AssetObjectEventPlaylist idle_scene;

    AssetObjectEventPlayer animator;

    void Awake () {
        animator = GetComponent<AssetObjectEventPlayer>();
    }

    void Start () {
        idle_scene.InitializePerformance(new List<AssetObjectEventPlayer> { animator }, transform.position, transform.rotation, null);
    }
}


