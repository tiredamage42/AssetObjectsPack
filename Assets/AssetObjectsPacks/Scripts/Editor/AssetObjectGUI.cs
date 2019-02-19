using UnityEngine;
using UnityEditor;

namespace AssetObjectsPacks {

    public static class AssetObjectGUI 
    {
        static readonly GUIContent showConditionsGUI = new GUIContent("C", "Show Conditions");
        static readonly GUIContent deleteConditionParameterGUI = new GUIContent("D", "Delete Parameter");
        static readonly GUIContent addParameterToConditionalGUI = new GUIContent("Add Parameter +");
        static readonly GUIContent deleteConditionalGUI = new GUIContent("D", "Delete Condition");
        static readonly GUIContent addConditionGUI = new GUIContent("Add Condition +");
        static readonly GUILayoutOption paramWidth = GUILayout.Width(128);
        
        public static bool DrawAssetObject (EditorProp ao, bool complex, GUIContent lbl, bool selected, bool hidden, GUILayoutOption elementWidth, GUILayoutOption[] paramWidths) {

            EditorGUILayout.BeginHorizontal();
            bool showCondition = false;
            if (complex) {
                EditorProp showProp = ao[AssetObject.showConditionsField];
                bool changed;
                showCondition = GUIUtils.SmallToggleButton(showConditionsGUI, showProp.boolValue, out changed);
                if (changed) showProp.SetValue ( showCondition );
            }

            bool selectedElement = GUIUtils.ScrollWindowElement (lbl, selected, hidden, false, elementWidth);                    

            if (complex) DrawParameterFields(ao, paramWidths);
            
            EditorGUILayout.EndHorizontal();

            if (showCondition) DrawConditions(ao);

            return selectedElement;
        }

        static void DrawParameterFields (EditorProp ao, GUILayoutOption[] paramWidths) {
            EditorProp parameters = ao[AssetObject.params_field];
            int l = parameters.arraySize;  
            
            for (int i = 0; i < l; i++) {
                GUIUtils.DrawProp( AOParameters.GetParamProperty( parameters[i] ), GUIUtils.blank_content, paramWidths[i]);
                GUIUtils.SmallButtonClear();   
            }
        }
        
        static void DrawConditions (EditorProp ao) {

            EditorProp conditions = ao[AssetObject.conditionChecksField];

            GUIUtils.BeginIndent();

            GUIUtils.StartBox (0, EditorColors.dark_color);
            
            EditorGUILayout.BeginHorizontal();

            if (GUIUtils.Button(addConditionGUI, true, EditorStyles.miniButton)) {
                EditorProp newCondition = conditions.AddNew();
                newCondition[AssetObject.paramsToMatchField].Clear();
                DefaultifyConditionParam(newCondition[AssetObject.paramsToMatchField].AddNew());                    
            }
            
            EditorGUILayout.EndHorizontal();


            int deleteIndex = -1;

            int l = conditions.arraySize;
            for (int i = 0; i < l; i++) {
                bool delete;
                DrawCondition(conditions[i], out delete);
                if(delete) deleteIndex = i;
            }
            if (deleteIndex >= 0) conditions.DeleteAt(deleteIndex);
            
            
            GUIUtils.EndBox (1);

            GUIUtils.EndIndent();            
        }
        static void DrawCondition (EditorProp condition, out bool delete) {
            
            EditorProp parameters = condition[AssetObject.paramsToMatchField];

            GUIUtils.StartBox (0);

            EditorGUILayout.BeginHorizontal();
            delete = GUIUtils.SmallButton(deleteConditionalGUI, EditorColors.red_color, EditorColors.white_color);
            if (GUIUtils.Button(addParameterToConditionalGUI, true, EditorStyles.miniButton)) DefaultifyConditionParam(parameters.AddNew());
            
            EditorGUILayout.EndHorizontal();

            GUIUtils.BeginIndent();

            int deleteIndex = -1;
            int l = parameters.arraySize;
            for (int x = 0; x < l; x++) {
                bool d;
                DrawConditionParameter(parameters[x], out d);
                if(d) deleteIndex = x;
            }
            if (deleteIndex >= 0) parameters.DeleteAt(deleteIndex);
            GUIUtils.EndIndent();
            GUIUtils.EndBox(0);
        }

        static void DefaultifyConditionParam (EditorProp parameter) {
            parameter[CustomParameter.nameField].SetValue( "Parameter Name" );
        }
        static void DrawConditionParameter(EditorProp parameter, out bool delete) {
            EditorGUILayout.BeginHorizontal();
        
            delete = GUIUtils.SmallButton(deleteConditionParameterGUI, EditorColors.red_color, EditorColors.white_color);
            
            GUIUtils.NextControlOverridesKeyboard();
            GUIUtils.DrawProp(parameter[CustomParameter.nameField], GUIContent.none, paramWidth );
            GUIUtils.CheckLoseFocusLastRect();
            
            GUIUtils.DrawProp(parameter[CustomParameter.typeField], GUIContent.none, paramWidth ) ;
            GUIUtils.DrawProp(AOParameters.GetParamProperty( parameter ), GUIContent.none, paramWidth );
            
            EditorGUILayout.EndHorizontal();
        }  
    }
}