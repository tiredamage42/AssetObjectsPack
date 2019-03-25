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
            foreach(System.Delegate d in updateCallbacks.GetInvocationList())
                updateCallbacks -= (System.Action)d;
        }
            
        void Awake () {
            AddUpdateCallback( Playlist.UpdatePerformances );
        }
        event System.Action updateCallbacks;
        

        void Update () {
            updateCallbacks();
            //UpdateManager();
        }
/*
        static List<int> active_performances = new List<int>();
        static Pool<Playlist.Performance> performance_pool = new Pool<Playlist.Performance>();
        public static Playlist.Performance GetNewPerformance () {
            int new_performance_key = performance_pool.GetNewObject();
            active_performances.Add(new_performance_key);
            Playlist.Performance p = performance_pool[new_performance_key];
            p.SetPerformanceKey(new_performance_key);
            return p;
        }
        public static void UpdateManager () {
            for (int i = 0; i < active_performances.Count; i++) {
                performance_pool[active_performances[i]].UpdatePerformance();
            }
        }
        public static void ReturnPerformanceToPool(int key) {
            performance_pool.ReturnToPool(key);
            active_performances.Remove(key);
        }
 */
    }
}
