using UnityEngine;
using AssetObjectsPacks;

public class DummyAI : MonoBehaviour{
    public int stance;
    public bool agitated;

    void UpdateParameters () {
        player["Stance"].SetValue(stance);
        player["Agitated"].SetValue(agitated);
    }

    void Update () {
        UpdateParameters();
    }
    
    //public int speed;
    //public int weapon;

    public AssetObjectEventPlaylist idle_scene;

    AssetObjectEventPlayer player;
    void Awake () {
        player = GetComponent<AssetObjectEventPlayer>();
        player.playerParams = new CustomParameter[] {
             new CustomParameter("Stance", 0, ""),
             new CustomParameter("Agitated", false, ""),
        };
    }
    
    void Start () {
        idle_scene.InitializePerformance(new AssetObjectEventPlayer[] { player }, transform.position, transform.rotation, null);
    }
}


