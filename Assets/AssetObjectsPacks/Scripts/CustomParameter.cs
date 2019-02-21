using UnityEngine;
namespace AssetObjectsPacks {
    [System.Serializable] public class CustomParameter {

        public enum ParamType { 
            BoolValue = 0, 
            FloatValue = 1, 
            IntValue = 2, 
        };
        
        public string name, hint;
        public ParamType paramType;
        public bool BoolValue;
        public float FloatValue;
        public int IntValue;

        public CustomParameter (string name, float default_value, string hint) {
            paramType = ParamType.FloatValue;
            this.name = name;
            this.hint = hint;
            this.FloatValue = default_value;
        }
        public CustomParameter (string name, bool default_value, string hint) {
            paramType = ParamType.BoolValue;
            this.name = name;
            this.hint = hint;
            this.BoolValue = default_value;
        }
        public CustomParameter (string name, int default_value, string hint) {
            paramType = ParamType.IntValue;
            this.name = name;
            this.hint = hint;
            this.IntValue = default_value;
        }

        bool CheckCompatibleSet (ParamType type_trying) {
            if (paramType != type_trying) {
                Debug.LogWarning("Incompatible paramtype, trying to set " + type_trying.ToString() + " on a " + paramType);
                return true;
            }
            return false;
        }


        public void SetValue (float default_value) {
            if (CheckCompatibleSet(ParamType.FloatValue)) return;
            this.FloatValue = default_value;
        }
        public void SetValue (bool default_value) {
            if (CheckCompatibleSet(ParamType.BoolValue)) return;
            this.BoolValue = default_value;
        }
        public void SetValue (int default_value) {
            if (CheckCompatibleSet(ParamType.IntValue)) return;
            this.IntValue = default_value;
        }

        public bool MatchesParameter (CustomParameter other_parameter) {
            if (other_parameter.name != name) {
                Debug.LogWarning("Name Mismatch! " + other_parameter.name + " / " + name);
                return false;
            }
            if (other_parameter.paramType != paramType) return false;
            
            switch (paramType) {
                case ParamType.BoolValue:
                    if (other_parameter.BoolValue != BoolValue) return false;
                    break;
                case ParamType.IntValue:
                    if (other_parameter.IntValue != IntValue) return false;
                    break;
                case ParamType.FloatValue:
                    if (other_parameter.FloatValue != FloatValue) return false;
                    break;
            }
            return true;
        }
    }

}
