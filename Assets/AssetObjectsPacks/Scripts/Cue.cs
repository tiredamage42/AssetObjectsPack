using UnityEngine;
using System.Collections.Generic;
namespace AssetObjectsPacks {

    [System.Serializable] public class Cue : MonoBehaviour
    {
        #if UNITY_EDITOR
        public const string playlist_field = "playlist", event_packs_field = "events";
        public const string snap_player_style_field = "snapPlayerStyle";
        public const string sendMessageField = "sendMessage";
        public const string smooth_pos_time_field = "smoothPositionTime", smooth_rot_time_field = "smoothRotationTime";
        #endif
        
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







        
        public string sendMessage;

        //if the event should wait for the player to snap to the interest transform
        //before being considered ready
        public enum SnapPlayerStyle { None, Snap, Smooth };
        public SnapPlayerStyle snapPlayerStyle;
        public float smoothPositionTime = 1;
        public float smoothRotationTime = 1;    

        void OnDrawGizmos () {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, .25f);
        }
    }
}