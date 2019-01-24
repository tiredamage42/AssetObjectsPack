using System.Collections.Generic;
using UnityEngine;
namespace AssetObjectsPacks {
    [System.Serializable] public class AssetObjectParam {

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
            Debug.Log("float!");
        }
        public AssetObjectParam (string name, bool default_value) {
            this.name = name;
            this.boolValue = default_value;
            paramType = ParamType.Bool;
            Debug.Log("bool!");
        }
        public AssetObjectParam (string name, int default_value) {
            this.name = name;
            this.intValue = default_value;
            paramType = ParamType.Int;
            Debug.Log("integer!");
        }
    }


    [System.Serializable] public class AssetObject {
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

