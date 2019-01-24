



//using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
namespace AssetObjectsPacks {

    [System.Serializable] public class AssetObjectParamDef {
        public AssetObjectParam parameter;
        public string hint;
        public AssetObjectParamDef (AssetObjectParam parameter, string hint) {
            this.parameter = parameter;
            this.hint = hint;
        }
    }


    public static class AssetObjectsEditor 
    {
        

        public const string asset_object_key = "@ID-";  
        public const string asset_objects_packs_root_directory = "Assets/AssetObjectsPacks/";
        const string asset_objects_packs_packs_directory = asset_objects_packs_root_directory + "Packs/";
        const string packs_asset_objects_contents_dir_name = "AssetObjects/";
        const string tags_holder_file_name = "alltags.txt";
        const string editor_dir = "Editor/";
        const string back_slash = "/", dash = "-";
        const char back_slash_c = '/', dash_c = '-', dot_c = '.', comma_c = ',';    

        static string GetTagsPath(string pack_name) {
            string pack_dir = GetPackRootDirectory(pack_name) + editor_dir;
            pack_dir = EditorUtils.MakeDirectoryIfNone(pack_dir);
            return pack_dir + tags_holder_file_name;
        }

        public static List<string> LoadAllTags(string pack_name) {
            string tags_path = GetTagsPath(pack_name);
            TextAsset text = AssetDatabase.LoadAssetAtPath<TextAsset>(tags_path);
            if (text == null) {
                text = new TextAsset();
                AssetDatabase.CreateAsset(text, tags_path);
            }

            string content = text.text;
            List<string> all_tags = new List<string>();
            if (content.Length != 0) {
                all_tags = content.Split(comma_c).ToList();
            }
            return all_tags;
        }
     
     
        public static void SaveAllTags(string pack_name, List<string> tags) {
            string tags_path = GetTagsPath(pack_name);
            
            StreamWriter writer = new StreamWriter(tags_path, false);
            writer.Write(string.Join(",", tags));
            writer.Close();
            //Re-import the file to update the reference in the editor
            AssetDatabase.ImportAsset(tags_path); 

        }





        public static string GetAssetObjectPackName(string file_path) {
            if (!file_path.StartsWith("Assets/")) {
                Debug.LogError("Needs full file path Couldnt find pack name for :" + file_path);
                return null;
            }
            return file_path.Replace(asset_objects_packs_packs_directory, "").Split('/')[0];
        }

        public static string GetPackRootDirectory(string pack_name) {
            return asset_objects_packs_packs_directory + pack_name + "/";
        }
        public static string GetAssetObjectsDirectory(string pack_name) {
            return GetPackRootDirectory(pack_name) + packs_asset_objects_contents_dir_name;
        }
        public static string[] GetAllAssetObjectPaths (string pack_name, string file_extension, bool include_dir) {    
            return EditorUtils.GetFilePathsInDirectory(GetAssetObjectsDirectory(pack_name), include_dir, file_extension, asset_object_key, true);
        }
        public static string[] GetAllAssetObjectPaths (string pack_name, string file_extension, bool include_dir, out string[] without_ids) {    
            string pack_dir = GetAssetObjectsDirectory(pack_name);
            without_ids = EditorUtils.GetFilePathsInDirectory(pack_dir, true, file_extension, asset_object_key, false);
            return EditorUtils.GetFilePathsInDirectory(pack_dir, include_dir, file_extension, asset_object_key, true);
        }
        public static string[] GetAllAssetObjectPaths (string pack_name, string file_extension, bool include_dir, out string[] without_ids, out Dictionary<int, string> id2file) {
            string[] f_paths = GetAllAssetObjectPaths(pack_name, file_extension, include_dir, out without_ids);
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
            string dir = StringUtils.empty;
            string n = path;
            if (path.Contains(back_slash)) {
                string[] dir_and_name = EditorUtils.DirectoryNameSplit(path);
                dir = dir_and_name[0];
                n = dir_and_name[1];
            }
            //remove file extension
            n = n.Split(dot_c)[0]; 
            //take out id -234-
            n = string.Join(dash, n.Split(dash_c).Slice(2, -1));
            return dir + n;
        }



        static int[] GetExistingIDs (string[] all_valid_paths) {
            int l = all_valid_paths.Length;
            int[] all_ids = new int[l];
            for (int i = 0; i < l; i++) {
                all_ids[i] = GetObjectIDFromPath(all_valid_paths[i]);
            }
            return all_ids;
        }

        static bool Contains(this int[] a, int e) {
            int l = a.Length;
            for (int i = 0; i < l; i++) {
                if (a[i] == e) {
                    return true;
                }
            }
            return false;
        }
        static int[] GenerateNewIDList (int count, int[] used_ids) {
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



                Debug.Log(asset_path);

                string orig_name = asset_path;
                if (orig_name.Contains("/")) {
                    orig_name = EditorUtils.RemoveDirectory(asset_path);
                }
                
                
                
                if (orig_name.Contains(asset_object_key)) {
                    Debug.LogError("asset was already assigned an id: " + orig_name + " (to fix, just delete the '@ID-#-' section)");
                    return;
                }

                string new_name = asset_object_key + new_ids[i] + "-" + orig_name;
                //Debug.Log(new_name);
                
                AssetDatabase.RenameAsset(asset_path, new_name);
            }
            Debug.Log("Animations are now ready to be added to the corpus directory");
        }
    }
}
