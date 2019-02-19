using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
namespace AssetObjectsPacks {
    public class EditorProp {

        public SerializedProperty prop;

        public int intValue { get { return prop.intValue; } }
        public float floatValue { get { return prop.floatValue; } }
        public bool boolValue { get { return prop.boolValue; } }
        public string stringValue { get { return prop.stringValue; } }
        public int enumValueIndex { get { return prop.enumValueIndex; } }

        
        public int arraySize { get { return prop.arraySize; } }
        bool isArray { get { return prop.isArray; } }
        
        List<EditorProp> arrayElements = new List<EditorProp>();
        Dictionary<string, EditorProp> name2Prop = new Dictionary<string, EditorProp>();
        public EditorProp (SerializedProperty prop) {
            this.prop = prop;
            if (isArray) RebuildArray(); 
        }

        void RebuildArray () {
            int c = arraySize;
            arrayElements.Clear();
            for (int i = 0; i < c; i++) arrayElements.Add( new EditorProp( prop.GetArrayElementAtIndex(i) ) );
        }

        public bool ContainsDuplicateNames (out string duplicateName, string checkNameField) {
            duplicateName = string.Empty;
            if (CheckNonArray()) return false;
            int l = arraySize;
            for (int i = 0; i < l; i++) {
                string oName = this[i][checkNameField].stringValue;
                for (int x = i+1; x < l; x++) {
                    if (oName == this[x][checkNameField].stringValue) {
                        duplicateName = oName;
                        return true;
                    }
                }
            }
            return false;
        }

        bool CheckNonArray () {
            if (isArray) return false;
            Debug.LogError (prop.displayName + " isnt an array type");
            return true;
        }

        public void SetEnumValue (int value) { prop.enumValueIndex = value; }
        public void SetValue (bool value) { prop.boolValue = value; }
        public void SetValue (float value) { prop.floatValue = value; }
        public void SetValue (int value) { prop.intValue = value; }
        public void SetValue (Object value) { prop.objectReferenceValue = value; }
        public void SetValue (string value) { prop.stringValue = value; }

        public void Clear () {
            if (CheckNonArray()) return;
            prop.ClearArray();
            arrayElements.Clear();
        }
        public EditorProp AddNew () {
            if (CheckNonArray()) return null;
            int l = prop.arraySize;
            prop.InsertArrayElementAtIndex(l);
            arrayElements.Add( new EditorProp( prop.GetArrayElementAtIndex(l) ) );
            return arrayElements.Last();
        }
        public void DeleteAt (int index) {
            if (CheckNonArray()) return;
            prop.DeleteArrayElementAtIndex(index);
            RebuildArray(); 
        }
        public EditorProp this [int index] {
            get {
                if (CheckNonArray()) return null;
                if (index < 0 || index >= arraySize) {
                    Debug.LogError("Array index out of range");
                    return null;
                }
                return arrayElements[index];
            }
        }

        public EditorProp this [string name] {
            get {
                if (isArray) {
                    Debug.LogError (prop.displayName + " is an array type");
                    return null;
                }
                EditorProp customProp;
                if (!name2Prop.TryGetValue(name, out customProp)) {
                    customProp = new EditorProp ( prop.FindPropertyRelative ( name ) );
                    name2Prop.Add(name, customProp);
                }
                return customProp;
            }
        }

        public void CopyProp (EditorProp copy) {
            SerializedProperty c = copy.prop;
            if (prop.propertyType != c.propertyType) {
                Debug.LogError("Incompatible types (" + prop.propertyType + ", " + c.propertyType + ")");
                return;
            }
            prop.intValue = c.intValue;
            prop.boolValue = c.boolValue;
            prop.floatValue = c.floatValue;
            prop.stringValue = c.stringValue;
            prop.colorValue = c.colorValue;
            prop.objectReferenceValue = c.objectReferenceValue;
            prop.enumValueIndex = c.enumValueIndex;
            prop.vector2Value = c.vector2Value;
            prop.vector3Value = c.vector3Value;
            prop.vector4Value = c.vector4Value;
            prop.rectValue = c.rectValue;
            prop.animationCurveValue = c.animationCurveValue;
            prop.boundsValue = c.boundsValue;
            prop.quaternionValue = c.quaternionValue;
            prop.exposedReferenceValue = c.exposedReferenceValue;
            prop.vector2IntValue = c.vector2IntValue;
            prop.vector3IntValue = c.vector3IntValue;
            prop.rectIntValue = c.rectIntValue;
            prop.boundsIntValue = c.boundsIntValue;

            /*
            switch (p.propertyType){
                case SerializedPropertyType.Integer	:p.intValue = c.intValue;break;
                case SerializedPropertyType.Boolean	:p.boolValue = c.boolValue;break;
                case SerializedPropertyType.Float	:p.floatValue = c.floatValue;break;
                case SerializedPropertyType.String	:p.stringValue = c.stringValue;break;
                case SerializedPropertyType.Color	:p.colorValue = c.colorValue;break;
                case SerializedPropertyType.ObjectReference	:p.objectReferenceValue = c.objectReferenceValue;break;
                case SerializedPropertyType.Enum	:p.enumValueIndex = c.enumValueIndex;break;
                case SerializedPropertyType.Vector2	:p.vector2Value = c.vector2Value;break;
                case SerializedPropertyType.Vector3	:p.vector3Value = c.vector3Value;break;
                case SerializedPropertyType.Vector4	:p.vector4Value = c.vector4Value;break;
                case SerializedPropertyType.Rect	:p.rectValue = c.rectValue;break;
                case SerializedPropertyType.AnimationCurve	:p.animationCurveValue = c.animationCurveValue;break;
                case SerializedPropertyType.Bounds	:p.boundsValue = c.boundsValue;break;
                case SerializedPropertyType.Quaternion	:p.quaternionValue = c.quaternionValue;break;
                case SerializedPropertyType.ExposedReference:	p.exposedReferenceValue = c.exposedReferenceValue;break;
                case SerializedPropertyType.Vector2Int:	p.vector2IntValue = c.vector2IntValue;break;
                case SerializedPropertyType.Vector3Int:	p.vector3IntValue = c.vector3IntValue;break;
                case SerializedPropertyType.RectInt	:p.rectIntValue = c.rectIntValue;break;
                case SerializedPropertyType.BoundsInt	:p.boundsIntValue = c.boundsIntValue;break;
                default:Debug.LogError("Not implemented: " + p.propertyType);break;
            }
            */
        }
    }
}