

using System.Collections.Generic;
using UnityEngine;
using AssetObjectsPacks;

public class DummyAI : MonoBehaviour{
    public int stance;
    public int speed;
    public int weapon;

    public AssetObjectEventPlaylist idle_scene;

    AssetObjectEventPlayer player;

    void OnGrenadeThrow () {
        Debug.Log("GRENADE!");
    }

    void Awake () {
        player = GetComponent<AssetObjectEventPlayer>();
    }
    void Start () {
        idle_scene.InitializePerformance(new List<AssetObjectEventPlayer> { player }, transform.position, transform.rotation, null);
    }
}


