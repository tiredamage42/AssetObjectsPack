#if UNITY_EDITOR
using System.Collections.Generic;
//#endif
using UnityEngine;



namespace AssetObjectsPacks {

    //[ExecuteInEditMode] 
    public class RuntimeTransformTracker// : MonoBehaviour
    {
        //#if UNITY_EDITOR
        public struct MiniTransform {
            public Vector3 pos;
            public Quaternion rot;
            public MiniTransform (Vector3 pos, Quaternion rot) 
            => (this.pos, this.rot) = (pos, rot);
        }

        public static Dictionary<int, MiniTransform> savedTransforms = new Dictionary<int, MiniTransform>();
        public bool tracking;
        int instanceID;
        public void OnEnable (Transform t) {
            instanceID = t.gameObject.GetInstanceID();
            if (!Application.isPlaying) {
                MiniTransform miniTransform;
                if (savedTransforms.TryGetValue(instanceID, out miniTransform)) {
                    t.localPosition = miniTransform.pos;
                    t.localRotation = miniTransform.rot;
                    savedTransforms.Remove(instanceID);
                }
            }
        }
        public void PlayingUpdate (Transform t) {
            if (Application.isPlaying) {
                if (tracking) {
                    savedTransforms[instanceID] = new MiniTransform(t.localPosition, t.localRotation);
                }
                else {
                    savedTransforms.Remove(instanceID);
                }
            }
        }
    }
}

        #endif