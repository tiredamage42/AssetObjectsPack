using System.Collections.Generic;
using UnityEngine;
namespace AssetObjectsPacks {
    public static class StringUtils {
        

        public static bool IsEmpty(this string s) {
            return string.IsNullOrEmpty(s) || string.IsNullOrWhiteSpace(s);
        }
        public static System.Type ToType(this string s ) {
            // Try Type.GetType() first. This will work with types defined
            // by the Mono runtime, in the same assembly as the caller, etc.
            var type = System.Type.GetType( s ); 
            if( type != null ) return type;
            
            var assemblyName = "UnityEngine";    
            // Attempt to load the indicated Assembly
            var assembly = System.Reflection.Assembly.Load( assemblyName );
            // Ask that assembly to return the proper Type
            type = assembly.GetType( assemblyName + "." + s );
            if( type != null ) return type;
            return null;
        }
    }
    public class Pool<T> where T : new() {
        List<T> pool = new List<T>();
        Queue<int> available = new Queue<int>();
        public int GetNewObject () {
            if (available.Count == 0) {
                pool.Add(new T());
                return pool.Count - 1;
            }
            return available.Dequeue();
        }
        public void ReturnToPool (int key) {
            if (!available.Contains(key)) available.Enqueue(key);
        }
        public T this[int key] { get { return pool[key]; } }
    }

    public struct MiniTransform {

        public Transform targetParent;
        public Vector3 pos;
        public Quaternion rot;
        public MiniTransform (Vector3 pos, Quaternion rot) 
        => (this.pos, this.rot, this.targetParent) = (pos, rot, null);
        public MiniTransform(Transform t, bool useAsTargetParent) {
            if (useAsTargetParent) {
                targetParent = t;
            }
            else {
                targetParent = null;
            }
            (this.pos, this.rot) = (t.position, t.rotation);
        }
    }


    public class SmoothValue {
        public enum SmoothType { Lerp = 0, MoveTowards = 1, SmoothDamp = 2 }
        float v;
        public float Smooth (float orig, float target, float speed, float deltaTime, SmoothType smoothType) {
            switch(smoothType) {
                case SmoothType.Lerp:
                    return Mathf.Lerp(orig, target, speed * deltaTime);
                case SmoothType.MoveTowards:
                    return Mathf.MoveTowards(orig, target, speed * deltaTime);
                case SmoothType.SmoothDamp:
                    return Mathf.SmoothDamp(orig, target, ref v, speed);
            }
            return 0;
        }
    }
}


