//using System.Collections;
//using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


namespace AssetObjectsPacks {

    public static class SerializedPropertyUtils
    {

        public static class Parameters {

            public static void CopyParameter(SerializedProperty orig, SerializedProperty to_copy) {
                orig.FindPropertyRelative(AssetObjectParam.name_field).CopyProperty(to_copy.FindPropertyRelative(AssetObjectParam.name_field));
                orig.FindPropertyRelative(AssetObjectParam.param_type_field).CopyProperty(to_copy.FindPropertyRelative(AssetObjectParam.param_type_field));
                orig.FindPropertyRelative(AssetObjectParam.bool_val_field).CopyProperty(to_copy.FindPropertyRelative(AssetObjectParam.bool_val_field));
                orig.FindPropertyRelative(AssetObjectParam.float_val_field).CopyProperty(to_copy.FindPropertyRelative(AssetObjectParam.float_val_field));
                orig.FindPropertyRelative(AssetObjectParam.int_val_field).CopyProperty(to_copy.FindPropertyRelative(AssetObjectParam.int_val_field));
            }
            public static void CopyParameter (SerializedProperty orig, AssetObjectParam to_copy) {
                orig.FindPropertyRelative(AssetObjectParam.name_field).stringValue = to_copy.name;
                orig.FindPropertyRelative(AssetObjectParam.param_type_field).enumValueIndex = (int)to_copy.paramType;
                orig.FindPropertyRelative(AssetObjectParam.bool_val_field).boolValue = to_copy.boolValue;
                orig.FindPropertyRelative(AssetObjectParam.float_val_field).floatValue = to_copy.floatValue;
                orig.FindPropertyRelative(AssetObjectParam.int_val_field).intValue = to_copy.intValue;
            }

            public static void UpdateParametersToReflectDefaults (SerializedProperty params_list, AssetObjectParamDef[] defs) {
            
                int c_p = params_list.arraySize;
                int c_d = defs.Length;

                //check for parameters to delete
                for (int i = c_p - 1; i >= 0; i--) {
                    string param_name = params_list.GetArrayElementAtIndex(i).FindPropertyRelative(AssetObjectParam.name_field).stringValue;
                    bool is_in_default_params = false;
                    for (int d = 0; d < c_d; d++) {
                        if (defs[d].parameter.name == param_name) {
                            is_in_default_params = true;
                            break;
                        }
                    }
                    if (!is_in_default_params) {
                        params_list.DeleteArrayElementAtIndex(i);
                        Debug.Log("Deleteing param: " + param_name);
                    }
                }

                //check for parameters that need adding
                for (int i = 0; i < c_d; i++) {
                    AssetObjectParam def_param = defs[i].parameter;
                    bool is_in_params = false;
                    for (int p = 0; p < c_p; p++) {
                        if (params_list.GetArrayElementAtIndex(i).FindPropertyRelative(AssetObjectParam.name_field).stringValue == def_param.name) {
                            is_in_params = true;
                            break;
                        }
                    }
                    if (!is_in_params) {
                        CopyParameter(params_list.AddNewElement(), def_param);
                        Debug.Log("Adding Param: " + def_param.name);
                    }
                }

                //reorder to same order

                //make extra temp parameeter
                SerializedProperty temp = params_list.AddNewElement();

                for (int d = 0; d < c_d; d++) {
                    AssetObjectParam def_param = defs[d].parameter;
                    SerializedProperty param = params_list.GetArrayElementAtIndex(d);

                    if (param.FindPropertyRelative(AssetObjectParam.name_field).stringValue == def_param.name) continue;

                    SerializedProperty real_param = null;
                    for (int p = d + 1; p < c_p; p++) {
                        real_param = params_list.GetArrayElementAtIndex(d);
                        if (real_param.FindPropertyRelative(AssetObjectParam.name_field).stringValue == def_param.name) break;
                    }

                    //put the current one in temp
                    CopyParameter(temp, param);
                    //place the real param in the current
                    CopyParameter(param, real_param);
                    //place temp in old param that was moved
                    CopyParameter(real_param, temp);
                }

                //delete temp parameter
                params_list.DeleteArrayElementAtIndex(params_list.arraySize-1);

                //check type changes
                for (int i = 0; i < c_d; i++) {
                    AssetObjectParam def_param = defs[i].parameter;
                    SerializedProperty param = params_list.GetArrayElementAtIndex(i);
                    if (param.FindPropertyRelative(AssetObjectParam.param_type_field).enumValueIndex != (int)def_param.paramType) {
                        CopyParameter(param, def_param);
                        Debug.Log("chaging type " + param.FindPropertyRelative(AssetObjectParam.name_field).stringValue);
                    }
                }
            }

            
        }
        



    }


}
