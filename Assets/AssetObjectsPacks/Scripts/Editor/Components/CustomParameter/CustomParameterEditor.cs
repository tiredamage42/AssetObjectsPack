using UnityEngine;
using UnityEditor;
using System;
namespace AssetObjectsPacks {
    public static class CustomParameterEditor {

        const string typeField = "pType", nameField = "name";

        public static void CopyParameterList (EditorProp orig, EditorProp toCopy) {
            orig.Clear();
            int l = toCopy.arraySize;
            for (int p = 0; p < l; p++) CopyParameter(orig.AddNew(), toCopy[p]);
        }
        public static void DefaultDurationParameter (EditorProp parameter) {
            parameter[nameField].SetValue( "Duration" );
            parameter[typeField].SetValue( (int)CustomParameter.ParamType.FloatValue );

            parameter[CustomParameter.ParamType.FloatValue.ToString()].SetValue( -1.0f );
//            parameter["value"].SetValue( -1.0f );


        
        }
        public static EditorProp GetParamValueProperty(EditorProp parameter) {
            return parameter[((CustomParameter.ParamType)parameter[typeField].intValue).ToString()];
        //    return parameter["value"];
        
        }
        public static void CopyParameter(EditorProp orig, EditorProp to_copy) {
            orig[nameField].CopyProp(to_copy[nameField]);
            orig[typeField].CopyProp(to_copy[typeField]);
            GetParamValueProperty(orig).CopyProp(GetParamValueProperty(to_copy));
        }
        public static void UpdateParametersToReflectDefaults (EditorProp parameters, EditorProp defaultParams) {
            int c_p = parameters.arraySize;
            int c_d = defaultParams.arraySize;

            Func<EditorProp, string> GetParamName = (EditorProp parameter) => parameter[nameField].stringValue;

            //check for parameters to delete
            for (int i = c_p - 1; i >= 0; i--) {                    
                string name = GetParamName(parameters[i]);
                bool inDefParams = false;
                for (int d = 0; d < c_d; d++) {                
                    if (GetParamName(defaultParams[d]) == name) {
                        inDefParams = true;
                        break;
                    }
                }
                if (!inDefParams) {
                    Debug.Log("Deleting param: " + name);
                    parameters.DeleteAt(i);
                }
            }

            //check for parameters that need adding
            for (int i = 0; i < c_d; i++) {
                EditorProp defParam = defaultParams[i];
                string defParamName = GetParamName(defParam);
                bool inParams = false;
                for (int p = 0; p < c_p; p++) {
                    if (GetParamName(parameters[p])== defParamName) {
                        inParams = true;
                        break;
                    }
                }
                if (!inParams) {
                    Debug.Log("adding param: " + defParamName);
                    
                    CopyParameter(parameters.AddNew(), defParam);
                }
            }
            
            //reorder to same order

            //make extra temp parameeter
            EditorProp temp = parameters.AddNew();

            for (int d = 0; d < c_d; d++) {
                string defParamName = GetParamName(defaultParams[d]);
                EditorProp parameter = parameters[d];
                if (GetParamName(parameter) == defParamName) continue;

                Debug.Log("moving param: " + GetParamName(parameter));
                    
                EditorProp trueParam = null;
                for (int p = d + 1; p < c_p; p++) {
                    trueParam = parameters[p];
                    if (GetParamName(trueParam) == defParamName) break;
                }
                //put the current one in temp
                CopyParameter(temp, parameter);
                //place the real param in the current
                CopyParameter(parameter, trueParam);
                //place temp in old param that was moved
                CopyParameter(trueParam, temp);
            }
            //delete temp parameter
            parameters.DeleteAt(parameters.arraySize-1);
            
            //check type changes
            Func<EditorProp, int> GetParamType = (EditorProp parameter) => parameter[typeField].intValue;
            for (int i = 0; i < c_d; i++) {
                if (GetParamType(parameters[i]) != GetParamType(defaultParams[i])) {
                    Debug.Log("chaning: " + GetParamName(parameters[i]) + " from " + GetParamType(parameters[i]) + " to " + GetParamType(defaultParams[i]));
                    CopyParameter(parameters[i], defaultParams[i]);
                }
            }
        }
    
        public static class GUI  {
            public static GUIContent GetNameGUI (EditorProp parameter) {
                return new GUIContent(parameter[nameField].stringValue);
            }
            public static void DrawParameter(EditorProp parameter) {        
                //name
                GUIUtils.DrawTextProp(parameter[nameField], GUIUtils.TextFieldType.Normal, false, GUILayout.MinWidth(32));
                
                //type
                GUILayoutOption pFieldWidth = GUILayout.Width(75);
                EditorProp typeProp = parameter[typeField];
                int origValue = typeProp.intValue;
                int newValue = (int)(CustomParameter.ParamType)EditorGUILayout.EnumPopup((CustomParameter.ParamType)origValue, pFieldWidth);
                if (newValue != origValue) typeProp.SetValue( newValue );
                
                //value
                GUIUtils.DrawProp(GetParamValueProperty( parameter), pFieldWidth);
            }
        }
    }
}