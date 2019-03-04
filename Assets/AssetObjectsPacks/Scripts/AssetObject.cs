using System.Collections.Generic;
using UnityEngine;
namespace AssetObjectsPacks {
    [System.Serializable] public class AssetObject {
        #if UNITY_EDITOR
        public bool isCopy;
        #endif
        public bool solo, mute;
        public Object objRef;

        public int id;
        public CustomParameter[] parameters;
        Dictionary<string, CustomParameter> paramDict = new Dictionary<string, CustomParameter>();

        public CustomParameter this [string paramName] {
            get {
                int l = parameters.Length;
                if (paramDict.Count != l) {
                    paramDict.Clear();
                    for (int i = 0; i < l; i++) paramDict.Add(parameters[i].name, parameters[i]);
                }
                return paramDict[paramName];
            }
        }
    }
}

