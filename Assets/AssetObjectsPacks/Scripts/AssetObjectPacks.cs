using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AssetObjectsPacks {


    [System.Serializable] public class AssetObjectParamDef {
        public AssetObjectParam parameter;
        #if UNITY_EDITOR
        public string hint;
        #endif

        public AssetObjectParamDef (AssetObjectParam parameter, string hint) {
            this.parameter = parameter;
            #if UNITY_EDITOR
            this.hint = hint;
            #endif
        }
    }
    [System.Serializable] public class AssetObjectPack {
        #if UNITY_EDITOR
        public AssetObjectParamDef[] defaultParams {
            get {
                int b_l = AssetObject.base_def_params.Length;
                int d_l = defParams.Length;
                AssetObjectParamDef[] defs = new AssetObjectParamDef[b_l + d_l];
                int u = 0;
                for (int i = 0; i < b_l; i++) {
                    defs[u] = AssetObject.base_def_params[i];
                    u++;
                }
                for (int i = 0; i < d_l; i++) {
                    defs[u] = defParams[i];
                    u++;
                }
                return defs;
            }
        }
        public AssetObjectParamDef[] defParams;
        public string objectsDirectory, fileExtensions, assetType;
        public string[] allTags;
        public const string allTagsField = "allTags", nameField = "name";
        public const string assetTypeField = "assetType";
        public const string objectsDirectoryField = "objectsDirectory", fileExtensionsField = "fileExtensions";
        #endif

        public string name;
        public int id;
    }


    [CreateAssetMenu()]
    public class AssetObjectPacks : ScriptableObject
    {
        #if UNITY_EDITOR
        public const string packsField = "packs";
        #endif
        
        public List<AssetObjectPack> packs = new List<AssetObjectPack>();
        public AssetObjectPack FindPackByID (int id) {
            int c = packs.Count;
            for (int i = 0; i < c; i++) {
                if (packs[i].id == id) return packs[i];
            }
            return null;
        }
        
    }
}

