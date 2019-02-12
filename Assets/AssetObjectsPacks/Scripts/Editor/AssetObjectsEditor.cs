using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
namespace AssetObjectsPacks {
    public static class AssetObjectsEditor 
    {
        
        public const string asset_object_key = "@ID-";  
        const string back_slash = "/", dash = "-";
        const char back_slash_c = '/', dash_c = '-', dot_c = '.', comma_c = ',';    

        public static string[] GetAllAssetObjectPaths (string directory, string file_extensions, bool include_dir) {    
            return EditorUtils.GetFilePathsInDirectory(directory, include_dir, file_extensions, asset_object_key, true);
        }
        public static string[] GetAllAssetObjectPaths (string directory, string file_extensions, bool include_dir, out string[] without_ids) {    
            without_ids = EditorUtils.GetFilePathsInDirectory(directory, true, file_extensions, asset_object_key, false);
            string[] paths = EditorUtils.GetFilePathsInDirectory(directory, include_dir, file_extensions, asset_object_key, true);
            return paths;
        }
        
        public static string[] GetAllAssetObjectPaths (string directory, string file_extensions, bool include_dir, out string[] without_ids, out Dictionary<int, string> id2file) {
            string[] f_paths = GetAllAssetObjectPaths(directory, file_extensions, include_dir, out without_ids);
            
            int l = f_paths.Length;
            id2file = new Dictionary<int, string>(l);
            for (int i = 0; i < l; i++) id2file.Add(GetObjectIDFromPath(f_paths[i]), f_paths[i]);
            return f_paths;
        }
        
        public static int GetObjectIDFromPath(string path) {
            if (path.Contains(back_slash)) path = path.Split(back_slash_c).Last();
            return int.Parse(path.Split(dash_c)[1]);
        }
        public static string RemoveIDFromPath (string path) {
            string directory = StringUtils.empty;
            string file_name = path;



            if (path.Contains(back_slash)) {

                string[] dir_and_name = EditorUtils.DirectoryNameSplit(path);
                directory = dir_and_name[0];
                file_name = dir_and_name[1];

                
                
            }
            //remove file extension
            file_name = file_name.Split(dot_c)[0]; 
            
            //take out id -234-
            string[] split_by_dash = file_name.Split(dash_c);


            IList<string> sans_id = split_by_dash.Slice(2, -1);


            file_name = string.Join(dash, sans_id);


            return directory + file_name;
        }



        static int[] GetExistingIDs (string[] all_valid_paths) {
            int l = all_valid_paths.Length;
            int[] all_ids = new int[l];
            for (int i = 0; i < l; i++) {
                all_ids[i] = GetObjectIDFromPath(all_valid_paths[i]);
            }
            return all_ids;
        }
        public static int[] GenerateNewIDList (int count, int[] used_ids) {
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

        //call from editor on enable if paths without id's length is not 0
        public static void GenerateNewIDs (string[] all_valid_paths, string[] paths_without_ids) {

            int l = paths_without_ids.Length;
            int[] new_ids = GenerateNewIDList(l, GetExistingIDs (all_valid_paths));
            
            for (int i = 0; i < l; i++) {
                string asset_path = paths_without_ids[i];

                string orig_name = asset_path;
                if (orig_name.Contains("/")) {
                    orig_name = EditorUtils.RemoveDirectory(asset_path);
                }
                
                if (orig_name.Contains(asset_object_key)) {
                    Debug.LogError("asset was already assigned an id: " + orig_name + " (to fix, just delete the '@ID-#-' section)");
                    return;
                }
                string new_name = asset_object_key + new_ids[i] + "-" + orig_name;                
                AssetDatabase.RenameAsset(asset_path, new_name);
            }
            Debug.Log("Assets are now ready with unique IDs");
        }





        public static SerializedProperty GetRelevantParamProperty(this SerializedProperty parameter) {
            switch((AssetObjectParam.ParamType)parameter.FindPropertyRelative(AssetObjectParam.param_type_field).enumValueIndex) {
                case AssetObjectParam.ParamType.Bool: return parameter.FindPropertyRelative(AssetObjectParam.bool_val_field);
                case AssetObjectParam.ParamType.Float: return parameter.FindPropertyRelative(AssetObjectParam.float_val_field);
                case AssetObjectParam.ParamType.Int: return parameter.FindPropertyRelative(AssetObjectParam.int_val_field);
            }
            return null;
                
        }
    }
}
