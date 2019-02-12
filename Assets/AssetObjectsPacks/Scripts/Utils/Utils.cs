
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AssetObjectsPacks {
    public static class StringUtils {
        public static string PadToLength(this string s, int length, string pad = " ") {
            int r = length - s.Length;
            for (int i = 0; i < r; i++) s += pad;
            return s;
        }
        public const string empty = "";
        public static bool IsEmpty(this string s) {
            return string.IsNullOrEmpty(s) || string.IsNullOrWhiteSpace(s);
        }

        #if UNITY_EDITOR
        public static bool IsValidDirectory(this string s) {
            if (s.IsEmpty()) return false;
            return System.IO.Directory.Exists(s);
        }
        public static bool IsValidTypeString(this string s) {
            if (s.IsEmpty()) return false;
            return s.ToType() != null;
        }
        public static System.Type ToType(this string s ) {
            // Try Type.GetType() first. This will work with types defined
            // by the Mono runtime, in the same assembly as the caller, etc.
            var type = System.Type.GetType( s );
 
            if( type != null ) return type;
 
            // If the TypeName is a full name, then we can try loading the defining assembly directly
            if( s.Contains( "." ) ) {
                // Get the name of the assembly (Assumption is that we are using fully-qualified type names)
                var assemblyName = s.Substring( 0, s.IndexOf( '.' ) );
 
                // Attempt to load the indicated Assembly
                var assembly = System.Reflection.Assembly.Load( assemblyName );
                if( assembly != null ) {
                    // Ask that assembly to return the proper Type
                    type = assembly.GetType( s );
                    if( type != null ) return type;
                }
            }
            // If we still haven't found the proper type, we can enumerate all of the 
            // loaded assemblies and see if any of them define the type
            var currentAssembly = System.Reflection.Assembly.GetExecutingAssembly();
            var referencedAssemblies = currentAssembly.GetReferencedAssemblies();
            foreach( var assemblyName in referencedAssemblies ) {
                // Load the referenced assembly
                var assembly = System.Reflection.Assembly.Load( assemblyName );
                if( assembly != null ) {
                    type = assembly.GetType( s );
                    if( type != null ) return type;
                }
            }
            return null;
        }
        #endif
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

}


