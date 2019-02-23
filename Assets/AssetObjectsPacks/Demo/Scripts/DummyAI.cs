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

    //public Playlist idle_scene;
    public PlaylistHolder walkScene;
    EventPlayer player;
    void Awake () {
        player = GetComponent<EventPlayer>();
        player.playerParams = new CustomParameter[] {
             new CustomParameter("Stance", 0),
             new CustomParameter("Agitated", false),
        };
    }
    
    void Start () {
        //idle_scene.InitializePerformance(new EventPlayer[] { player }, transform.position, transform.rotation, null);
        walkScene.PlayPlaylist(new EventPlayer[] { player }, null);

    }
}


