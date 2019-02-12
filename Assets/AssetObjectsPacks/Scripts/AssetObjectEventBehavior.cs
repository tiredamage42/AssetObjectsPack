

using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace AssetObjectsPacks {

    public enum ConditionMode { If, IfNot, Equals, NotEquals };
    public abstract class AssetObjectEventBehavior : MonoBehaviour{

        [System.Serializable] public class CustomCase {

        }





        public string event_pack_name;
        public abstract List<AssetObject> FilterEventAssets (AssetObjectEventPlayer player, List<AssetObject> original_list);
    }
}


