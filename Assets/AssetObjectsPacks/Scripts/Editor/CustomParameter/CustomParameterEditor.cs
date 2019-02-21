using UnityEngine;
using UnityEditor;
namespace AssetObjectsPacks {
    public static class CustomParameterEditor {

        const string hintField = "hint", typeField = "paramType", nameField = "name";


        public static void CopyParameterList (EditorProp orig, EditorProp toCopy) {
            orig.Clear();
            for (int p = 0; p < toCopy.arraySize; p++) CopyParameter(orig.AddNew(), toCopy[p]);
        }

        public static bool ParamsListContainsDuplicateName (EditorProp parameters, out string dupName) {
            return parameters.ContainsDuplicateNames(out dupName, nameField);     
        }
        public static void MakeParamDefault (EditorProp parameter) {
            parameter[hintField].SetValue( "Hint" );   
            parameter[nameField].SetValue( "Parameter Name" );
        }
        public static void DefaultDurationParameter (EditorProp parameter) {
            parameter[nameField].SetValue( "Duration" );
            parameter[typeField].SetEnumValue( (int)CustomParameter.ParamType.FloatValue );
            parameter[CustomParameter.ParamType.FloatValue.ToString()].SetValue( -1.0f );
            parameter[hintField].SetValue( "Nagative values for object duration" );
        }
        public static EditorProp GetParamProperty(EditorProp parameter) {
            return parameter[((CustomParameter.ParamType)parameter[typeField].enumValueIndex).ToString()];
        }
        public static void CopyParameter(EditorProp orig, EditorProp to_copy) {
            orig[nameField].CopyProp(to_copy[nameField]);
            orig[typeField].CopyProp(to_copy[typeField]);
            GetParamProperty(orig).CopyProp(GetParamProperty(to_copy));
        }
        public static void ClearAndRebuildParameters(EditorProp parameters, EditorProp defaultParams) {
            parameters.Clear();
            int l = defaultParams.arraySize;    
            for (int i = 0; i < l; i++) CopyParameter(parameters.AddNew(), defaultParams[i]);
        }
        public static void UpdateParametersToReflectDefaults (EditorProp parameters, EditorProp defaultParams) {
            
            int c_p = parameters.arraySize;
            int c_d = defaultParams.arraySize;

            System.Func<EditorProp, string> GetParamName = (EditorProp parameter) => { return parameter[nameField].stringValue; };
            System.Func<EditorProp, int> GetParamType = (EditorProp parameter) => { return parameter[typeField].enumValueIndex; };

            //check for parameters to delete
            for (int i = c_p - 1; i >= 0; i--) {                    
                string param_name = GetParamName(parameters[i]);
                bool is_in_default_params = false;
                for (int d = 0; d < c_d; d++) {                
                    if (GetParamName(defaultParams[d]) == param_name) {
                        is_in_default_params = true;
                        break;
                    }
                }
                if (!is_in_default_params) parameters.DeleteAt(i);
            }

            //check for parameters that need adding
            for (int i = 0; i < c_d; i++) {
                EditorProp defParam = defaultParams[i];
                string defParamName = GetParamName(defParam);
                bool is_in_params = false;
                for (int p = 0; p < c_p; p++) {
                    if (GetParamName(parameters[p])== defParamName) {
                        is_in_params = true;
                        break;
                    }
                }
                if (!is_in_params) CopyParameter(parameters.AddNew(), defParam);
            }
            
            //reorder to same order

            //make extra temp parameeter
            EditorProp temp = parameters.AddNew();

            for (int d = 0; d < c_d; d++) {
                string defParamName = GetParamName(defaultParams[d]);
                EditorProp parameter = parameters[d];

                if (GetParamName(parameter) == defParamName) continue;

                EditorProp real_param = null;
                for (int p = d + 1; p < c_p; p++) {
                    real_param = parameters[p];
                    if (GetParamName(real_param) == defParamName) break;
                }

                //put the current one in temp
                CopyParameter(temp, parameter);
                //place the real param in the current
                CopyParameter(parameter, real_param);
                //place temp in old param that was moved
                CopyParameter(real_param, temp);
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
            static readonly GUIContent deleteParameterGUI = new GUIContent("D", "Delete Parameter");
            static readonly GUILayoutOption pFieldWidth = GUILayout.Width(75);
            public static GUIContent GetParamGUI (EditorProp parameter) {
                return new GUIContent(parameter[nameField].stringValue, parameter[hintField].stringValue);
            }
            public static void DrawAOParameters (EditorProp parameters, GUILayoutOption[] paramWidths) {
                EditorGUILayout.BeginHorizontal();
                GUIContent blank = GUIUtils.blank_content;
                int l = parameters.arraySize;  
                for (int i = 0; i < l; i++) {
                    GUIUtils.DrawProp( GetParamProperty( parameters[i] ), blank, paramWidths[i]);
                    GUIUtils.SmallButtonClear();   
                }
                EditorGUILayout.EndHorizontal();
            }
            public static void DrawParamsList (EditorProp parameters, bool pm, out bool changedAnyParamNames, out int deleteParamIndex) {
                deleteParamIndex = -1;
                changedAnyParamNames = false;

                int l = parameters.arraySize;
                for (int i = 0; i < l; i++) {

                    bool d, changedParamName;
                    UnityEngine.GUI.enabled = i != 0 || !pm;
                    DrawParameter(pm, parameters[i], out d, out changedParamName);
                    UnityEngine.GUI.enabled = true;
            
                    if(d) deleteParamIndex = i;
                    changedAnyParamNames = changedAnyParamNames || changedParamName;
                }
            }
            static void DrawParameter(bool drawHint, EditorProp parameter, out bool delete, out bool changedName) {
                if (drawHint) GUIUtils.StartBox(0);
        
                GUIContent blank = GUIUtils.blank_content;
                
                EditorGUILayout.BeginHorizontal();
                delete = GUIUtils.SmallButton(deleteParameterGUI, EditorColors.red, EditorColors.white);
               
                changedName = false;
                if (drawHint) {
                    //hint
                    EditorGUILayout.BeginVertical();
                    GUIUtils.DrawProp(parameter[hintField], blank);
                    EditorGUILayout.BeginHorizontal();
                    //name
                    changedName = GUIUtils.DrawDelayedTextProp(parameter[nameField]);
                }
                else {
                    //name
                    GUIUtils.NextControlOverridesKeyboard();
                    GUIUtils.DrawProp(parameter[nameField], blank );
                    GUIUtils.CheckLoseFocusLastRect();
                }

                //type
                GUIUtils.DrawProp(parameter[typeField], blank, pFieldWidth);
                //value
                GUIUtils.DrawProp(GetParamProperty( parameter), blank, pFieldWidth);
                
                if (drawHint) {
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndVertical();
                }

                EditorGUILayout.EndHorizontal();

                if (drawHint) GUIUtils.EndBox(0);
            }
        }
    }
}