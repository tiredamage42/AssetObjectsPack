using UnityEngine;

namespace AssetObjectsPacks {

    [System.Serializable] public class AssetObjectEvent : MonoBehaviour
    {
        #if UNITY_EDITOR
        public const string playlist_field = "playlist", event_packs_field = "eventPacks";
        public const string snap_player_style_field = "snapPlayerStyle";
        public const string smooth_pos_time_field = "smoothPositionTime", smooth_rot_time_field = "smoothRotationTime";
        #endif
        
        public AssetObjectEventPlaylist playlist;
        public AssetObjectEventPack[] eventPacks;
        

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




