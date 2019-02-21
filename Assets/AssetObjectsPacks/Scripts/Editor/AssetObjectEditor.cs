using UnityEngine;
using UnityEditor;

namespace AssetObjectsPacks {

    public static class AssetObjectEditor {
        public static class GUI 
        {

            public static bool AssetObjectDirectoryElement (GUIContent gui, bool selected) {
                GUIStyle s = GUIStyles.toolbarButton;
                TextAnchor a = s.alignment;
                s.alignment = TextAnchor.MiddleLeft;
                bool pressed = GUIUtils.Button(gui, false, s, EditorColors.ToggleBackgroundColor(selected), EditorColors.black);
                s.alignment = a;
                return pressed;
            }

            static bool AssetObjectElement (GUIContent gui, bool selected, bool hidden) {
                GUIStyle s = GUIStyles.toolbarButton;
                TextAnchor a = s.alignment;
                Texture2D t = s.normal.background;

                s.fontStyle = hidden ? FontStyle.Italic : FontStyle.Normal;
                s.normal.background = (!selected) ? null : t;
                s.alignment = TextAnchor.MiddleLeft;

                Color32 txtColor = hidden ? EditorColors.yellow : (selected ? EditorColors.black : EditorColors.liteGray);
                bool pressed = GUIUtils.Button(gui, false, s, EditorColors.ToggleBackgroundColor(selected), txtColor);
                s.normal.background = t;
                s.alignment = a;
                s.fontStyle = FontStyle.Normal;
                
                return pressed;
            }

            
            static readonly GUIContent showConditionsGUI = new GUIContent("C", "Show Conditions");
            static readonly GUIContent showParamsGUI = new GUIContent("P", "Show Parameters");
            static readonly GUIContent addParameterToConditionalGUI = new GUIContent("Add Parameter +");
            static readonly GUIContent deleteConditionalGUI = new GUIContent("D", "Delete Condition");
            static readonly GUIContent addConditionGUI = new GUIContent("Add Condition +");
            static readonly GUIContent setValuesGUI = new GUIContent("S","Set Values");

            public static bool DrawAssetObjectProjectView (GUIContent lbl, bool selected, bool hidden) {
                return AssetObjectElement (lbl, selected, hidden);                    
            }

            static bool ToggleProp (EditorProp prop, GUIContent c, out bool changed) {
                bool value = GUIUtils.SmallToggleButton(c, prop.boolValue, out changed);
                if (changed) prop.SetValue ( value );
                return value;
            }
            public static void DrawAssetObjectMultiEditView (out bool showConditionChanged, out bool showParamsChanged, EditorProp ao, GUIContent lbl, GUILayoutOption[] paramWidths, out bool addMulti, out bool replaceMulti, out int setProp, GUIContent[] paramLabels) {
                setProp = -1;
                showConditionChanged = false;
                showParamsChanged = false;
                
                EditorGUILayout.BeginHorizontal();

                bool showConditions = ToggleProp(ao[AssetObject.showConditionsField], showConditionsGUI, out showConditionChanged);
                bool showParameters = ToggleProp(ao[AssetObject.showParamsField], showParamsGUI, out showParamsChanged);
                
                int l = paramLabels.Length;                
                for (int i = 0; i < l; i++) {
                    GUIUtils.Label(paramLabels[i], true);
                    UnityEngine.GUI.enabled = showParameters;
                    if (GUIUtils.SmallButton(setValuesGUI)) setProp = i;
                    UnityEngine.GUI.enabled = true;
                }
                
                EditorGUILayout.EndHorizontal();
                DrawParamsAndConditionsAO (ao, showParameters, showConditions, true, paramWidths, out addMulti, out replaceMulti);
            }



            
            public static bool DrawAssetObjectEventView (EditorProp ao, GUIContent lbl, bool selected, bool hidden, GUILayoutOption[] paramWidths) {
                EditorGUILayout.BeginHorizontal();
                bool showConditions = ToggleProp(ao[AssetObject.showConditionsField], showConditionsGUI, out _);
                bool showParameters = ToggleProp(ao[AssetObject.showParamsField], showParamsGUI, out _);
                bool selectedElement = AssetObjectElement (lbl, selected, hidden);                    
                EditorGUILayout.EndHorizontal();
                DrawParamsAndConditionsAO (ao, showParameters, showConditions, false, paramWidths, out bool _, out bool _);
                return selectedElement;
            }

            static void DrawParamsAndConditionsAO (EditorProp ao, bool showParameters, bool showConditions, bool drawMultiSet, GUILayoutOption[] paramWidths, out bool addMulti, out bool replaceMulti) {

                addMulti = replaceMulti = false;
                if (showParameters || showConditions) {
                    GUIUtils.BeginIndent(2);
                    
                    GUIUtils.StartBox (0, EditorColors.darkGray, conditionWidth);
                
                    if (showParameters) {
                        EditorProp parameters = ao[AssetObject.params_field];
                        CustomParameterEditor.GUI.DrawAOParameters(parameters, paramWidths);                            
                    }
                    if (showConditions) {
                        EditorProp conditions = ao[AssetObject.conditionChecksField];
                        DrawConditions(conditions, drawMultiSet, out addMulti, out replaceMulti);
                    }

                    GUIUtils.EndBox (0);


                    GUIUtils.EndIndent();            
                }
            }

            static GUILayoutOption conditionWidth = GUILayout.Width(360);
            
            static void DrawConditions (EditorProp conditions, bool drawmulti, out bool multiConditionAdd, out bool multiConditionReplace) {

                //GUIUtils.StartBox (0, EditorColors.darkGray, conditionWidth);
                
                EditorGUILayout.BeginHorizontal();

                if (GUIUtils.Button(addConditionGUI, true, GUIStyles.miniButton)) {
                    EditorProp newParamsList = conditions.AddNew()[AssetObject.paramsToMatchField];
                    newParamsList.Clear();
                    CustomParameterEditor.MakeParamDefault(newParamsList.AddNew());                    
                }

                GUILayout.FlexibleSpace();
                multiConditionAdd = drawmulti && GUIUtils.Button(new GUIContent("Add"), true, GUIStyles.miniButtonLeft);
                multiConditionReplace = drawmulti && GUIUtils.Button(new GUIContent("Replace"), true, GUIStyles.miniButtonRight);
                EditorGUILayout.EndHorizontal();

                int deleteIndex = -1;
                int addParamIndex = -1;

                int deleteParamConditionIndex = -1;
                int deleteParamIndex = -1;

                int l = conditions.arraySize;
                for (int i = 0; i < l; i++) {
                    bool delete, addParameter;
                    int dParamI;
                    DrawCondition(conditions[i], out delete, out addParameter, out dParamI);
                    if (delete) deleteIndex = i;
                    if (addParameter) addParamIndex = i;
                    if (dParamI >= 0) {
                        deleteParamConditionIndex = i;
                        deleteParamIndex = dParamI;
                    }
                }

                if (deleteIndex >= 0) {
                    conditions.DeleteAt(deleteIndex);
                }
                if (addParamIndex >= 0) {
                    CustomParameterEditor.MakeParamDefault(conditions[addParamIndex][AssetObject.paramsToMatchField].AddNew());
                }
                if (deleteParamConditionIndex > 0) {
                    conditions[deleteParamConditionIndex][AssetObject.paramsToMatchField].DeleteAt(deleteParamIndex);
                }
                
                //GUIUtils.EndBox (0);
            }
            static void DrawCondition (EditorProp condition, out bool deleteCondition, out bool addParameter, out int deleteParamIndex) {
                
                EditorProp parameters = condition[AssetObject.paramsToMatchField];
                GUIUtils.StartBox (0);
                EditorGUILayout.BeginHorizontal();
                deleteCondition = GUIUtils.SmallButton(deleteConditionalGUI, EditorColors.red, EditorColors.white);
                addParameter = GUIUtils.Button(addParameterToConditionalGUI, true, GUIStyles.miniButton);
                EditorGUILayout.EndHorizontal();
                GUIUtils.BeginIndent();
                CustomParameterEditor.GUI.DrawParamsList(parameters, false, out _, out deleteParamIndex);
                GUIUtils.EndIndent();
                GUIUtils.EndBox(1);
            }
        }
    }

}