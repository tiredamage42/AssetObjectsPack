using UnityEngine;
using System;
namespace AssetObjectsPacks {
    [Serializable] public class CustomParameter {
        public enum ParamType { 
            BoolValue = 0, FloatValue = 1, IntValue = 2, StringValue = 3, 
            ColorValue = 4, GradientValue = 5, AnimationCurveValue = 6, 
            Vector2Value = 7, Vector3Value = 8,
        };
        
        static readonly Type[] typesCheck = new Type[] {
            typeof(bool), typeof(float), typeof(int), typeof(string),
            typeof(Color32), typeof(Gradient), typeof(AnimationCurve),
        };

        void BuildAllValues () {
            _allValues = new Func<object>[] {
                () => BoolValue, () => FloatValue, () => IntValue, () => StringValue, 
                () => ColorValue, () => GradientValue, () => AnimationCurveValue,
                () => Vector2Value, () => Vector3Value,
            };
            _allSetVals = new Action<object>[] {
                (o) => BoolValue = (bool)o, 
                (o) => FloatValue = (float)o, 
                (o) => IntValue = (int)o, 
                (o) => StringValue = (string)o, 
                (o) => ColorValue = (Color32)o, 
                (o) => GradientValue = (Gradient)o, 
                (o) => AnimationCurveValue = (AnimationCurve)o,
                (o) => Vector2Value = (Vector2)o,
                (o) => Vector3Value = (Vector3)o,
            };
        }

        Func<object>[] _allValues;

        Func<object>[] allValues {
            get {
                if (_allValues == null) {
                    BuildAllValues();
                }
                return _allValues;
            }
        }
        Action<object>[] _allSetVals;

        Action<object>[] allSetVals {
            get {
                if (_allSetVals == null) {
                    BuildAllValues();
                }
                return _allSetVals;
            }
        }

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
        [SerializeField] Color32 ColorValue;
        [SerializeField] Gradient GradientValue;
        [SerializeField] AnimationCurve AnimationCurveValue;
        [SerializeField] Vector2 Vector2Value;
        [SerializeField] Vector3 Vector3Value;

        
        
        Func<object> valueLink;
        object GetValue () {
            return allValues[(int)paramType]();
        }

        public T GetValue<T>() {
            return (T)(valueLink != null ? valueLink() : GetValue());
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
            allSetVals[(int)vType]( value );
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
            for (int i = 0; i < typesCheck.Length; i++) {
                if (sType == typesCheck[i]) return (ParamType)i;
            }
            return ParamType.BoolValue;
        }
    }
}
