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
                if (!path.Contains(mixamo_download_check)) 
                    return false;
                if (!path.StartsWith(AssetObjectsEditor.GetAssetObjectsDirectory(animations_pack_name))) 
                    return false;
                return true;
            }
        }
        static GameObject avatar_model_gameobject { get { return AssetDatabase.LoadAssetAtPath<GameObject>(avatar_path); } }
        static Animator _anim;
        static Animator animator { 
            get { 
                if (_anim == null) _anim = avatar_model_gameobject.GetComponent<Animator>();
                return _anim;
            } 
        }


        Avatar source_avatar { get { return animator.avatar; } }
        /*
            when importing automatically use humanoid rigging and y bot avatar (if auto downloaded)
        */
        void OnPreprocessModel() {
            if (!is_auto_downloaded) return;
            ModelImporter importer = assetImporter as ModelImporter;
            if (importer.animationType != UnityEditor.ModelImporterAnimationType.Human) {
                importer.animationType = UnityEditor.ModelImporterAnimationType.Human;
                importer.sourceAvatar = source_avatar;
            }
        }
    }
}