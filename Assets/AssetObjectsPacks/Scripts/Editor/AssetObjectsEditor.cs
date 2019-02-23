using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
namespace AssetObjectsPacks {
    public static class AssetObjectsEditor 
    {
        static PacksManager pm;
        public static PacksManager packManager {
            get {
                if (pm == null) pm = GetPackManager();
                return pm;
            }
        }
        static PacksManager GetPackManager () {
            string[] guids = AssetDatabase.FindAssets("t:"+ typeof(PacksManager).Name);  
            int l = guids.Length;
            if (l == 0) {
                Debug.LogError("No PacksManager Object Found");
                return null;
            }
            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            if (l > 1) Debug.LogWarning("Multiple PackManagers found, using: '" + path + "'");
            return AssetDatabase.LoadAssetAtPath<PacksManager>(path);
        }
       
        const string sIDKey = "@ID-";  
        const string back_slash = "/", dash = "-";
        const char back_slash_c = '/', dash_c = '-', dot_c = '.';    

        public static string[] GetAllAssetObjectPaths (string directory, string file_extensions, bool include_dir) {    
            return EditorUtils.GetFilePathsInDirectory(directory, include_dir, file_extensions, sIDKey, true);
        }
        public static string[] GetAllAssetObjectPathsWithoutIDs (string directory, string file_extensions) {
            return EditorUtils.GetFilePathsInDirectory(directory, true, file_extensions, sIDKey, false);
        }
        public static string[] GetAllAssetObjectPaths (string directory, string file_extensions, bool include_dir, out Dictionary<int, string> id2file) {
            string[] paths = GetAllAssetObjectPaths(directory, file_extensions, include_dir);
            int l = paths.Length;
            id2file = new Dictionary<int, string>(l);
            for (int i = 0; i < l; i++) id2file.Add(GetObjectIDFromPath(paths[i]), paths[i]);
            return paths;
        }
        
        public static int GetObjectIDFromPath(string path) {
            if (path.Contains(back_slash)) path = path.Split(back_slash_c).Last();
            return int.Parse(path.Split(dash_c)[1]);
        }
        public static string RemoveIDFromPath (string path) {
            string[] dirName = EditorUtils.DirectoryNameSplit(path);
            string directory = dirName[0];
            string name = dirName[1];
            
            //remove file extension
            name = name.Split(dot_c)[0]; 
            
            //take out id -234-
            string[] split = name.Split(dash_c);

            //remove id
            IList<string> noID = split.Slice(2, -1);
            name = string.Join(dash, noID);
            return directory + name;
        }



        public static int[] GenerateNewIDList (int count, HashSet<int> used_ids) {
            int[] result = new int[count];
            int generated = 0;
            int trying_id = 0;
            while (generated < count) {
                if (!used_ids.Contains(trying_id)) {
                    result[generated] = trying_id;
                    generated++;
                }
                trying_id++;
            }
            return result;
        }

        public static void GenerateNewIDs (string[] validPaths, string[] noIDs) {
            int l = noIDs.Length;
            int[] newIDs = GenerateNewIDList(l, new HashSet<int>().Generate(validPaths.Length, i => GetObjectIDFromPath(validPaths[i]) ));
            for (int i = 0; i < l; i++) {
                string path = noIDs[i];
                if (path.Contains(sIDKey)) {
                    Debug.LogWarning("asset was already assigned an id: " + path);
                    continue;
                }
                AssetDatabase.RenameAsset(path, sIDKey + newIDs[i] + "-" + EditorUtils.RemoveDirectory(path));
            }
            Debug.Log("Assets are now ready with unique IDs");
        }


        public static int[] GetAllUsedIDs (string packName) {
            List<int> used = new List<int>();
            Event[] allEvents = EditorUtils.GetAllAssetsOfType<Event>();
            int l = allEvents.Length;
            for (int i = 0; i < l; i++) {
                Event e = allEvents[i];
                if (packManager.FindPackByID( e.assetObjectPackID, out _ ).name == packName) {                    
                    int y = e.assetObjects.Length;
                    for (int z = 0; z < y; z++) {
                        int id = e.assetObjects[z].id;
                        if (!used.Contains(id)) used.Add(id);
                    }
                }
            }
            if (used.Count == 0) Debug.LogWarning("no IDs used for " + packName + " pack!");
            return used.ToArray();
        }
    }
}
