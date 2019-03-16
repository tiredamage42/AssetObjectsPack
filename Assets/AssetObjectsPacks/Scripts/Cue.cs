using UnityEngine;
using System.Collections.Generic;
namespace AssetObjectsPacks {


    [System.Serializable] [ExecuteInEditMode]
    public class Cue : MonoBehaviour
    {

        
        #if UNITY_EDITOR 

        public RuntimeTransformTracker transformTracker = new RuntimeTransformTracker();
        

        void Update () {
            transformTracker.PlayingUpdate(transform);
        }
        void OnEnable () {
            transformTracker.OnEnable(transform);
        }

        #endif





        public Color gizmoColor = Color.green;
        //public Transform playlist;
        public bool useRandomPlaylist;

        public Event[] events;

        public float overrideDuration = -1;
        public int repeats;
        public bool playImmediate;

        //if the event should wait for the player to snap to the interest transform
        //before being considered ready
        public enum SnapPlayerStyle { None, Snap, Smooth };
        public SnapPlayerStyle snapPlayerStyle;
        public float smoothPositionTime = 1;
        public float smoothRotationTime = 1;    

        
        public enum MessageEvent { OnStart = 0, OnPlay = 1, OnEnd = 2, OnSnap = 3 };
        public string[] messageBlocks = new string[4] {"", "", "", ""};

        public string GetMessageBlock(MessageEvent msgEvent) {
            return messageBlocks[(int)msgEvent];
        }

        Dictionary<string, Event> packName2Event = new Dictionary<string, Event>();
        
        //check only at runtime
        public Event GetEventByName (string packName) {
            int l = events.Length;
            if (events.Length == 0) return null;
            if (l != packName2Event.Count) {
                for (int i = 0; i < l; i++) {
                    string thisPackName = AssetObjectsManager.instance.packs.FindPackByID( events[i].assetObjectPackID, out _).name;
                    packName2Event.Add(thisPackName, events[i]);
                }
            }
            Event e;
            if (packName2Event.TryGetValue(packName, out e)) {
                return e;
            }
            return null;
        }
    }
}