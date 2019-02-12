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
            this.name = name;
            this.floatValue = default_value;
            paramType = ParamType.Float;
        }
        public AssetObjectParam (string name, bool default_value) {
            this.name = name;
            this.boolValue = default_value;
            paramType = ParamType.Bool;
        }
        public AssetObjectParam (string name, int default_value) {
            this.name = name;
            this.intValue = default_value;
            paramType = ParamType.Int;
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
        
        #endif
        

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

