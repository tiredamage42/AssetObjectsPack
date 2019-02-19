using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEditor;
using System.Linq;
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

        public static string[] GetFilePathsInDirectory (string dir, bool include_dir, string file_extenstions, string valid_file_check, bool should_contain, SearchOption search = SearchOption.AllDirectories) {
            string data_path = Application.dataPath.Substring(0, Application.dataPath.Length - 6);
            int cutoff = data_path.Length + 1 + (include_dir ? 0 : dir.Length);//Assets ...;  
            string checkDir = data_path + "/" + dir;
            List<string> results = new List<string>();
            string[] extensions = file_extenstions.Split(',');
            int l = extensions.Length;
            for (int i = 0; i < l; i++) {
                results.AddRange(
                    Directory.GetFiles(checkDir, "*" + extensions[i], search).Where(s => s.Contains(valid_file_check) == should_contain).Select(s => s.Substring(cutoff))
                );
            }
            return results.ToArray();
        }

        
        // fbx files have extra "preview" clip that was getting in the awy (mixamo)
        public static T GetAssetAtPath<T> (string path) where T : Object {
            Object[] data = AssetDatabase.LoadAllAssetsAtPath(path);
            System.Type t = typeof(T);
            int l = data.Length;
            for (int i = 0; i < l; ++i) {
                Object d = data[i];
                if (d.name.Contains("__preview__")) continue;
                if (d.GetType() != t) continue; 
                return (T)d;
            }
            return null;
        }
        public static Object GetAssetAtPath (string path, string type_name) {
            Object[] data = AssetDatabase.LoadAllAssetsAtPath(path);
            int l = data.Length;
            for (int i = 0; i < l; ++i) {
                Object d = data[i];
                if (d.name.Contains("__preview__")) continue;
                if (d.GetType().ToString() != type_name) continue;
                return d;
            }
            return null;
        }

        const string back_slash = "/";
        const char back_slash_c = '/';

        public static string RemoveDirectory (string full_path) {
            if (!full_path.Contains(back_slash)) return full_path;
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
    }
}