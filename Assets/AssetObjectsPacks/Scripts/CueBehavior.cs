using UnityEngine;
namespace AssetObjectsPacks {
    [CreateAssetMenu(fileName = "Cue Behavior", menuName = "Asset Objects Packs/Cue Behavior", order = 1)]
    public class CueBehavior : ScriptableObject {
        [HideInInspector] public Event[] events;
        [HideInInspector] public float overrideDuration = -1;
        [HideInInspector] public bool playImmediate;
        //if the event should wait for the player to snap to the interest transform
        //before being considered ready
        [HideInInspector] public Cue.SnapPlayerStyle snapPlayerStyle;
        [HideInInspector] public float smoothPositionTime = 1;
        [HideInInspector] public float smoothRotationTime = 1;    
        [HideInInspector] public Vector3 positionOffset, rotationOffset;
        [HideInInspector] public string[] messageBlocks = new string[4] {"", "", "", ""};
    }
}