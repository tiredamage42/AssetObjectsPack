using UnityEngine;
using UnityEditor;
using System;
namespace AssetObjectsPacks {
    public static class CustomParameterEditor {

        const string typeField = "paramType", nameField = "name";

        public static void CopyParameterList (EditorProp orig, EditorProp toCopy) {
            orig.Clear();
            for (int p = 0; p < toCopy.arraySize; p++) CopyParameter(orig.AddNew(), toCopy[p]);
        }
        public static void AddParameterToList (EditorProp parameters) {
            parameters.AddNew()[nameField].SetValue( "Parameter Name" );
        }
        public static void DefaultDurationParameter (EditorProp parameter) {
            parameter[nameField].SetValue( "Duration" );
            parameter[typeField].SetEnumValue( (int)CustomParameter.ParamType.FloatValue );
            parameter[CustomParameter.ParamType.FloatValue.ToString()].SetValue( -1.0f );
        }
        static EditorProp GetParamProperty(EditorProp parameter) {
            return parameter[((CustomParameter.ParamType)parameter[typeField].enumValueIndex).ToString()];
        }
        public static void CopyParameter(EditorProp orig, EditorProp to_copy) {
            orig[nameField].CopyProp(to_copy[nameField]);
            orig[typeField].CopyProp(to_copy[typeField]);
            GetParamProperty(orig).CopyProp(GetParamProperty(to_copy));
        }
        public static void ClearAndRebuildParameters(EditorProp parameters, EditorProp defaultParams) {
            Debug.Log("clearing and rebuilding");
            parameters.Clear();
            int l = defaultParams.arraySize;    
            for (int i = 0; i < l; i++) CopyParameter(parameters.AddNew(), defaultParams[i]);
        }
        public static void UpdateParametersToReflectDefaults (EditorProp parameters, EditorProp defaultParams) {
            Debug.Log("Updating params");
            int c_p = parameters.arraySize;
            int c_d = defaultParams.arraySize;

            Func<EditorProp, string> GetParamName = (EditorProp parameter) => parameter[nameField].stringValue;
            Func<EditorProp, int> GetParamType = (EditorProp parameter) => parameter[typeField].enumValueIndex;

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
                if (!inDefParams) parameters.DeleteAt(i);
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
                if (!inParams) CopyParameter(parameters.AddNew(), defParam);
            }
            
            //reorder to same order

            //make extra temp parameeter
            EditorProp temp = parameters.AddNew();

            for (int d = 0; d < c_d; d++) {
                string defParamName = GetParamName(defaultParams[d]);
                EditorProp parameter = parameters[d];
                if (GetParamName(parameter) == defParamName) continue;
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
            for (int i = 0; i < c_d; i++) {
                EditorProp defParam = defaultParams[i];
                EditorProp param = parameters[i];
                if (GetParamType(param) != GetParamType(defParam)) CopyParameter(param, defParam);
            }
        }
    
        public static class GUI  {
            public static GUIContent GetNameGUI (EditorProp parameter) {
                return new GUIContent(parameter[nameField].stringValue);
            }
            public static void DrawAOParameters (EditorProp parameters, GUILayoutOption[] paramWidths, bool doMulti, out int multiParamSet) {
                multiParamSet = -1;
                //GUIUtils.StartBox(0);
                EditorGUILayout.BeginHorizontal();
                GUIContent blank = GUIUtils.blank_content;
                int l = parameters.arraySize;  
                for (int i = 0; i < l; i++) {
                    GUIUtils.DrawProp( GetParamProperty( parameters[i] ), blank, paramWidths[i]);
                    if (doMulti){
                        if (GUIUtils.SmallButton(new GUIContent("S", "Set Values"))) multiParamSet = i;
                    }
                    else {
                        GUIUtils.SmallButtonClear();   
                    }
                }
                EditorGUILayout.EndHorizontal();
                //GUIUtils.EndBox(0);
            }
            public static void DrawParamsList (EditorProp parameters, bool pm, GUIContent deleteContainerGUI, out bool deleteContainer) {
                GUIUtils.StartBox (0);
                
                EditorGUILayout.BeginHorizontal();

                deleteContainer = false;

                if (!pm) deleteContainer = GUIUtils.SmallButton(deleteContainerGUI, Colors.red, Colors.white);
                
                if (GUIUtils.Button(new GUIContent("Add Parameter"), true, GUIStyles.miniButton)) AddParameterToList(parameters);
                
                EditorGUILayout.EndHorizontal();
                
                if (!pm) GUIUtils.BeginIndent();
                
                int deleteParamIndex = -1;
                for (int i = 0; i < parameters.arraySize; i++) {
                    bool d;
                    UnityEngine.GUI.enabled = i != 0 || !pm;
                    DrawParameter(pm, parameters[i], out d);
                    UnityEngine.GUI.enabled = true;
                    if(d) deleteParamIndex = i;
                }
                if (deleteParamIndex >= 0) parameters.DeleteAt(deleteParamIndex);

                if (!pm) GUIUtils.EndIndent();
                
                GUIUtils.EndBox(1);
            }
            static void DrawParameter(bool pm, EditorProp parameter, out bool delete) {        
                GUIContent blank = GUIUtils.blank_content;
                EditorGUILayout.BeginHorizontal();
                delete = GUIUtils.SmallButton(new GUIContent("D", "Delete Parameter"), Colors.red, Colors.white);
               
                GUIUtils.NextControlOverridesKeyboard();
                GUIUtils.DrawTextProp(parameter[nameField], GUILayout.MinWidth(32), true);
                GUIUtils.CheckLoseFocusLastRect();

                GUILayoutOption pFieldWidth = GUILayout.Width(75);
                GUIUtils.DrawProp(parameter[typeField], blank, pFieldWidth);
                GUIUtils.DrawProp(GetParamProperty( parameter), blank, pFieldWidth);
                EditorGUILayout.EndHorizontal();
            }
        }
    }
}