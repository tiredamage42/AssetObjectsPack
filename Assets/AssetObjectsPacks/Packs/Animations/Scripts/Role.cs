



using UnityEngine;

namespace AssetObjectsPacks.Animations {
    public class Role : MonoBehaviour {
        AnimationEvent[] _cues;
        public AnimationEvent[] cues {
            get {
                if (_cues == null || _cues.Length == 0) {
                    _cues = GetComponentsInChildren<AnimationEvent>();
                }
                return _cues;
            }
        }
    }
}

