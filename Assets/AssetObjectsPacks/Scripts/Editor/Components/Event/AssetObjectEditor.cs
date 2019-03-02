using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace AssetObjectsPacks {

    public static class AssetObjectEditor {
        const string objRefField = "objRef", idField = "id", paramsField = "parameters", isCopyField = "isCopy";
        public static void MakeAssetObjectDefault (EditorProp ao, int packIndex, bool clear) {
            PackEditor.AdjustParametersToPack(ao[paramsField], packIndex, clear);
        }
        public static int GetID(EditorProp ao) {
            return ao[idField].intValue;
        }
        public static bool GetIsCopy(EditorProp ao) {
            return ao[isCopyField].boolValue;
        }
        public static void CopyAssetObject(EditorProp ao, EditorProp toCopy) {    
            ao.CopySubProps ( toCopy, new string[] { idField, objRefField } );
            CustomParameterEditor.CopyParameterList(ao[paramsField], toCopy[paramsField]);
        }
        public static void DuplicateAO (EditorProp aoList, int atIndex) {
            EditorProp newAO = aoList.InsertAtIndex(atIndex + 1);
            CopyAssetObject(newAO, aoList[atIndex]);
            newAO[isCopyField].SetValue(true);
        }
        public static void InitializeNewAssetObject (EditorProp ao, int id, Object obj, bool makeDefault, int packIndex) {
            ao[idField].SetValue ( id );
            ao[objRefField].SetValue ( obj );
            //only need to default first one added, the rest will copy the last one 'inserted' into the
            //serialized property array
            if (!makeDefault) return;
            MakeAssetObjectDefault(ao, packIndex, true);
        }
        public static void CopyParameters(IEnumerable<EditorProp> aos, EditorProp aoCopy, int paramIndex) {
            foreach (EditorProp ao in aos) CustomParameterEditor.CopyParameter (ao[paramsField][paramIndex], aoCopy[paramsField][paramIndex] );      
        }
        public static class GUI {    
            public static void DrawAssetObjectEdit (EditorProp ao, bool drawingSingle, GUIContent[] paramLabels, GUILayoutOption[] paramWidths, out int setParam) {                
                GUIUtils.BeginIndent();

                setParam = -1;
                GUIContent multiSetGUI = new GUIContent("S", "Set Values");
                //labels
                EditorGUILayout.BeginHorizontal();
                for (int i = 0; i < paramLabels.Length; i++) {
                    GUIUtils.Label(paramLabels[i], paramWidths[i]);
                    //multi set button
                    if (!drawingSingle){
                        if (GUIUtils.SmallButton(multiSetGUI)) setParam = i;
                    }
                    else GUIUtils.SmallButtonClear();
                }
                EditorGUILayout.EndHorizontal();

                //fields
                EditorGUILayout.BeginHorizontal();
                for (int i = 0; i < paramLabels.Length; i++) {
                    GUIUtils.DrawProp( CustomParameterEditor.GetParamValueProperty( ao[paramsField][i] ), paramWidths[i]);
                    GUIUtils.SmallButtonClear();   
                }
                EditorGUILayout.EndHorizontal();
            
                GUIUtils.EndIndent();
            }                   
        }
    }
}