using System.Collections.Generic;
using UnityEngine;

namespace AssetObjectsPacks.Animations {
    public class AnimationsPack : MonoBehaviour {
        public static readonly string[] p_Loop_Names = new string[] {"Loops_0", "Loops_1"};
        public static readonly string[] p_Loop_Indicies = new string[] {"LoopIndex_0", "LoopIndex_1"};
        public static readonly string[] p_Loop_Mirrors = new string[] {"p_LoopMirror_0", "p_LoopMirror_1"};
        public static readonly string[] p_Loop_Speeds = new string[] {"p_LoopSpeed_0", "p_LoopSpeed_1"};
        public const string p_Mirror = "Mirror";
        public const string p_Speed = "Speed";
        public const string p_ActiveLoopSet = "ActiveLoopSet";
        public const string shots_name = "OneShots";

        /*
        static AnimationCorpus _instance;
        public static AnimationCorpus instance {
            get {
                if (_instance == null) {
                    _instance = GameObject.FindObjectOfType<AnimationCorpus>();
                }
                return _instance;
            }
        }
        
        void Update () {
            PerformanceManager.UpdateManager();
        }

        public static class PerformanceManager {
            static List<int> active_performances = new List<int>();
            static Pool<AnimationScene.Performance> performance_pool = new Pool<AnimationScene.Performance>();
            public static AnimationScene.Performance GetNewPerformance () {
                int new_performance_key = performance_pool.GetNewObject();
                active_performances.Add(new_performance_key);
                AnimationScene.Performance p = performance_pool[new_performance_key];
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
        }
        */
    }
}




