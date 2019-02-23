using UnityEngine;
namespace AssetObjectsPacks {
    [CreateAssetMenu(fileName = "New Asset Object Event", menuName = "Asset Objects Packs/Event", order = 2)]
    [System.Serializable] public class Event : ScriptableObject {

        #if UNITY_EDITOR
        public const string asset_objs_field = "assetObjects", pack_id_field = "assetObjectPackID";
        public const string hiddenIDsField = "hiddenIDs";
        public const string multi_edit_instance_field = "multi_edit_instance";
        
        //used for multi anim editing and defaults in editor explorer
        public AssetObject multi_edit_instance;  
        public int[] hiddenIDs;
        #endif
    
        public AssetObject[] assetObjects;
        public int assetObjectPackID = -1;
    }
}





