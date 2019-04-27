using UnityEngine;
using System.Collections.Generic;
namespace AssetObjectsPacks {


    [System.Serializable] [ExecuteInEditMode]
    public class Cue : MonoBehaviour
    {

        #if UNITY_EDITOR 
        public Color gizmoColor = Color.green;
        void OnDrawGizmos () {
            Gizmos.color = gizmoColor;
            Gizmos.DrawWireSphere(transform.position, .5f);
        }
        #endif


        public CueBehavior behavior;
        public bool useRandomPlaylist;
        public int repeats;
        public enum SnapPlayerStyle { None, Snap, Smooth };
        public enum MessageEvent { OnStart = 0, OnPlay = 1, OnEnd = 2, OnSnap = 3 };
    }
}