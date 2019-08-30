
using UnityEngine;
using AssetObjectsPacks;

namespace Game.Combat {
    
    [System.Serializable] public class RecoilTransform {
        public VariableUpdateScript.UpdateMode updateMode;
        public Vector2 toFromSpeed = new Vector2(1,1);
        public Smoother.SmoothMethod smoothMethod;
        public Vector3 targetOffset = new Vector3(1,1,1);
        public Vector3 offsetRandomSign = new Vector3(0,0,0);

    }
    [CreateAssetMenu()] public class RecoilBehavior : ScriptableObject {
        public RecoilTransform position, rotation;
    }
}
