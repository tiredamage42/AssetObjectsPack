

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AssetObjectsPacks {
    public abstract class AssetObjectEventBehavior
    <O, E, B, P> : MonoBehaviour
    where O : AssetObject
    where E : AssetObjectEvent<O, E, B, P>
    where B : AssetObjectEventBehavior<O, E, B, P>
    where P : AssetObjectEventPlayer<O, E, B, P>
    {
        public abstract List<O> FilterEventAssets (P player, List<O> original_list);
    }
}


