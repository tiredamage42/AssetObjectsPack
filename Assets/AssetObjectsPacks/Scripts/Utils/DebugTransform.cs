using UnityEngine;
namespace AssetObjectsPacks {
    public class DebugTransform : MonoBehaviour {
        static DebugTransform i;
        public static DebugTransform instance {
            get {
                if (i == null) i = GameObject.FindObjectOfType<DebugTransform>();
                return i;
            }
        }
    }
}