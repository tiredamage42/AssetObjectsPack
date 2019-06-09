using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
namespace AssetObjectsPacks {
    public static class AssetObjectsEditor 
    {
        
       
        public const string sIDKey = "@ID-";  
        const string back_slash = "/", dash = "-";
        const char back_slash_c = '/', dash_c = '-', dot_c = '.';    

        public static string[] GetAllAssetObjectPaths (string directory, string file_extensions, bool include_dir) {    
            return EditorUtils.GetFilePathsInDirectory(directory, include_dir, file_extensions, sIDKey, true);
        }
        //public static string[] GetAllAssetObjectPathsWithoutIDs (string directory, string file_extensions) {
        //    return EditorUtils.GetFilePathsInDirectory(directory, true, file_extensions, sIDKey, false);
        //}
                
        public static string[] GetAllAssetObjectPaths (string directory, string file_extensions, bool include_dir, out Dictionary<int, int> id2file) {
            
            string[] paths = GetAllAssetObjectPaths(directory, file_extensions, include_dir);
            
            int l = paths.Length;
            id2file = new Dictionary<int, int>(l);
            for (int i = 0; i < l; i++) {
                id2file.Add(GetObjectIDFromPath(paths[i]), i);
            }
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


        static void GetAOIDs(EventState state, List<int> ids){//, int packID) {
            for (int i = 0; i < state.assetObjects.Length; i++) {
                // if (state.assetObjects[i].packID != packID)  continue;
                int id = state.assetObjects[i].id;
                if (!ids.Contains(id)) ids.Add(id);
            }
        }

        public static IEnumerable<int> GetAllUsedIDs (int packID) {
            List<int> used = new List<int>();
            IEnumerable<Event> allEvents = EditorUtils.GetAllAssetsOfType<Event>();
            
            foreach (var e in allEvents) {
                if (e.mainPackID == packID) {

                    for (int i = 0; i < e.allStates.Length; i++) {
                        GetAOIDs(e.allStates[i], used);//, packID);
                    }
                }
            }
            if (used.Count == 0) {
                Debug.LogWarning("no IDs used for " + PacksManager.ID2Name(packID) + " pack!");
            }
            return used;
        }


        // static void GetAOs(EventState state, List<int> ids, List<AssetObject> aos, int packID) {
        //     for (int i = 0; i < state.assetObjects.Length; i++) {
        //         if (state.assetObjects[i].packID != packID)  continue;
        //         int id = state.assetObjects[i].id;
        //         if (!ids.Contains(id)) {
        //             ids.Add(id);
        //             aos.Add(state.assetObjects[i]);
        //         }
        //     }
        // }

        // public static IEnumerable<AssetObject> GetAllUsedAssetObjects (int packID, out IEnumerable<int> ids) {
        //     List<AssetObject> used = new List<AssetObject>();
        //     // ids = new List<int>();
            
        //     IEnumerable<Event> allEvents = EditorUtils.GetAllAssetsOfType<Event>();
            
        //     foreach (var e in allEvents) {
        //         for (int i = 0; i < e.allStates.Length; i++) {
        //             GetAOs(e.allStates[i], ids, used, packID);
        //         }
        //     }
        //     if (used.Count == 0) {
        //         Debug.LogWarning("no IDs used for " + PacksManager.ID2Name(packID) + " pack!");
        //     }
        //     return used;
        // }
    }
}
