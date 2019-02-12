using System.Collections.Generic;
using UnityEngine;
namespace AssetObjectsPacks {
    [System.Serializable] public class AssetObjectParam {

        #if UNITY_EDITOR 
        public const string name_field = "name", param_type_field = "paramType";
        public const string bool_val_field = "boolValue", float_val_field = "floatValue", int_val_field = "intValue";
        #endif
        
        public enum ParamType { Bool, Float, Int };
        public string name;
        public ParamType paramType;
        public bool boolValue;
        public float floatValue;
        public int intValue;
        public AssetObjectParam (string name, float default_value) {
            paramType = ParamType.Float;
            this.name = name;
            this.floatValue = default_value;
        }
        public AssetObjectParam (string name, bool default_value) {
            paramType = ParamType.Bool;
            this.name = name;
            this.boolValue = default_value;
        }
        public AssetObjectParam (string name, int default_value) {
            paramType = ParamType.Int;
            this.name = name;
            this.intValue = default_value;
        }

        bool CheckCompatibleSet (ParamType type_trying) {
            if (paramType != type_trying) {
                Debug.LogWarning("Incompatible paramtype, trying to set " + type_trying.ToString() + " on a " + paramType);
                return true;
            }
            return false;
        }
        public void SetValue (float default_value) {
            if (CheckCompatibleSet(ParamType.Float)) return;
            this.floatValue = default_value;
        }
        public void SetValue (bool default_value) {
            if (CheckCompatibleSet(ParamType.Bool)) return;
            this.boolValue = default_value;
        }
        public void SetValue (int default_value) {
            if (CheckCompatibleSet(ParamType.Int)) return;
            this.intValue = default_value;
        }


        public bool MatchesParameter (AssetObjectParam other_parameter) {
            if (other_parameter.name != name) {
                Debug.LogWarning("Name Mismatch! " + other_parameter.name + " / " + name);
                return false;
            }
            if (other_parameter.paramType != paramType) {
                return false;
            }
            switch (paramType) {
                case ParamType.Bool:
                    if (other_parameter.boolValue != boolValue) {
                        return false;
                    }
                    break;
                case ParamType.Int:
                if (other_parameter.intValue != intValue) {
                        return false;
                    }
                    
                    break;
                case ParamType.Float:
                if (other_parameter.floatValue != floatValue) {
                        return false;
                    }
                    
                    break;
                
            }

            return true;
        }
    }


    [System.Serializable] public class AssetObject {
        #if UNITY_EDITOR 
        public static readonly AssetObjectParamDef[] base_def_params = new AssetObjectParamDef[] {
            new AssetObjectParamDef(new AssetObjectParam("Duration", -1.0f), "Nagative values for object duration"),
            new AssetObjectParamDef(new AssetObjectParam("Looped", false), ""),
        };
        
        public const string obj_ref_field = "object_reference", tags_field = "tags";
        public const string id_field = "id", params_field = "parameters";

        public const string conditionChecksField = "conditionChecks";
        public const string paramsToMatchField = "paramsToMatch";
        #endif

        


        [System.Serializable] public class ConditionCheck {
            //ands
            public AssetObjectParam[] paramsToMatch;

            AssetObjectParam FindParamByName (string name, AssetObjectParam[] paramsCheck) {
                int l = paramsCheck.Length;
                for (int i = 0; i < l; i++) {
                    if (name == paramsCheck[i].name) {
                        return paramsCheck[i];
                    }
                }
                Debug.LogWarning("Params List doesnt contain: " + name);
                        
                return null;
            }

            public bool ConditionMet (AssetObjectParam[] paramsCheck) {
                int l = paramsToMatch.Length;
                if (l == 0) return true;
                
                bool all_params_matched = true;

                for (int i = 0; i < l; i++) {
                    string name = paramsToMatch[i].name;
                    AssetObjectParam check = FindParamByName(name, paramsCheck);

                    if (check == null || !paramsToMatch[i].MatchesParameter(check)) {
                        all_params_matched = false;
                        break;
                    }
                }
                return all_params_matched;
            }
        }

        //ors
        public ConditionCheck[] conditionChecks;


        public bool PassesConditionCheck (AssetObjectParam[] paramsCheck) {

            if (conditionChecks.Length == 0) return true;

            for (int i = 0; i < conditionChecks.Length; i++ ) {

                if (conditionChecks[i].ConditionMet(paramsCheck)) {
                    return true;
                }
            }

            return false;

        }







        

        public Object object_reference;
        public List<string> tags = new List<string>();
        public int id;
        public List<AssetObjectParam> parameters = new List<AssetObjectParam>();
        Dictionary<string, AssetObjectParam> param_dict = new Dictionary<string, AssetObjectParam>();

        public AssetObjectParam this [string paramName] {
            get {
                if (param_dict.Count != parameters.Count) {
                    param_dict.Clear();
                    for (int i = 0; i < parameters.Count; i++) {
                        param_dict.Add(parameters[i].name, parameters[i]);
                    }
                }
                return param_dict[paramName];
            }
        }
    }
}

