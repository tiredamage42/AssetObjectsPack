using UnityEngine;

namespace AssetObjectsPacks {
    [System.Serializable] public class AssetObjectParamDef {
        public AssetObjectParam parameter;
        
        #if UNITY_EDITOR
        public string hint;
        public AssetObjectParamDef (AssetObjectParam parameter, string hint) {
            this.parameter = parameter;
            this.hint = hint;
        }
        #endif
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
        #endif

        public string name;
        public int id;
    }


    [CreateAssetMenu()]
    public class AssetObjectPacks : ScriptableObject
    {
        public AssetObjectPack[] packs;
        public AssetObjectPack FindPackByID (int id) {
            int c = packs.Length;
            for (int i = 0; i < c; i++) {
                if (packs[i].id == id) return packs[i];
            }
            return null;
        }   
    }
}

