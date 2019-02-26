using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace AssetObjectsPacks {

    public static class AssetObjectEditor {
        const string objRefField = "objRef";
        const string idField = "id", paramsField = "parameters";
        //const string conditionsField = "conditions";
        //const string conditionsBlockField = "conditionsBlock";
        public static void MakeAssetObjectDefault (EditorProp ao, int packIndex, bool clear) {
            PackEditor.AdjustParametersToPack(ao[paramsField], packIndex, clear);

            //if (clear) {
            //    ao[conditionsBlockField].SetValue(string.Empty);
            
                //ao[conditionsField].Clear();
            //}
        }



        public static string GetName (EditorProp ao) {
            return ao[objRefField].objRefValue.name;
        }
        public static int GetID(EditorProp ao) {
            return ao[idField].intValue;
        }

        public static void CopyAssetObject(EditorProp ao, EditorProp toCopy) {
            Debug.Log("Copying id");
            
            ao[idField].CopyProp ( toCopy[idField] );
            Debug.Log("Copying obj ref");
            ao[objRefField].CopyProp ( toCopy[objRefField] );

            
            Debug.Log("Copying params");
            CustomParameterEditor.ClearAndRebuildParameters(ao[paramsField], toCopy[paramsField]);
            

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
        //public static void CopyConditions(IEnumerable<EditorProp> aos, EditorProp aoCopy, bool additive) {
            //foreach (EditorProp ao in aos) ao[conditionsBlockField].CopyProp(aoCopy[conditionsBlockField]);//, additive);            
            
            //foreach (EditorProp ao in aos) ConditionsEditor.CopyConditions(ao[conditionsField], aoCopy[conditionsField], additive);            
            //aoCopy[conditionsField].Clear();
            //aoCopy[conditionsBlockField].SetValue(string.Empty);

        //}
        
        public static class GUI {    
          
            
            static GUILayoutOption conditionWidth = GUILayout.Width(360);
            
            
            public static void DrawAssetObjectMultiEditView (
                bool drawingSingle,
                EditorProp ao, GUILayoutOption[] paramWidths, 
                //out bool addMulti, 
                //out bool replaceMulti, 
                out bool setMultiCondition, 
                
                out int setProp, GUIContent[] paramLabels) {
                setProp = -1;
                
                GUIUtils.BeginIndent();
                
                EditorGUILayout.BeginHorizontal();

                
                int l = paramLabels.Length;                
                for (int i = 0; i < l; i++) {
                    GUIUtils.Label(paramLabels[i], true);
                    GUIUtils.SmallButtonClear();
                }

                EditorGUILayout.EndHorizontal();
                
                DrawParamsAndConditionsAO (ao, !drawingSingle, paramWidths, out setMultiCondition, out setProp);
                //DrawParamsAndConditionsAO (ao, !drawingSingle, paramWidths, out addMulti, out replaceMulti, out setProp);
                
                GUIUtils.EndIndent();
            }

            
            //static void DrawParamsAndConditionsAO (EditorProp ao, bool drawMultiSet, GUILayoutOption[] paramWidths, out bool addMulti, out bool replaceMulti, out int multiParamSet) {
            static void DrawParamsAndConditionsAO (EditorProp ao, bool drawMultiSet, GUILayoutOption[] paramWidths, out bool setMultiCondition, out int multiParamSet) {

                //addMulti = replaceMulti = false;
                multiParamSet = -1;
                setMultiCondition = false;
                
                CustomParameterEditor.GUI.DrawAOParameters(ao[paramsField], paramWidths, drawMultiSet, out multiParamSet);                            
                    
                //GUIUtils.StartBox (0, Colors.darkGray);//, conditionWidth);


                //GUIUtils.Label(new GUIContent("Condition Block"), true);

                //EditorGUILayout.BeginHorizontal();
                //bool changedCondition = GUIUtils.DrawTextProp(ao[conditionsBlockField], false);

                //setMultiCondition = false;
                //if (drawMultiSet){
                //    setMultiCondition = GUIUtils.SmallButton(new GUIContent("S","Set Values"));
                //}
                //else {
                //    GUIUtils.SmallButtonClear();   
                //}
                //EditorGUILayout.EndHorizontal();
                
                
                //ConditionsEditor.GUI.DrawConditions(
                //    ao[conditionsField], drawMultiSet, out addMulti, out replaceMulti
                //);
                
                
                //GUIUtils.EndBox (0);
                
            }       
        }
    }
}