
using UnityEngine;

namespace AssetObjectsPacks.Animations {
    [System.Serializable] public class AnimationAssetObject : AssetObject {
        public enum MirrorMode { None, Mirror, Random };
        public MirrorMode mirror_mode;
        public float speed = 1.0f;
        public float transition_speed = .1f;
    }
}
