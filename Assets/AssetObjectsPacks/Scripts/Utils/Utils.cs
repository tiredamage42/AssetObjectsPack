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
        public static readonly MiniTransform zero = new MiniTransform(Vector3.zero, Quaternion.identity);
        public Transform targetParent;
        public Vector3 pos;
        public Quaternion rot;

        public void SetTransform (Transform transform) {
            transform.position = pos;
            transform.rotation = rot;
        }
        public void CopyTransform (Transform transform) {
            pos = transform.position;
            rot = transform.rotation;
        }
                            
                            
        public MiniTransform (Vector3 pos, Quaternion rot) 
        => (this.pos, this.rot, this.targetParent) = (pos, rot, null);
        public MiniTransform(Transform t, bool useAsTargetParent) {
            targetParent = useAsTargetParent ? t : null;
            (this.pos, this.rot) = (t.position, t.rotation);
        }
    }

    
    [System.Serializable] public class Smoother {
        public enum SmoothMethod { SmoothDamp, Lerp, MoveTowards }
        public SmoothMethod smoothMethod;
        public float speed = 1.0f;
        float currentVelocity;
        Vector2 currentVelocity2;
        Vector3 currentVelocity3;
        

        public float Smooth(float a, float b, float deltaTime) {
            if (smoothMethod == SmoothMethod.Lerp) 
                return Mathf.Lerp(a, b, speed * deltaTime);
            else if (smoothMethod == SmoothMethod.SmoothDamp) 
                return Mathf.SmoothDamp(a, b, ref currentVelocity, speed);
            else 
                return Mathf.MoveTowards(a, b, speed * deltaTime);
        }
        public Vector2 Smooth(Vector2 a, Vector2 b, float deltaTime) {
            if (smoothMethod == SmoothMethod.Lerp) 
                return Vector2.Lerp(a, b, speed * deltaTime);
            else if (smoothMethod == SmoothMethod.SmoothDamp)
                return Vector2.SmoothDamp(a, b, ref currentVelocity2, speed);
            else
                return Vector2.MoveTowards(a, b, speed * deltaTime);
        }
        public Vector3 Smooth(Vector3 a, Vector3 b, float deltaTime) {
            if (smoothMethod == SmoothMethod.Lerp)
                return Vector3.Lerp(a, b, speed * deltaTime);
            else if (smoothMethod == SmoothMethod.SmoothDamp)
                return Vector3.SmoothDamp(a, b, ref currentVelocity3, speed);
            else 
                return Vector3.MoveTowards(a, b, speed * deltaTime);
        }

        public Quaternion Smooth (Quaternion a, Quaternion b, float deltaTime) {
            if (smoothMethod == SmoothMethod.Lerp)
                return Quaternion.Slerp(a, b, speed * deltaTime);
            else if (smoothMethod == SmoothMethod.SmoothDamp) {

                Vector3 eulerAngles = a.eulerAngles;
                Vector3 targetRotation = b.eulerAngles;
                
                eulerAngles.x = Mathf.SmoothDampAngle(eulerAngles.x, targetRotation.x, ref currentVelocity3.x, speed);
                eulerAngles.y = Mathf.SmoothDampAngle(eulerAngles.y, targetRotation.y, ref currentVelocity3.y, speed);
                eulerAngles.z = Mathf.SmoothDampAngle(eulerAngles.z, targetRotation.z, ref currentVelocity3.z, speed);
                
                return Quaternion.Euler (eulerAngles);
            }

            else {

                Vector3 eulerAngles = a.eulerAngles;
                Vector3 targetRotation = b.eulerAngles;
                
                eulerAngles.x = Mathf.MoveTowardsAngle(eulerAngles.x, targetRotation.x, speed * deltaTime);
                eulerAngles.y = Mathf.MoveTowardsAngle(eulerAngles.y, targetRotation.y, speed * deltaTime);
                eulerAngles.z = Mathf.MoveTowardsAngle(eulerAngles.z, targetRotation.z, speed * deltaTime);
                
                return Quaternion.Euler (eulerAngles);
            }





        }
    }
}


