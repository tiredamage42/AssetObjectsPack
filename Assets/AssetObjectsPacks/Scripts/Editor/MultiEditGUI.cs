using UnityEditor;
using UnityEngine;
namespace AssetObjectsPacks {
    public static class MultiEditGUI {
        static readonly GUIContent setValuesGUI = new GUIContent("S","Set Values");
        public static int DrawMultiEditGUI (EditorProp multiAO, GUIContent[] labels, GUILayoutOption elementWidth, GUILayoutOption[] paramWidths) {
            GUIUtils.StartBox(0);

            int setProp = -1;
            EditorGUILayout.BeginHorizontal();

            GUIUtils.SmallButtonClear();
            GUIUtils.ScrollWindowElement (GUIUtils.blank_content, false, false, false, elementWidth);    
            
            int l = labels.Length;
            for (int i = 0; i < l; i++) {
                GUIUtils.Label(labels[i], true);
                if (GUIUtils.SmallButton(setValuesGUI, EditorColors.selected_color, EditorColors.selected_text_color)) setProp = i;
            }
            
            EditorGUILayout.EndHorizontal();

            AssetObjectGUI.DrawAssetObject(multiAO, true, GUIUtils.blank_content, false, false, elementWidth, paramWidths);
                        
            GUIUtils.EndBox(0);

            return setProp;
        }




        

        
    }

}
