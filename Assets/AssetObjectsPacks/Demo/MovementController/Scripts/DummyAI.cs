using UnityEngine;
using AssetObjectsPacks;

public class DummyAI : MonoBehaviour{
    [Header("Debug")]
    public Transform debugTransformLook;
    void DebugLoop () {
        if (debugTransformLook) {
            movement.mover.SetFacePosition(debugTransformLook.position);
        }
    }

    public bool agitated;

    public Cue demoScene;
    EventPlayer player;
    MovementController movement;

    
    void Awake () {
        player = GetComponent<EventPlayer>();        
        player.AddParameters( new CustomParameter[] {
            new CustomParameter("Agitated", () => agitated),
        } );
        movement = GetComponent<MovementController>();
        
    }
    
    void Start () {
        Playlist.InitializePerformance(new Cue[] { demoScene }, new EventPlayer[] {player}, demoScene.transform.position, demoScene.transform.rotation, true, -1, null);
        //demoScene.InitializePerformance(player, true, null);
    }

    void Update () {
        DebugLoop();
    }
}


