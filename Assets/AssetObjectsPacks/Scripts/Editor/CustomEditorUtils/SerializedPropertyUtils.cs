﻿using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace AssetObjectsPacks {
    enum EditorPropType { Property, Array, SO }
    public class EditorProp {
        public SerializedProperty property;
        EditorPropType editorPropType;
        EditorProp baseObject;
        SerializedObject serializedObject;
        public string displayName { get { return errored ? "ERRORED" : (isBaseObject ? serializedObject.targetObject.name : property.displayName); } }
        bool isBaseObject { get { return editorPropType == EditorPropType.SO; } } 
        bool errored;
        Dictionary<string, EditorProp> name2Prop = new Dictionary<string, EditorProp>();
        EditorProp (SerializedProperty property, EditorProp baseObject) {
            this.property = property;
            this.baseObject = baseObject;
            editorPropType = EditorPropType.Property;
            errored = property == null;
            if (!errored) {
                if (property.isArray && property.propertyType != SerializedPropertyType.String) {
                    editorPropType = EditorPropType.Array;
                    RebuildArray(); 
                }
            }
        }

        public EditorProp (SerializedObject serializedObject) {
            this.serializedObject = serializedObject;
            this.baseObject = this;
            editorPropType = EditorPropType.SO;
            errored = serializedObject == null;
            
        }
        
        bool CheckEditorPropType (EditorPropType shouldBe) {
            if (errored) {
                Debug.LogError(displayName + " is errored");
                return false;
            }
            if (shouldBe != editorPropType) {
                Debug.LogError ( displayName + "(" + editorPropType.ToString() + ") isnt type: " + shouldBe.ToString());
                return false;
            }
            return true;
        }

        public void SaveObject () {
            if (editorPropType != EditorPropType.SO) {
                baseObject.SaveObject();
                return;
            }
            if (!errored) {
                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(serializedObject.targetObject);
            }
        }

        //get property/relative
        public EditorProp this [string name] {
            get {
                if (errored) return null;
                
                EditorProp customProp;
                if (!name2Prop.TryGetValue(name, out customProp)) {
                    SerializedProperty p = null;
                    if (editorPropType == EditorPropType.SO)
                        p = serializedObject.FindProperty ( name );
                    else
                        p = property.FindPropertyRelative ( name );
                    
                    if (p == null) Debug.Log("cant find param name: " + name + " on " + displayName);
                    customProp = new EditorProp ( p, baseObject );
                    name2Prop.Add(name, customProp);
                }
                return customProp;
            }
        }

        #region ARRAY_PROP
            List<EditorProp> arrayElements = new List<EditorProp>();

            //index into array prop
            public EditorProp this [int index] {
                get {
        
                    if (!CheckForArray(false)) return null;
                    if (index < 0 || index >= arraySize) {
                        Debug.LogError("Array index out of range on " + displayName + " tried: " + index + " count: "+ arraySize);
                        return null;
                    }
                    return arrayElements[index];
                }
            }
            bool CheckForArray(bool change) {
                if (!CheckEditorPropType(EditorPropType.Array)) return false;
                if (change) SaveObject();
                return true;
            }
            void RebuildArray () {
                arrayElements = arraySize.Generate(i => new EditorProp( property.GetArrayElementAtIndex(i), baseObject ) ).ToList();
            }
            public bool ContainsDuplicateNames (out string duplicateName, string checkNameField="name") {
                duplicateName = string.Empty;
                
                if (!CheckForArray(false)) return false;
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

            public void Clear () {
                
                if (!CheckForArray(true)) return;

                property.ClearArray();
                
                arrayElements.Clear();
            }
            bool NameUnique(string name, string nameField) {
                for (int i = 0; i < arraySize; i++) {
                    if (this[i][nameField].stringValue == name) return false;
                }
                return true;
            }
            string UniqueName (string prefix, string nameField) {
                if (prefix == null) return null;
                string origName = prefix;
                string new_name = origName;
                int trying = 0;
                while (!NameUnique(new_name, nameField) && trying <= 999 ) {
                    new_name = origName + " " + trying.ToString();
                    trying ++;
                }
                return new_name;
            }
            public EditorProp InsertAtIndex (int i, string uniqueNamePrefix = null, string nameField = "name") {
                
                if (!CheckForArray(true)) return null;
                string new_name = UniqueName(uniqueNamePrefix, nameField);
                property.InsertArrayElementAtIndex(i);
                RebuildArray();
                if (new_name != null) arrayElements[i][nameField].SetValue(new_name);
                return arrayElements[i];
            }

            public EditorProp AddNew (string uniqueNamePrefix = null, string nameField = "name") {
                
                if (!CheckForArray(true)) return null;
                string new_name = UniqueName(uniqueNamePrefix, nameField);            
                
                int l = arraySize;
                property.InsertArrayElementAtIndex(l);
                
                EditorProp newElement = new EditorProp( property.GetArrayElementAtIndex(l), baseObject );
                
                arrayElements.Add( newElement );
                
                if (new_name != null) newElement[nameField].SetValue(new_name);
                
                return newElement;
            }
            public void DeleteAt (int index, string reason) {
                
                if (!CheckForArray(true)) return;
                if (property.propertyType == SerializedPropertyType.ObjectReference && objRefValue != null) {
                    property.DeleteArrayElementAtIndex(index);
                }
                property.DeleteArrayElementAtIndex(index);
                RebuildArray(); 

                Debug.Log("deleted and rebuilt " + reason);
            }
        #endregion

        #region GETTERS_SETTERS
            #region GETTERS
                public Vector2Int vector2IntValue { get { 
                    if (!CheckEditorPropType(EditorPropType.Property)) return Vector2Int.zero;
                    return property.vector2IntValue;
                } }
                public int intValue { get { 
                    if (!CheckEditorPropType(EditorPropType.Property)) return 0;
                    return property.intValue;
                } }
                public float floatValue { get { 
                    if (!CheckEditorPropType(EditorPropType.Property)) return 0;
                    return property.floatValue; 
                } }
                public bool boolValue { get { 
                    if (!CheckEditorPropType(EditorPropType.Property)) return false;
                    return property.boolValue; 
                } }
                public string stringValue { get { 
                    if (!CheckEditorPropType(EditorPropType.Property)) return "";
                    return property.stringValue; 
                } }
                public int enumValueIndex { get { 
                    if (!CheckEditorPropType(EditorPropType.Property)) return 0;
                    return property.enumValueIndex; 
                } }
                public int arraySize { get { 
                    if (!CheckEditorPropType(EditorPropType.Array)) return 0;
                    return property.arraySize; 
                } }
                public Object objRefValue { get { 
                    if (!CheckEditorPropType(EditorPropType.Property)) return null;
                    return property.objectReferenceValue;
                } }
            #endregion
            #region SETTERS
                public void SetValue (Vector2Int value) { 
                    if (!CheckEditorPropType(EditorPropType.Property)) return;
                    property.vector2IntValue = value; 
                }
                public void SetEnumValue (int value) { 
                    if (!CheckEditorPropType(EditorPropType.Property)) return;
                    property.enumValueIndex = value; 
                }
                public void SetValue (bool value) { 
                    if (!CheckEditorPropType(EditorPropType.Property)) return;
                    property.boolValue = value; 
                }
                public void SetValue (float value) { 
                    if (!CheckEditorPropType(EditorPropType.Property)) return;
                    property.floatValue = value; 
                }
                public void SetValue (int value) { 
                    if (!CheckEditorPropType(EditorPropType.Property)) return;
                    property.intValue = value; 
                }
                public void SetValue (Object value) { 
                    if (!CheckEditorPropType(EditorPropType.Property)) return;
                    property.objectReferenceValue = value; 
                }
                public void SetValue (string value) { 
                    if (!CheckEditorPropType(EditorPropType.Property)) return;
                    property.stringValue = value; 
                }
            #endregion
        #endregion

        public void CopySubProps (EditorProp copy, IEnumerable<string> props) {
            foreach (var p in props) {
                this[p].CopyProp(copy[p]);
            }
        }
        public void CopyProp (EditorProp copy) {
            if (!CheckEditorPropType(EditorPropType.Property)) return;

            SerializedProperty c = copy.property;
            if (property.propertyType != c.propertyType) {
                Debug.LogError("Incompatible types (" + property.displayName + "," + c.displayName + ") (" + property.propertyType + ", " + c.propertyType + ")");
                return;
            }
            switch (property.propertyType){
                case SerializedPropertyType.Integer	            :   property.intValue              =    c.intValue              ;break;
                case SerializedPropertyType.Boolean	            :   property.boolValue             =    c.boolValue             ;break;
                case SerializedPropertyType.Float	            :   property.floatValue            =    c.floatValue            ;break;
                case SerializedPropertyType.String	            :   property.stringValue           =    c.stringValue           ;break;
                case SerializedPropertyType.Color	            :   property.colorValue            =    c.colorValue            ;break;
                case SerializedPropertyType.ObjectReference	    :   property.objectReferenceValue  =    c.objectReferenceValue  ;break;
                case SerializedPropertyType.Enum	            :   property.enumValueIndex        =    c.enumValueIndex        ;break;
                case SerializedPropertyType.Vector2	            :   property.vector2Value          =    c.vector2Value          ;break;
                case SerializedPropertyType.Vector3	            :   property.vector3Value          =    c.vector3Value          ;break;
                case SerializedPropertyType.Vector4	            :   property.vector4Value          =    c.vector4Value          ;break;
                case SerializedPropertyType.Rect	            :   property.rectValue             =    c.rectValue             ;break;
                case SerializedPropertyType.AnimationCurve	    :   property.animationCurveValue   =    c.animationCurveValue   ;break;
                case SerializedPropertyType.Bounds	            :   property.boundsValue           =    c.boundsValue           ;break;
                case SerializedPropertyType.Quaternion	        :   property.quaternionValue       =    c.quaternionValue       ;break;
                case SerializedPropertyType.ExposedReference    :   property.exposedReferenceValue =    c.exposedReferenceValue ;break;
                case SerializedPropertyType.Vector2Int          :   property.vector2IntValue       =    c.vector2IntValue       ;break;
                case SerializedPropertyType.Vector3Int          :   property.vector3IntValue       =    c.vector3IntValue       ;break;
                case SerializedPropertyType.RectInt	            :   property.rectIntValue          =    c.rectIntValue          ;break;
                case SerializedPropertyType.BoundsInt	        :   property.boundsIntValue        =    c.boundsIntValue        ;break;
                
                default:Debug.LogError("Not implemented: " + property.propertyType);break;
            }
        }
    }
}