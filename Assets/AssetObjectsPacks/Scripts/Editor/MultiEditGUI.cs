using UnityEditor;
using UnityEngine;
namespace AssetObjectsPacks {
    public static class MultiEditGUI {
        static readonly GUIContent multiEditGUI = new GUIContent("<b>Multi-Object Editing</b>","");
        
        public static int DrawMultiEditGUI (
            EditorProp multiAO, 
            GUIContent[] labels, 
            GUILayoutOption[] paramWidths, 
            out bool showParamsChanged, 
            out bool showConditionsChanged, 
            out bool multiConditionAdd, 
            out bool multiConditionReplace
        ) {
            GUIUtils.StartBox(0);

            GUIUtils.Label(multiEditGUI, false);

            int setProp = -1;
            //EditorGUILayout.BeginHorizontal();
            //GUIUtils.SmallButtonClear();
            //GUIUtils.SmallButtonClear();
            //EditorGUILayout.EndHorizontal();

            GUIUtils.BeginIndent(2);
            GUIUtils.EndIndent();

            AssetObjectEditor.GUI.DrawAssetObjectMultiEditView(out showConditionsChanged, out showParamsChanged, multiAO, multiEditGUI, paramWidths, out multiConditionAdd, out multiConditionReplace, out setProp, labels);
            
            GUIUtils.EndBox(1);

            return setProp;
        }




        

        
    }

}
