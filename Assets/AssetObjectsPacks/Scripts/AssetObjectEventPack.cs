using System.Collections.Generic;
using UnityEngine;

namespace AssetObjectsPacks {
    [CreateAssetMenu()]
    [System.Serializable] public class AssetObjectEventPack : ScriptableObject {

        #if UNITY_EDITOR
        public const string asset_objs_field = "assetObjects", pack_id_field = "assetObjectPackID";
        public const string hiddenIDsField = "hiddenIDs";
        public const string multi_edit_instance_field = "multi_edit_instance";
        
        //used for multi anim editing and defaults in editor explorer
        public AssetObject multi_edit_instance;  

        public int[] hiddenIDs;
        #endif
    
        public List<AssetObject> assetObjects = new List<AssetObject>();
        public int assetObjectPackID;
    }
}





