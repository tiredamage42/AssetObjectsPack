using UnityEngine;
using AssetObjectsPacks;

public class DummyAI : MonoBehaviour{
    public bool agitated;
    public PlaylistHolder walkScene;
    EventPlayer player;
    void Awake () {
        player = GetComponent<EventPlayer>();        
        player.AddParameters( new CustomParameter[] {
            new CustomParameter("Agitated", () => agitated),
        } );
    }
    
    void Start () {
        walkScene.PlayPlaylist(new EventPlayer[] { player }, null);
    }
}


