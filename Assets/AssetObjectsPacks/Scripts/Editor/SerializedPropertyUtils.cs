//using System.Collections;
//using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


namespace AssetObjectsPacks {

    public static class SerializedPropertyUtils
    {

        public static class Parameters {
            public static SerializedProperty GetParamProperty(SerializedProperty parameter) {
                switch((AssetObjectParam.ParamType)parameter.FindPropertyRelative(AssetObjectParam.param_type_field).enumValueIndex) {

                    case AssetObjectParam.ParamType.Bool: return parameter.FindPropertyRelative(AssetObjectParam.bool_val_field);
                    case AssetObjectParam.ParamType.Float: return parameter.FindPropertyRelative(AssetObjectParam.float_val_field);
                    case AssetObjectParam.ParamType.Int: return parameter.FindPropertyRelative(AssetObjectParam.int_val_field);

                }
                return null;
                    
            }

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

            public static void UpdateParametersToReflectDefaults (SerializedProperty parameters, AssetObjectParamDef[] defs) {
            
                int c_p = parameters.arraySize;
                int c_d = defs.Length;

                //check for parameters to delete
                for (int i = c_p - 1; i >= 0; i--) {
                    string param_name = parameters.GetArrayElementAtIndex(i).FindPropertyRelative(AssetObjectParam.name_field).stringValue;
                    bool is_in_default_params = false;
                    for (int d = 0; d < c_d; d++) {
                        if (defs[d].parameter.name == param_name) {
                            is_in_default_params = true;
                            break;
                        }
                    }
                    if (!is_in_default_params) {
                        parameters.DeleteArrayElementAtIndex(i);
                        Debug.Log("Deleteing param: " + param_name);
                    }
                }

                Debug.Log("done deleting excess params");


                //check for parameters that need adding
                for (int i = 0; i < c_d; i++) {
                    AssetObjectParam def_param = defs[i].parameter;
                    bool is_in_params = false;
                    for (int p = 0; p < c_p; p++) {
                        if (parameters.GetArrayElementAtIndex(i).FindPropertyRelative(AssetObjectParam.name_field).stringValue == def_param.name) {
                            is_in_params = true;
                            break;
                        }
                    }
                    if (!is_in_params) {
                        CopyParameter(parameters.AddNewElement(), def_param);
                        Debug.Log("Adding Param: " + def_param.name);
                    }
                }

                Debug.Log("done adding params");

                //reorder to same order

                //make extra temp parameeter
                SerializedProperty temp = parameters.AddNewElement();



                for (int d = 0; d < c_d; d++) {
                    AssetObjectParam def_param = defs[d].parameter;
                    SerializedProperty parameter = parameters.GetArrayElementAtIndex(d);

                    if (parameter.FindPropertyRelative(AssetObjectParam.name_field).stringValue == def_param.name) continue;

                    Debug.Log("looking for param: " + def_param.name);
                    SerializedProperty real_param = null;
                    for (int p = d + 1; p < c_p; p++) {
                        real_param = parameters.GetArrayElementAtIndex(p);
                        if (real_param.FindPropertyRelative(AssetObjectParam.name_field).stringValue == def_param.name) break;
                    }

                    //put the current one in temp
                    CopyParameter(temp, parameter);
                    //place the real param in the current
                    CopyParameter(parameter, real_param);
                    //place temp in old param that was moved
                    CopyParameter(real_param, temp);
                }
                Debug.Log("done reordering " + parameters.arraySize);
                //delete temp parameter
                parameters.DeleteArrayElementAtIndex(parameters.arraySize-1);
                Debug.Log("done reordering 2 " + parameters.arraySize);
                
                //check type changes
                for (int i = 0; i < c_d; i++) {
                    AssetObjectParam def_param = defs[i].parameter;
                    SerializedProperty param = parameters.GetArrayElementAtIndex(i);
                    if (param.FindPropertyRelative(AssetObjectParam.param_type_field).enumValueIndex != (int)def_param.paramType) {
                        CopyParameter(param, def_param);
                        Debug.Log("chaging type " + param.FindPropertyRelative(AssetObjectParam.name_field).stringValue);
                    }
                }
            }

            
        }
        



    }


}
