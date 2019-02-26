using UnityEngine;
namespace AssetObjectsPacks {
    [System.Serializable] public class CustomParameter {

        public enum ParamType { BoolValue = 0, FloatValue = 1, IntValue = 2, StringValue = 3, };
        
        public string name;
        public ParamType paramType;
        
        public bool BoolValue;
        public float FloatValue;
        public int IntValue;
        public string StringValue;

        public CustomParameter (string name, float value) {
            paramType = ParamType.FloatValue;
            this.name = name;
            this.FloatValue = value;
        }
        public CustomParameter (string name, bool value) {
            paramType = ParamType.BoolValue;
            this.name = name;
            this.BoolValue = value;
        }
        public CustomParameter (string name, int value) {
            paramType = ParamType.IntValue;
            this.name = name;
            this.IntValue = value;
        }
        public CustomParameter (string name, string value) {
            paramType = ParamType.StringValue;
            this.name = name;
            this.StringValue = value;
        }


        bool CheckCompatibleSet (ParamType trying) {
            if (paramType != trying) {
                Debug.LogWarning("Incompatible paramtype, trying to set " + trying.ToString() + " on a " + paramType);
                return true;
            }
            return false;
        }

        public void SetValue (float value) {
            if (CheckCompatibleSet(ParamType.FloatValue)) return;
            this.FloatValue = value;
        }
        public void SetValue (bool value) {
            if (CheckCompatibleSet(ParamType.BoolValue)) return;
            this.BoolValue = value;
        }
        public void SetValue (int value) {
            if (CheckCompatibleSet(ParamType.IntValue)) return;
            this.IntValue = value;
        }
        public void SetValue (string value) {
            if (CheckCompatibleSet(ParamType.StringValue)) return;
            this.StringValue = value;
        }

        public bool MatchesParameter(string[] paramStringSplit) {
            if (paramStringSplit[1] != name) {
                Debug.LogWarning("Name Mismatch! " + paramStringSplit[1] + " / " + name);
                return false;
            }
            
            
            switch ( paramStringSplit[0].ToLower() ) {
                case "b":
                return bool.Parse(paramStringSplit[2]) == BoolValue;
                case "i":
                return int.Parse(paramStringSplit[2]) == IntValue;
                case "f":
                return float.Parse(paramStringSplit[2]) == FloatValue;
                case "s":
                return paramStringSplit[2] == StringValue;
            }
            return true;
                
                
        }

        public bool MatchesParameter (CustomParameter other_parameter) {
            if (other_parameter.name != name) {
                Debug.LogWarning("Name Mismatch! " + other_parameter.name + " / " + name);
                return false;
            }
            if (other_parameter.paramType != paramType) return false;
            switch (paramType) {
                case ParamType.BoolValue: return other_parameter.BoolValue == BoolValue;
                case ParamType.IntValue: return other_parameter.IntValue == IntValue;
                case ParamType.FloatValue: return other_parameter.FloatValue == FloatValue;
                case ParamType.StringValue: return other_parameter.StringValue == StringValue;
            }
            return true;
        }
    }
}
