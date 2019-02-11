

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AssetObjectsPacks {
    public abstract class AssetObjectEventBehavior{
        public string event_pack_name;
        public abstract List<AssetObject> FilterEventAssets (AssetObjectEventPlayer player, List<AssetObject> original_list);
    }
}


