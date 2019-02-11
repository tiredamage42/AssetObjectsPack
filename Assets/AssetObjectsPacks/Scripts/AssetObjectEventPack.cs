using System.Collections.Generic;
using UnityEngine;

namespace AssetObjectsPacks {
    [CreateAssetMenu()]
    [System.Serializable] public class AssetObjectEventPack : ScriptableObject {

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
}





