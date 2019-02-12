

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;
using UnityEditor;

namespace AssetObjectsPacks {
    public static class EditorUtils {

        
        static System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();

        public static void StartTimer() {
            stopwatch.Reset();
            stopwatch.Start();
        
        }
        public static void PrintTimer(string message) {
            stopwatch.Stop();
            System.TimeSpan ts = stopwatch.Elapsed;
            string elapsedTime = string.Format("{0:00}.{1:00}", ts.Seconds, ts.Milliseconds / 10);
            Debug.Log(message + " :: " + elapsedTime);
            StartTimer();
        }

        public static string MakeDirectoryIfNone (string directory) {
            if(!Directory.Exists(directory)) Directory.CreateDirectory(directory);
            return directory;
        }
        public static string[] GetFilePathsInDirectory (string dir, bool include_dir, string file_extenstions, string valid_file_check, bool should_contain, SearchOption search = SearchOption.AllDirectories) {
            string data_path = Application.dataPath.Substring(0, Application.dataPath.Length - 6);
            int sub_index = data_path.Length + 1 + (include_dir ? 0 : dir.Length);//Assets ...;  
            List<string> results = new List<string>();
            string[] extensions = file_extenstions.Split(',');
            for (int i = 0; i < extensions.Length; i++) {
                results.AddRange(Directory.GetFiles(data_path+"/"+dir, "*" + extensions[i], search)
                .Where(s => s.Contains(valid_file_check) == should_contain)
                .Select(s => s.Substring(sub_index)));
            }
            return results.ToArray();
        }

        public static T GetAssetAtPath<T> (string path) where T : Object {
            Object[] data = AssetDatabase.LoadAllAssetsAtPath(path);
            System.Type t = typeof(T);
            int l = data.Length;
            for (int i = 0; i < l; ++i) {
                Object d = data[i];
                if (d.GetType() == t) {
                    return (T)d;
                }
            }
            return null;
        }
        public static Object GetAssetAtPath (string path, string type_name) {
            Object[] data = AssetDatabase.LoadAllAssetsAtPath(path);
            int l = data.Length;
            for (int i = 0; i < l; ++i) {
                Object d = data[i];
                if (d.GetType().ToString() == type_name) {
                    return d;
                }
            }
            return null;
        }
        public static Object[] GetAssetsAtPath (string path, string type_name) {
            List<Object> ret = new List<Object>();
            Object[] data = AssetDatabase.LoadAllAssetsAtPath(path);
            int l = data.Length;
            for (int i = 0; i < l; ++i) {
                Object d = data[i];
                if (d.GetType().ToString() == type_name) {
                    ret.Add(d);
                }
            }
            return ret.ToArray();
        }
        
        public static T[] GetAssetsAtPath<T> (string path) where T : Object {
            System.Type t = typeof(T);
            List<T> ret = new List<T>();
            Object[] data = AssetDatabase.LoadAllAssetsAtPath(path);
            int l = data.Length;
            for (int i = 0; i < l; ++i) {
                Object d = data[i];
                if (d.GetType() == t) {
                    ret.Add((T)d);
                }
            }
            return ret.ToArray();
                
        }
        const string back_slash = "/";
        const char back_slash_c = '/';


        public static string RemoveDirectory (string full_path) {
            if (!full_path.Contains(back_slash)) {
                return full_path;
            }
            return full_path.Split(back_slash_c).Last();
        }
         
        public static string[] DirectoryNameSplit(string full_path) {
            if (!full_path.Contains(back_slash)) {
                Debug.LogError(full_path + " isnt a valid path");
                return null;
            }
            string[] sp = full_path.Split(back_slash_c);
            string name = sp.Last();
            string dir = string.Join(back_slash, sp.Slice(0, -2)) + back_slash;
            
            return new string[] {dir, name};
        }



        #region SERIALIZED_PROPERTY_EXTENSIONS

        public static bool Contains (this SerializedProperty p, string e, out int at_index) {
            at_index = -1;
            int a = p.arraySize;
            
            for (int i = 0; i < a; i++) {
                if (p.GetArrayElementAtIndex(i).stringValue == e) {
                    at_index = i;
                    return true;
                }
            }
            return false;
        }
        public static bool Contains (this SerializedProperty p, int e, out int at_index) {
            at_index = -1;
            int a = p.arraySize;
            for (int i = 0; i < a; i++) {
                if (p.GetArrayElementAtIndex(i).intValue == e) {
                    at_index = i;
                    return true;
                }
            }
            return false;
        }

        public static SerializedProperty AddNewElement(this SerializedProperty p) {
            int l = p.arraySize;
            p.InsertArrayElementAtIndex(l);
            return p.GetArrayElementAtIndex(l);
        }
        public static void Add (this SerializedProperty p, string e) {
            p.AddNewElement().stringValue = e;
        }
        public static void Add (this SerializedProperty p, int e) {
            p.AddNewElement().intValue = e;
        }

        public static void Remove (this SerializedProperty p, string e) {
            int i;
            if (p.Contains(e, out i)) p.DeleteArrayElementAtIndex(i);
        }
        public static void Remove (this SerializedProperty p, int e) {
            int i;
            if (p.Contains(e, out i)) p.DeleteArrayElementAtIndex(i);
        }
        /*
         */

        public static void RemoveRange(this SerializedProperty p, IList<int> l) {
            int c = p.arraySize;
            for (int i = c - 1; i >= 0; i--) {
                if (l.Contains(p.GetArrayElementAtIndex(i).intValue)) p.DeleteArrayElementAtIndex(i);
            }

        }
        public static bool Contains (this SerializedProperty p, string e) {
            int at_index;
            return p.Contains(e, out at_index);
        }
        public static bool Contains (this SerializedProperty p, int e) {
            int at_index;
            return p.Contains(e, out at_index);
        }

        
        public static void CopyProperty(this SerializedProperty p, SerializedProperty c) {
            if (p.propertyType != c.propertyType) {
                Debug.LogError("Incompatible types (" + p.propertyType + ", " + c.propertyType + ")");
                return;
            }
            switch (p.propertyType){
                case SerializedPropertyType.Integer	:p.intValue = c.intValue;break;
                case SerializedPropertyType.Boolean	:p.boolValue = c.boolValue;break;
                case SerializedPropertyType.Float	:p.floatValue = c.floatValue;break;
                case SerializedPropertyType.String	:p.stringValue = c.stringValue;break;
                case SerializedPropertyType.Color	:p.colorValue = c.colorValue;break;
                case SerializedPropertyType.ObjectReference	:p.objectReferenceValue = c.objectReferenceValue;break;
                case SerializedPropertyType.Enum	:p.enumValueIndex = c.enumValueIndex;break;
                case SerializedPropertyType.Vector2	:p.vector2Value = c.vector2Value;break;
                case SerializedPropertyType.Vector3	:p.vector3Value = c.vector3Value;break;
                case SerializedPropertyType.Vector4	:p.vector4Value = c.vector4Value;break;
                case SerializedPropertyType.Rect	:p.rectValue = c.rectValue;break;
                case SerializedPropertyType.AnimationCurve	:p.animationCurveValue = c.animationCurveValue;break;
                case SerializedPropertyType.Bounds	:p.boundsValue = c.boundsValue;break;
                case SerializedPropertyType.Quaternion	:p.quaternionValue = c.quaternionValue;break;
                case SerializedPropertyType.ExposedReference:	p.exposedReferenceValue = c.exposedReferenceValue;break;
                case SerializedPropertyType.Vector2Int:	p.vector2IntValue = c.vector2IntValue;break;
                case SerializedPropertyType.Vector3Int:	p.vector3IntValue = c.vector3IntValue;break;
                case SerializedPropertyType.RectInt	:p.rectIntValue = c.rectIntValue;break;
                case SerializedPropertyType.BoundsInt	:p.boundsIntValue = c.boundsIntValue;break;
                default:Debug.LogError("Not implemented: " + p.propertyType);break;
            }
        }

        #endregion


       
        
        
        



        
        
        
        
        
    }

}







