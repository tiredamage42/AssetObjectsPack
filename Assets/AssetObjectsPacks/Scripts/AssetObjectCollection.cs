
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*
namespace AssetObjectsPacks {
    [System.Serializable] public abstract class AssetObjectEvent
    <O, E, B, P> : MonoBehaviour
    where O : AssetObject
    where E : AssetObjectEven<O, E, B, P>
    where B : AssetObjectEventBehavio<O, E, B, P>
    where P : AssetObjectEvenPlayer<O, E, B, P>
    {
        #if UNITY_EDITOR
        //used for multi anim editing and defaults in editor explorer
        public List<O> multi_edit_instance;        
        public List<int> hidden_ids;
        #endif

        [SerializeField] public List<AssetObject> assetObjects = new List<AssetObject>();

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
 */




