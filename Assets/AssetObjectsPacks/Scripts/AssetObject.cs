using System.Collections.Generic;
using UnityEngine;
namespace AssetObjectsPacks {
    [System.Serializable] public class AssetObject {
        
        [System.Serializable] public class Condition {
            //ands
            public CustomParameter[] parameters;
            CustomParameter FindParamByName (string name, CustomParameter[] paramsCheck) {
                int l = paramsCheck.Length;
                for (int i = 0; i < l; i++) {
                    if (name == paramsCheck[i].name) return paramsCheck[i];
                }
                Debug.LogWarning("Params List doesnt contain: " + name);
                return null;
            }
            public bool ConditionMet (CustomParameter[] paramsCheck) {
                int l = parameters.Length;
                if (l == 0) return true;
                bool all_params_matched = true;
                for (int i = 0; i < l; i++) {
                    string name = parameters[i].name;
                    CustomParameter check = FindParamByName(name, paramsCheck);
                    //Debug.Log("checking parameter " + name);
                    if (check == null || !parameters[i].MatchesParameter(check)) {
                        //Debug.Log("parameter failed" + name);
                        all_params_matched = false;
                        break;
                    }
                }
                return all_params_matched;
            }
        }

        //ors
        public Condition[] conditions;
        public bool PassesConditionCheck (CustomParameter[] paramsCheck) {
            if (conditions.Length == 0) {
                //Debug.Log("[" + id + "] has no conditions");
                return true;
            }
            for (int i = 0; i < conditions.Length; i++ ) {
                if (conditions[i].ConditionMet(paramsCheck)) {
                    //Debug.Log("[" + id + "] condition " + i +  " met");   
                    return true;
                }
            }
            return false;
        }


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

