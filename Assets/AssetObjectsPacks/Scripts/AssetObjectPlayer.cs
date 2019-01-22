
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
namespace AssetObjectsPacks {

    public abstract class AssetObjectEventPlayer
    <O, E, B, P> : MonoBehaviour
    where O : AssetObject
    where E : AssetObjectEvent<O, E, B, P>
    where B : AssetObjectEventBehavior<O, E, B, P>
    where P : AssetObjectEventPlayer<O, E, B, P>
    {
        P _s = null;
        P self {
            get {
                if (_s == null) _s = (P)this;
                return _s;
            }
        }

        //public void UseAssetObject (O asset_object) {
        //    OnUseAssetObject(asset_object);
        //}
        public void PlayEvent (E asset_object_event, Action on_end_use) {
            List<O> objs = new List<O>(asset_object_event.assetObjects);
            for (int i = 0; i < asset_object_event.behaviors.Length; i++) {
                objs = asset_object_event.behaviors[i].FilterEventAssets(self, objs);
            }
            OnUseAssetObjectHolder(asset_object_event, objs.RandomChoice(), on_end_use);
        }
        protected abstract void OnUseAssetObjectHolder (E asset_object_event, O chosen_obj, Action on_end_use);
        //protected abstract void OnUseAssetObject (O asset_object);
        

        
    }
}

