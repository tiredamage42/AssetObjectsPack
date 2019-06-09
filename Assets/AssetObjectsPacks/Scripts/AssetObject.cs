using System.Collections.Generic;
using UnityEngine;
namespace AssetObjectsPacks {
    [System.Serializable] public class AssetObject {
        #if UNITY_EDITOR
        public bool isCopy, isNew;
        public string name;
        #endif
        public bool solo, mute;
        public Object objRef;
        public string messageBlock, conditionBlock;
        public int id;//, packID;
        public CustomParameter[] parameters;
        Dictionary<string, int> paramDict = new Dictionary<string, int>();
        public CustomParameter this [string paramName] {
            get {
                int l = parameters.Length;
                if (paramDict.Count != l) {
                    paramDict.Clear();
                    for (int i = 0; i < l; i++) paramDict[parameters[i].name] = i;
                }
                return parameters[paramDict[paramName]];
            }
        }
    }
}

