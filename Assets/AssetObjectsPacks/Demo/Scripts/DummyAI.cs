using UnityEngine;
using AssetObjectsPacks;

public class DummyAI : MonoBehaviour{
    public bool agitated;

    public Playlist demoScene;
    EventPlayer player;
    void Awake () {
        player = GetComponent<EventPlayer>();        
        player.AddParameters( new CustomParameter[] {
            new CustomParameter("Agitated", () => agitated),
        } );
    }
    
    void Start () {
        demoScene.InitializePerformance(new EventPlayer[] { player }, true, null);
    }
}


