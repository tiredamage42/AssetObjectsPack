using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
namespace AssetObjectsPacks {
    public static class AssetObjectsEditor 
    {
        /*
        static PacksManager pm;
        public static PacksManager packManager {
            get {
                if (pm == null) pm = GetPackManager();
                return pm;
            }
        }
         */
        

       
        const string sIDKey = "@ID-";  
        const string back_slash = "/", dash = "-";
        const char back_slash_c = '/', dash_c = '-', dot_c = '.';    

        public static string[] GetAllAssetObjectPaths (string directory, string file_extensions, bool include_dir) {    
            return EditorUtils.GetFilePathsInDirectory(directory, include_dir, file_extensions, sIDKey, true);
        }
        public static string[] GetAllAssetObjectPathsWithoutIDs (string directory, string file_extensions) {
            return EditorUtils.GetFilePathsInDirectory(directory, true, file_extensions, sIDKey, false);
        }
        /*
        public static int GetAllAssetObjectPathsWithoutIDsCount (string directory, string file_extensions) {
            return EditorUtils.GetCountInDirectory(directory, file_extensions, sIDKey, false);
        }
         */

                
        public static string[] GetAllAssetObjectPaths (string directory, string file_extensions, bool include_dir, out Dictionary<int, int> id2file) {
            string[] paths = GetAllAssetObjectPaths(directory, file_extensions, include_dir);
            int l = paths.Length;
            id2file = new Dictionary<int, int>(l);
            for (int i = 0; i < l; i++) {
                id2file.Add(GetObjectIDFromPath(paths[i]), i);
            }
            return paths;
        }
/*
        public static Dictionary<int, Dictionary<int, string>> AllPacksID2FileMaps (bool include_dir) {
            Dictionary<int, Dictionary<int, string>> r = new Dictionary<int, Dictionary<int, string>>();
            PacksManager p = packManager;
            for (int i = 0; i < p.packs.Length; i++) {
                r.Add(p.packs[i].id, ID2File(p.packs[i].dir, p.packs[i].extensions, include_dir));
            } 
            return r;
        }
        public static Dictionary<int, int> ID2File (string directory, string file_extensions, bool include_dir) {
            string[] paths = GetAllAssetObjectPaths(directory, file_extensions, include_dir);
            int l = paths.Length;
            Dictionary<int, int> id2file = new Dictionary<int, int>(l);
            for (int i = 0; i < l; i++) 
                id2file.Add(GetObjectIDFromPath(paths[i]), i);
            return id2file;
        }
        
 */
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


        public static void UpdateIDsForPack (AssetObjectPack pack) {
            string[] pathsWithoutIDs = GetAllAssetObjectPathsWithoutIDs(pack.dir, pack.extensions);
            if (pathsWithoutIDs.Length == 0) {
                Debug.Log("All assets in " + pack.name + " objects directory have IDs");
                return;
            }
            GenerateNewIDs(GetAllAssetObjectPaths(pack.dir, pack.extensions, false), pathsWithoutIDs);
        }
        

        public static void GenerateNewIDs (string[] validPaths, string[] noIDs) {
            int l = noIDs.Length;
            int[] newIDs = GenerateNewIDList(l, validPaths.Length.Generate(i => GetObjectIDFromPath(validPaths[i])).ToHashSet() );
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

        static void GetAOIDs(EventState state, List<int> ids, int packID) {
            for (int i = 0; i < state.assetObjects.Length; i++) {
                if (state.assetObjects[i].packID != packID)  continue;
                int id = state.assetObjects[i].id;
                if (!ids.Contains(id)) ids.Add(id);
            }
            /*
            for (int i = 0; i < state.subStates.Length; i++) {
                GetAOIDs(state.subStates[i], ids, packID);
            }
             */
        }


        public static IEnumerable<int> GetAllUsedIDs (int packID) {
            List<int> used = new List<int>();
            IEnumerable<Event> allEvents = EditorUtils.GetAllAssetsOfType<Event>();
            
            foreach (var e in allEvents) {
                //GetAOIDs(e.baseState, used, packID);


                for (int i = 0; i < e.allStates.Length; i++) {
                    GetAOIDs(e.allStates[i], used, packID);
                }
            }
            if (used.Count == 0) {
                Debug.LogWarning("no IDs used for " + PacksManager.ID2Name(packID) + " pack!");
            }
            return used;
        }
    }
}
