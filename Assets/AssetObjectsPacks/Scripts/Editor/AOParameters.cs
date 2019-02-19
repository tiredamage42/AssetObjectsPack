using UnityEngine;
using UnityEditor;

namespace AssetObjectsPacks {
    public static class AOParameters {

        public static EditorProp GetParamProperty(EditorProp parameter) {
            return parameter[((CustomParameter.ParamType)parameter[CustomParameter.typeField].enumValueIndex).ToString()];
        }

        public static void CopyParameter(EditorProp orig, EditorProp to_copy) {
            orig[CustomParameter.nameField].CopyProp(to_copy[CustomParameter.nameField]);
            orig[CustomParameter.typeField].CopyProp(to_copy[CustomParameter.typeField]);
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

            System.Func<EditorProp, string> GetParamName = (EditorProp parameter) => {
                return parameter[CustomParameter.nameField].stringValue;
            };
            System.Func<EditorProp, int> GetParamType = (EditorProp parameter) => {
                return parameter[CustomParameter.typeField].enumValueIndex;
            };

            //check for parameters to delete
            for (int i = c_p - 1; i >= 0; i--) {
                EditorProp parameter = parameters[i];
                    
                string param_name = GetParamName(parameter);
                bool is_in_default_params = false;
                for (int d = 0; d < c_d; d++) {
                    EditorProp defParam = defaultParams[d];
                
                    if (GetParamName(defParam) == param_name) {
                        is_in_default_params = true;
                        break;
                    }
                }
                if (!is_in_default_params) {
                    parameters.DeleteAt(i);
                    Debug.Log("Deleteing param: " + param_name);
                }
            }

            //check for parameters that need adding
            for (int i = 0; i < c_d; i++) {
                EditorProp defParam = defaultParams[i];
                string defParamName = GetParamName(defParam);
                //CustomParameter def_param = defs[i];

                
                bool is_in_params = false;
                for (int p = 0; p < c_p; p++) {
                    EditorProp parameter = parameters[p];
                    if (GetParamName(parameter)== defParamName) {
                        is_in_params = true;
                        break;
                    }
                }
                if (!is_in_params) {
                    CopyParameter(parameters.AddNew(), defParam);
                    Debug.Log("Adding Param: " + defParamName);
                }
            }
            
            //reorder to same order

            //make extra temp parameeter
            EditorProp temp = parameters.AddNew();

            for (int d = 0; d < c_d; d++) {
                //CustomParameter def_param = defs[d];
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

                if (GetParamType(param) != GetParamType(defParam)) {
                    CopyParameter(param, defParam);
                    Debug.Log("chaging type " + GetParamName(param));
                }
            }
        }
    }

}
