using System.Collections.Generic;
using UnityEngine;

namespace AssetObjectsPacks {
    public class AssetObjectsManager : MonoBehaviour
    {
        public PacksManager packs;

        static AssetObjectsManager _instance;
        public static AssetObjectsManager instance {
            get {
                if (_instance == null) {
                    _instance = GameObject.FindObjectOfType<AssetObjectsManager>();
                    if (_instance == null) {
                        _instance = Resources.Load<AssetObjectsManager>("AssetObjectsManager");
                        if (_instance == null) {
                            Debug.LogError("No Asset Objects Manager instance present in the scene!\n\nDrag the prefab from the AssetObjectPacks root directory into the scene.\n\n");
                        }
                    }
                }
                return _instance;
            }
        }

        public void AddUpdateCallback (System.Action update) {
            updateCallbacks += update;
        }
        void OnDisable () {
            if (updateCallbacks != null) {
                foreach(System.Delegate d in updateCallbacks.GetInvocationList())
                    updateCallbacks -= (System.Action)d;
            }
        }
        event System.Action updateCallbacks;
            
        void Awake () {
            
        }
        

        void Update () {
            if (updateCallbacks != null) {
                updateCallbacks();
            }
        }
    }
}
