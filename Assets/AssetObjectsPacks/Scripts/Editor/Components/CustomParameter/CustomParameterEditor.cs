using UnityEngine;
namespace AssetObjectsPacks {
    public static class CustomParameterEditor {
        public const string typeField = "pType", nameField = "name";
        public static void CopyParameterList (EditorProp orig, EditorProp toCopy) {
            orig.Clear();
            int l = toCopy.arraySize;
            for (int p = 0; p < l; p++) {
                CopyParameter(orig.AddNew(), toCopy[p]);
                //Debug.Log("copying :" + toCopy[p][nameField].stringValue);
            }
        }
        public static EditorProp GetParamValueProperty(EditorProp parameter) {
            return parameter[((CustomParameter.ParamType)parameter[typeField].intValue).ToString()];        
        }
        public static void CopyParameter(EditorProp orig, EditorProp to_copy) {
            orig[nameField].CopyProp(to_copy[nameField]);
            orig[typeField].CopyProp(to_copy[typeField]);
            GetParamValueProperty(orig).CopyProp(GetParamValueProperty(to_copy));
        }
    }
}