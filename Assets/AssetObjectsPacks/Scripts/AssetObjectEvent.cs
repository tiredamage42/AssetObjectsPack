
//using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AssetObjectsPacks {
    [System.Serializable] public abstract class AssetObjectEvent
    : MonoBehaviour
    //<E, P> : MonoBehaviour
    //where E : AssetObjectEven<E, P>
    //where B : AssetObjectEventBehavio<E, B, P>
    //where P : AssetObjectEvenPlayer<E, P>
    
    {
        #if UNITY_EDITOR
        //used for multi anim editing and defaults in editor explorer
        public AssetObject multi_edit_instance;  
        public List<int> hidden_ids;
        #endif

        public bool looped;
        public float duration = -1; // <= 0 for animation/audio duration
        public List<AssetObject> assetObjects = new List<AssetObject>();



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
    }
}




