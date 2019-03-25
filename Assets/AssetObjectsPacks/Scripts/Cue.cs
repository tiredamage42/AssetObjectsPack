using UnityEngine;
using System.Collections.Generic;
namespace AssetObjectsPacks {


    [System.Serializable] [ExecuteInEditMode]
    public class Cue : MonoBehaviour
    {

        #if UNITY_EDITOR 
        public PlayModeToEditModeTransformTracker transformTracker = new PlayModeToEditModeTransformTracker();
        void Update () {
            transformTracker.Update(transform);
        }
        void OnEnable () {
            transformTracker.OnEnable(transform);
        }
        public Color gizmoColor = Color.green;
        #endif

        public void CalculateLocalPositionAndRotation(out Vector3 localPos, out Quaternion localRot) {
            if (behavior == null) {
                localPos = transform.localPosition;
                localRot = transform.localRotation;
                return;
            }
            localPos = transform.localPosition + behavior.positionOffset;
            localRot = Quaternion.Euler(transform.localRotation.eulerAngles + behavior.rotationOffset);
        }
        public CueBehavior behavior;
        public bool useRandomPlaylist;
        public int repeats;
        public enum SnapPlayerStyle { None, Snap, Smooth };
        public enum MessageEvent { OnStart = 0, OnPlay = 1, OnEnd = 2, OnSnap = 3 };

        public string GetMessageBlock(MessageEvent msgEvent) {
            if (behavior == null) {
                return null;
            }
            return behavior.messageBlocks[(int)msgEvent];
        }
    }
}