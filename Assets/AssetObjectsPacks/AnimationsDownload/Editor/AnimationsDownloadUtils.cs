using UnityEngine;
using UnityEditor;
namespace AssetObjectsPacks {
    class HumanoidSettingsOverride : AssetPostprocessor {
        /* CHANGE THIS IF YOU CHANGE THE MODEL ON THE DOWNLOADER SCRIPT FOR CHROME */
        const string character_model_name = "ybot";
        const string mixamo_download_check = character_model_name + "@-";
        const string mixamo_utils_dir = "AnimationsDownload/";
        const string animations_pack_name = "Animations";
        const string avatar_path = AssetObjectsEditor.asset_objects_packs_root_directory + mixamo_utils_dir + character_model_name + ".fbx";

        bool is_auto_downloaded {
            get {
                string path = assetPath;
                if (!path.StartsWith(AssetObjectsEditor.GetAssetObjectsDirectory(animations_pack_name))) 
                    return false;
                if (!path.Contains(mixamo_download_check)) 
                    return false;
                return true;
            }
        }
        GameObject avatar_model_gameobject { get { return AssetDatabase.LoadAssetAtPath<GameObject>(avatar_path); } }
        Animator animator { get { return avatar_model_gameobject.GetComponent<Animator>(); } }
        Avatar source_avatar { get { return animator.avatar; } }
        /*
            when importing automatically use humanoid rigging and y bot avatar (if auto downloaded)
        */
        void OnPreprocessModel() {
            if (!is_auto_downloaded) return;
            ModelImporter importer = assetImporter as ModelImporter;
            importer.animationType = UnityEditor.ModelImporterAnimationType.Human;
            importer.sourceAvatar = source_avatar;
        }
        void OnPostprocessModel(GameObject g)
        {
            if (!is_auto_downloaded) return;
            string file_path = assetPath;
            string[] dir_name = EditorUtils.DirectoryNameSplit(file_path);
            //remove file extension
            string n = dir_name[1].Split('.')[0];
            string new_name = n.Replace(mixamo_download_check, AssetObjectsEditor.asset_object_key);
            AssetDatabase.RenameAsset(file_path, new_name);
            EditorUtility.SetDirty(g);
        }
    }
}












