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


        // void OnDrawGizmos () {

        //     float angleSubdivision = 45.0f;

        //     Vector3 fwd = transform.forward;
        //     Vector3 pos = transform.position;
            
        //     Gizmos.color = Color.red;
        //     for (float x = 0.0f; x <= 360.0f; x+=angleSubdivision) {
        //         Vector3 dir = Quaternion.Euler(0, x, 0) * fwd;
        //         Gizmos.DrawLine(pos, pos + dir * 5);
        //     }
        // }
    }
}