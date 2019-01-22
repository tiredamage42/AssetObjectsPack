
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AssetObjectsPacks {
    public abstract class AssetObjectEvent
    <O, E, B, P> : MonoBehaviour
    where O : AssetObject
    where E : AssetObjectEvent<O, E, B, P>
    where B : AssetObjectEventBehavior<O, E, B, P>
    where P : AssetObjectEventPlayer<O, E, B, P>
    {
        #if UNITY_EDITOR
        //used for multi anim editing and defaults in editor explorer
        public O multi_edit_instance;        
        public List<int> hidden_ids;
        #endif

        public List<O> assetObjects = new List<O>();

        B[] _b;
        public B[] behaviors {
            get {
                if (_b == null) {
                    _b = GetComponents<B>();
                    if (_b.Length == 0) {
                        Debug.LogError("no behaviors");
                    }
                }
                return _b;
            }
        }
    }
}




