using UnityEngine;
namespace AssetObjectsPacks {
    [System.Serializable] public class AssetObjectPack {
        #if UNITY_EDITOR
        public string objectsDirectory, fileExtensions, assetType;
        public CustomParameter[] defaultParameters;
        #endif

        public string name;
        public int id;
    }

    [CreateAssetMenu(fileName = "Packs Manager", menuName = "Asset Objects Packs/Packs Manager", order = 1)]
    public class PacksManager : ScriptableObject
    {
        public AssetObjectPack[] packs;
        public AssetObjectPack FindPackByID (int id, out int index) {
            index = -1;
            int c = packs.Length;
            for (int i = 0; i < c; i++) {
                if (packs[i].id == id) {
                    index = i;
                    return packs[i];
                }
            }
            return null;
        }   
    }
}

