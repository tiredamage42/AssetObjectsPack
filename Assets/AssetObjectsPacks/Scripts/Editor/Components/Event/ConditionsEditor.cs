using UnityEngine;
using UnityEditor;
namespace AssetObjectsPacks {
    public static class ConditionsEditor {
        const string paramsField = "parameters";
        public static void CopyConditions(EditorProp conditions, EditorProp copy, bool additive) {
            if (!additive) conditions.Clear();
            for (int i = 0; i < copy.arraySize; i++) CustomParameterEditor.CopyParameterList(conditions.AddNew()[paramsField], copy[i][paramsField]);
        }
        public static class GUI {

            static bool showConditions;
            static readonly GUIContent showConditionsGUI = new GUIContent("C", "Show Conditions");
            
            public static void DrawConditions (EditorProp conditions, bool drawmulti, out bool multiConditionAdd, out bool multiConditionReplace) {
                
                
                EditorGUILayout.BeginHorizontal();
                
                showConditions = GUIUtils.SmallToggleButton(showConditionsGUI, showConditions, out _);

                multiConditionAdd = multiConditionReplace = false;
                
                if (showConditions){

                    if (GUIUtils.Button(new GUIContent("Add Condition +"), true, GUIStyles.miniButton)) {
                        EditorProp newParamsList = conditions.AddNew()[paramsField];
                        newParamsList.Clear();
                        CustomParameterEditor.AddParameterToList(newParamsList);
                    }
                    GUILayout.FlexibleSpace();
                    multiConditionAdd = drawmulti && GUIUtils.Button(new GUIContent("Add"), true, GUIStyles.miniButtonLeft);
                    multiConditionReplace = drawmulti && GUIUtils.Button(new GUIContent("Replace"), true, GUIStyles.miniButtonRight);
                }
                EditorGUILayout.EndHorizontal();

                if (showConditions) {

                    int deleteIndex = -1;
                    int l = conditions.arraySize;
                    for (int i = 0; i < l; i++) {
                        bool delete;
                        CustomParameterEditor.GUI.DrawParamsList(conditions[i][paramsField], false, new GUIContent("D", "Delete Condition"), out delete);
                        if (delete) deleteIndex = i;    
                    }
                    if (deleteIndex >= 0) conditions.DeleteAt(deleteIndex);           
                }
            }
        }
    }
}