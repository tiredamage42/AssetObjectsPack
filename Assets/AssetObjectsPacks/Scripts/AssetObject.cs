using System.Collections.Generic;
using UnityEngine;
namespace AssetObjectsPacks {
    [System.Serializable] public class AssetObject {
        #if UNITY_EDITOR 

        public const string obj_ref_field = "objRef";
        public const string id_field = "id", params_field = "parameters";
        public const string conditionChecksField = "conditionChecks";
        public const string showConditionsField = "showConditions", showParamsField = "showParams";
        public const string paramsToMatchField = "paramsToMatch";
        //for custom editor
        public bool showConditions, showParams;
        #endif
        
        [System.Serializable] public class ConditionCheck {
            //ands
            public CustomParameter[] paramsToMatch;
            CustomParameter FindParamByName (string name, CustomParameter[] paramsCheck) {
                int l = paramsCheck.Length;
                for (int i = 0; i < l; i++) {
                    if (name == paramsCheck[i].name) return paramsCheck[i];
                }
                Debug.LogWarning("Params List doesnt contain: " + name);
                return null;
            }
            public bool ConditionMet (CustomParameter[] paramsCheck) {
                int l = paramsToMatch.Length;
                if (l == 0) return true;
                bool all_params_matched = true;
                for (int i = 0; i < l; i++) {
                    string name = paramsToMatch[i].name;
                    CustomParameter check = FindParamByName(name, paramsCheck);
                    //Debug.Log("checking parameter " + name);
                    if (check == null || !paramsToMatch[i].MatchesParameter(check)) {
                        //Debug.Log("parameter failed" + name);
                        all_params_matched = false;
                        break;
                    }
                }
                return all_params_matched;
            }
        }

        //ors
        public ConditionCheck[] conditionChecks;
        public bool PassesConditionCheck (CustomParameter[] paramsCheck) {
            if (conditionChecks.Length == 0) {
                //Debug.Log("[" + id + "] has no conditions");
                return true;
            }
            for (int i = 0; i < conditionChecks.Length; i++ ) {
                if (conditionChecks[i].ConditionMet(paramsCheck)) {
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

