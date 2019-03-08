using UnityEngine;
using System;
namespace AssetObjectsPacks {
    [Serializable] public class CustomParameter {
        public enum ParamType { BoolValue = 0, FloatValue = 1, IntValue = 2, StringValue = 3, };
        public string name;
        public ParamType paramType {
            get { return (ParamType) pType; }
            private set { pType = (int)value; }
        }
        [SerializeField] int pType;
        [SerializeField] bool BoolValue;
        [SerializeField] float FloatValue;
        [SerializeField] int IntValue;
        [SerializeField] string StringValue;

        Func<object> valueLink;

        public T GetValue<T>() {
            return (T)(valueLink != null ? valueLink() : GetValue());
        }
        object GetValue () {
            switch (paramType) {
                case ParamType.BoolValue: return BoolValue;
                case ParamType.FloatValue: return FloatValue;
                case ParamType.IntValue: return IntValue;
                case ParamType.StringValue: return StringValue;
            }
            return null;
        }

        public CustomParameter (string name, object value) {
            this.name = name;
            paramType = SType2PType(value.GetType());
            SetValue(value);
        }
        public CustomParameter (string name, Func<object> valueLink) {
            this.name = name;
            this.paramType = SType2PType(valueLink().GetType());
            this.valueLink = valueLink;
        }
        
        static ParamType SType2PType (Type sType) {
            if (sType == typeof(float)) return ParamType.FloatValue;
            else if (sType == typeof(int)) return ParamType.IntValue;
            else if (sType == typeof(bool)) return ParamType.BoolValue;
            else if (sType == typeof(string)) return ParamType.StringValue;
            return ParamType.BoolValue;
        }


        bool CheckCompatibleSet (ParamType trying) {
            if (paramType != trying) {
                Debug.LogWarning("Incompatible paramtype, trying to set " + trying.ToString() + " on a " + paramType);
                return true;
            }
            return false;
        }
        public void SetValue (object value) {   
            ParamType vType = SType2PType(value.GetType());
            if (CheckCompatibleSet(vType)) return;
            switch ( vType ) {
                case ParamType.IntValue: this.IntValue = (int)value; break;
                case ParamType.FloatValue: this.FloatValue = (float)value; break;
                case ParamType.BoolValue: this.BoolValue = (bool)value; break;
                case ParamType.StringValue: this.StringValue = (string)value; break;
            }
        }

        
    }
}
