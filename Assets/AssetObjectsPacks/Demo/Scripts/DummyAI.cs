using UnityEngine;
using AssetObjectsPacks;

public class DummyAI : MonoBehaviour{
    public bool agitated;

    void UpdateParameters () {
        player["Agitated"].SetValue(agitated);        
    }

    void Update () {
        UpdateParameters();
    }
    
    public PlaylistHolder walkScene;
    EventPlayer player;
    void Awake () {
        player = GetComponent<EventPlayer>();        
        player.AddParameters( new CustomParameter[] {
            new CustomParameter("Agitated", false),
        } );
    }
    
    void Start () {
        walkScene.PlayPlaylist(new EventPlayer[] { player }, null);
    }
}


