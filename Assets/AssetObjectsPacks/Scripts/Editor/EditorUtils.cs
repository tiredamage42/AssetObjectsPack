using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
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

        public static string[] GetFilePathsInDirectory (string dir, bool includeDir, string extensions, string validCheck, bool shouldContain, SearchOption search = SearchOption.AllDirectories) {
            string dPath = Application.dataPath.Substring(0, Application.dataPath.Length - 6);
            int cutoff = dPath.Length + 1 + (includeDir ? 0 : dir.Length);//Assets ...;  
            string checkDir = dPath + "/" + dir;
            List<string> results = new List<string>();
            string[] ext = extensions.Split(',');
            for (int i = 0; i < ext.Length; i++) results.AddRange(Directory.GetFiles(checkDir, "*" + ext[i], search).Where(s => s.Contains(validCheck) == shouldContain).Select(s => s.Substring(cutoff)));
            return results.ToArray();
        }

        
        // fbx files have extra "preview" clip that was getting in the awy (mixamo)
        public static T GetAssetAtPath<T> (string path) where T : Object {
            Object[] data = AssetDatabase.LoadAllAssetsAtPath(path);
            for (int i = 0; i < data.Length; ++i) {
                Object d = data[i];
                if (d.name.Contains("__preview__")) continue;
                if (d.GetType() != typeof(T)) continue; 
                return (T)d;
            }
            return null;
        }
        public static Object GetAssetAtPath (string path, string type_name) {
            Object[] data = AssetDatabase.LoadAllAssetsAtPath(path);
            for (int i = 0; i < data.Length; ++i) {
                Object d = data[i];
                if (d.name.Contains("__preview__")) continue;
                if (d.GetType().ToString() != type_name) continue;
                return d;
            }
            return null;
        }

        public static IEnumerable<T> GetAllAssetsOfType<T> () where T : Object {
            string nm = typeof(T).Name;
            string[] guids = AssetDatabase.FindAssets("t:"+ nm);  
            int l = guids.Length;
            if (l == 0) {
                Debug.LogWarning("No " + nm + " Objects Found");
                return null;
            }
            return l.Generate(i => AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guids[i])));
        }

        public static string RemoveDirectory (string path) {
            if (!path.Contains("/")) return path;
            return path.Split('/').Last();
        }
        public static string[] DirectoryNameSplit(string path) {
            if (!path.Contains("/")) return new string[] { "", path };
            string[] sp = path.Split('/');
            string name = sp.Last();
            string dir = string.Join("/", sp.Slice(0, -2)) + "/";
            return new string[] {dir, name};
        }
    }
}