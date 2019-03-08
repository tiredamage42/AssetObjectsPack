using UnityEngine;
using System.Collections.Generic;
namespace AssetObjectsPacks {

    [System.Serializable] public class Cue : MonoBehaviour
    {
        #if UNITY_EDITOR
        public const string playlist_field = "playlist", event_packs_field = "events", playImmediateField = "playImmediate";
        public const string messagesBlockField = "messagesBlock";

        public const string overrideDurationField = "overrideDuration", repeatsField = "repeats";

        public const string snap_player_style_field = "snapPlayerStyle";
        public const string sendMessageField = "sendMessage", postMessageField = "postMessage";
        public const string smooth_pos_time_field = "smoothPositionTime", smooth_rot_time_field = "smoothRotationTime";
        
        #endif
        public Color gizmoColor = Color.green;
        
        public Playlist playlist;
        public Event[] events;



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





        public float overrideDuration = -1;
        public int repeats;
        public bool playImmediate;




        
        public string sendMessage;

        public string messagesBlock;

        public string postMessage;

        //if the event should wait for the player to snap to the interest transform
        //before being considered ready
        public enum SnapPlayerStyle { None, Snap, Smooth };
        public SnapPlayerStyle snapPlayerStyle;
        public float smoothPositionTime = 1;
        public float smoothRotationTime = 1;    

        
        
        
        
        
        
    }
}