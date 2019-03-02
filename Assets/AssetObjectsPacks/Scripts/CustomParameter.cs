using UnityEngine;
namespace AssetObjectsPacks {
    [System.Serializable] public class CustomParameter {
        public enum ParamType { BoolValue = 0, FloatValue = 1, IntValue = 2, StringValue = 3, };
        public string name;
        public ParamType paramType {
            get { return (ParamType) pType; }
            private set { pType = (int)value; }
        }
        public int pType;
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
            string pName = paramStringSplit[0];
            string pVals = paramStringSplit[1];

            if (pName != name) {
                Debug.LogWarning("Name Mismatch! " + pName + " / " + name);
                return false;
            }
            switch ( paramType ) {
                case ParamType.IntValue:
                
                    if (pVals.StartsWith("<")) return IntValue < int.Parse(pVals.Substring(1));
                    else if (pVals.StartsWith(">")) return IntValue > int.Parse(pVals.Substring(1));
                    return IntValue == int.Parse(pVals);

                case ParamType.FloatValue:

                    if (pVals.StartsWith("<")) return FloatValue < float.Parse(pVals.Substring(1));
                    else if (pVals.StartsWith(">")) return FloatValue > float.Parse(pVals.Substring(1));                    
                    return FloatValue == float.Parse(pVals);

                case ParamType.BoolValue:
                    return bool.Parse(pVals) == BoolValue;

                case ParamType.StringValue:
                    return pVals == StringValue;
            }
            return true; 
        }
/*
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
 */
    }
}
