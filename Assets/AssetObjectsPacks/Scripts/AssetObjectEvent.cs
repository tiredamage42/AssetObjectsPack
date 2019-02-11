
//using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AssetObjectsPacks {
/*
    [System.Serializable] public class AssetObjectEventPack {

        #if UNITY_EDITOR
        public const string asset_objs_field = "assetObjects", pack_id_field = "assetObjectPackID";
        public const string hidden_ids_field = "hidden_ids", multi_edit_instance_field = "multi_edit_instance";
        
        //used for multi anim editing and defaults in editor explorer
        public AssetObject multi_edit_instance;  
        public List<int> hidden_ids;
        #endif
    
        public List<AssetObject> assetObjects = new List<AssetObject>();
        public int assetObjectPackID;
    }

 */
    [System.Serializable] public class AssetObjectEvent : MonoBehaviour
    {
        #if UNITY_EDITOR
        public const string playlist_field = "playlist", event_packs_field = "eventPacks";
        public const string main_pack_index_field = "main_pack_index";
        public const string snap_player_style_field = "snapPlayerStyle";
        public const string smooth_pos_time_field = "smoothPositionTime", smooth_rot_time_field = "smoothRotationTime";
        #endif
        
        public AssetObjectEventPlaylist playlist;


        public AssetObjectEventPack[] eventPacks;
        //public List<AssetObjectEventPack> event_packs = new List<AssetObjectEventPack>();

        //the event pack that determines when the event is done 
        //-1 for wait for all
        public int main_pack_index;

        
        AssetObjectEventBehavior[] _b;
        public AssetObjectEventBehavior[] behaviors {
            get {
                if (_b == null) {
                    _b = GetComponents<AssetObjectEventBehavior>();
                    if (_b.Length == 0) {
                        Debug.LogError("no behaviors");
                    }
                }
                return _b;
            }
        }

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




