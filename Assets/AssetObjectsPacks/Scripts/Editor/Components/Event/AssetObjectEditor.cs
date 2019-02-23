using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace AssetObjectsPacks {

    public static class AssetObjectEditor {
        const string objRefField = "objRef";
        const string idField = "id", paramsField = "parameters";
        const string conditionsField = "conditions";
        
        public static void MakeAssetObjectDefault (EditorProp ao, int packIndex, bool clear) {
            PackEditor.AdjustParametersToPack(ao[paramsField], packIndex, clear);
            if (clear) ao[conditionsField].Clear();
        }

        public static int GetID(EditorProp ao) {
            return ao[idField].intValue;
        }
        public static void InitializeNewAssetObject (EditorProp ao, int obj_id, UnityEngine.Object obj_ref, bool make_default, int packIndex) {
            ao[idField].SetValue ( obj_id );
            ao[objRefField].SetValue ( obj_ref );
            //only need to default first one added, the rest will copy the last one 'inserted' into the
            //serialized property array
            if (!make_default) return;
            MakeAssetObjectDefault(ao, packIndex, true);
        }

        public static void CopyParameters(IEnumerable<EditorProp> aos, EditorProp aoCopy, int paramIndex) {
            foreach (EditorProp ao in aos) CustomParameterEditor.CopyParameter (ao[paramsField][paramIndex], aoCopy[paramsField][paramIndex] );      
        }
        public static void CopyConditions(IEnumerable<EditorProp> aos, EditorProp aoCopy, bool additive) {
            foreach (EditorProp ao in aos) ConditionsEditor.CopyConditions(ao[conditionsField], aoCopy[conditionsField], additive);            
            aoCopy[conditionsField].Clear();
        }
        
        public static class GUI {    
          
            
            static readonly GUIContent showConditionsGUI = new GUIContent("C", "Show Conditions");
            static readonly GUIContent showParamsGUI = new GUIContent("P", "Show Parameters");
            static GUILayoutOption conditionWidth = GUILayout.Width(360);
            
            static bool ToggleProp (EditorProp prop, GUIContent c, out bool changed) {
                bool value = GUIUtils.SmallToggleButton(c, prop.boolValue, out changed);
                if (changed) prop.SetValue ( value );
                return value;
            }

            public static void DrawAssetObjectMultiEditView (
                bool drawingSingle,
                //out bool showConditionChanged, 
                //out bool showParamsChanged, 
                EditorProp ao, GUILayoutOption[] paramWidths, out bool addMulti, out bool replaceMulti, out int setProp, GUIContent[] paramLabels) {
                setProp = -1;
                //showConditionChanged = false;
                //showParamsChanged = false;

                GUIUtils.BeginIndent();
                
                EditorGUILayout.BeginHorizontal();

                //showConditions = GUIUtils.SmallToggleButton(showConditionsGUI, showConditions, out _);
                //showParameters = GUIUtils.SmallToggleButton(showParamsGUI, showParameters, out _);

                //bool showConditions = ToggleProp(ao[showConditionsField], showConditionsGUI, out showConditionChanged);
                //bool showParameters = ToggleProp(ao[showParamsField], showParamsGUI, out showParamsChanged);
                
                //EditorGUILayout.BeginHorizontal(GUILayout.Width(8));
                //EditorGUILayout.EndHorizontal();
                
                int l = paramLabels.Length;                
                for (int i = 0; i < l; i++) {
                    GUIUtils.Label(paramLabels[i], true);
                    GUIUtils.SmallButtonClear();
                }

                EditorGUILayout.EndHorizontal();
                
                DrawParamsAndConditionsAO (ao, !drawingSingle, paramWidths, out addMulti, out replaceMulti, out setProp);
                GUIUtils.EndIndent();
            }

/*
            public static bool DrawAssetObjectEventView (EditorProp ao, GUIContent lbl, bool selected, bool hidden, GUILayoutOption[] paramWidths) {
                //EditorGUILayout.BeginHorizontal();


                //bool showConditions = ToggleProp(ao[showConditionsField], showConditionsGUI, out _);
                //bool showParameters = ToggleProp(ao[showParamsField], showParamsGUI, out _);
                bool selectedElement = ElementSelectionSystem.SelectionSystemElementGUI (lbl, selected, hidden, false);                    
                //EditorGUILayout.EndHorizontal();
                //DrawParamsAndConditionsAO (ao, showParameters, showConditions, false, paramWidths, out _, out _, out _);
                return selectedElement;
            }
*/
            
            static void DrawParamsAndConditionsAO (EditorProp ao, bool drawMultiSet, GUILayoutOption[] paramWidths, out bool addMulti, out bool replaceMulti, out int multiParamSet) {

                addMulti = replaceMulti = false;
                multiParamSet = -1;
                //if (showParameters || showConditions) {
                    //GUIUtils.BeginIndent(1);



                    //if (showParameters) 
                    CustomParameterEditor.GUI.DrawAOParameters(ao[paramsField], paramWidths, drawMultiSet, out multiParamSet);                            
                    
                    

                
                    //if (showParameters) CustomParameterEditor.GUI.DrawAOParameters(ao[paramsField], paramWidths, drawMultiSet, out multiParamSet);                            
                    GUIUtils.StartBox (0, Colors.darkGray, conditionWidth);
                    //EditorGUILayout.BeginHorizontal();        
                    //showConditions = GUIUtils.SmallToggleButton(showConditionsGUI, showConditions, out _);
                
                   // if (showConditions) {

                        ConditionsEditor.GUI.DrawConditions(ao[conditionsField], drawMultiSet, out addMulti, out replaceMulti);

                    //}
                    //EditorGUILayout.EndHorizontal();
                    GUIUtils.EndBox (0);
                
                    //GUIUtils.EndIndent();    

                //}
            }       
        }
    }
}