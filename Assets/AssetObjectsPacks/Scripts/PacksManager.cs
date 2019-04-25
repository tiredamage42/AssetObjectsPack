using UnityEngine;
using System.Collections.Generic;

namespace AssetObjectsPacks {
    [System.Serializable] public class AssetObjectPack {
        #if UNITY_EDITOR
        public string dir, extensions, assetType;
        public CustomParameter[] defaultParameters;
        public bool isCustom;
        #endif

        public string name;
        public int id;
    }

    [CreateAssetMenu(fileName = "Packs Manager", menuName = "Asset Objects Packs/Packs Manager", order = 1)]
    public class PacksManager : ScriptableObject
    {
        public static PacksManager instance { get { return AssetObjectsManager.instance.packs; } }

        public static int Name2ID (string name, bool debug=true) {
            if (!Application.isPlaying) {
                PacksManager pm = instance;
                for (int i = 0; i < pm.packs.Length; i++) {
                    if (pm.packs[i].name == name) return pm.packs[i].id;
                }
            }
            else {
                InitializeDictionaries();
                if (name2ID.ContainsKey(name)) return name2ID[name];    
            }
            if (debug) Debug.LogError("pack manager does not contain pack: " + name);
            return -1;
        }
        public static string ID2Name (int id, bool debug=true) {
            if (!Application.isPlaying) {
                PacksManager pm = instance;
                for (int i = 0; i < pm.packs.Length; i++) {
                    if (pm.packs[i].id == id) return pm.packs[i].name;
                }   
            }
            else {
                InitializeDictionaries();
                if (id2name.ContainsKey(id)) return id2name[id];    
            }
            if (debug) Debug.LogError("pack manager does not contain pack id: " + id);
            return null;
        }
        static void InitializeDictionaries () {
            if (id2name == null) {
                PacksManager inst = instance;
                int l = inst.packs.Length;
                id2name = new Dictionary<int, string>(l);
                name2ID = new Dictionary<string, int>(l);       
                for (int i = 0; i < l; i++) {
                    id2name.Add(inst.packs[i].id, inst.packs[i].name);
                    name2ID.Add(inst.packs[i].name, inst.packs[i].id);
                }
            }
        }
        static Dictionary<int, string> id2name = null;
        static Dictionary<string, int> name2ID = null;
        public AssetObjectPack[] packs;   
    }
}