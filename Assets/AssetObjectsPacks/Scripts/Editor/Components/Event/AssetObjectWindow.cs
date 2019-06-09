using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;

namespace AssetObjectsPacks {
 
    public class AssetObjectWindow : EditorWindow
    {

        const int parameterYSize = 16;
        const int minSizeWithoutParameters = 256;

        GUILayoutOption parameterLabelWidth = GUILayout.Width(150);

        [SerializeField] AssetObject multiEditAssetObject;  
        EditorProp multiEditAssetObjectProp;

        GUIContent[] parameterLabels;

        GUIContent multiSetValueGUI = new GUIContent("", "Set Values");
        GUIContent sendMessagesGUI = new GUIContent("Send Messages On Play");
        GUIContent conditionsGUI = new GUIContent("Conditions");
        GUIContent noAOsGUI = new GUIContent("No Asset Objects selected...");
        GUIContent selectEditGUI = new GUIContent("Multi-Edit <b>Selected</b> Objects");
        GUIContent closeGUI = new GUIContent("Close");

        ElementSelectionSystem selectionSystem;
        AOStateMachineEditor stateMachineEditor;
        EditorProp currentState;
        int multiSetParameterIndex = -1;
        
        
        bool CloseIfError () {
            if (this.stateMachineEditor == null){
                return true;
            }
            return false;
        }



        AssetObjectWindow InitializeInternal (AOStateMachineEditor stateMachineEditor, EditorProp packProp, ElementSelectionSystem selectionSystem) {

            
            this.stateMachineEditor = stateMachineEditor;
            this.selectionSystem = selectionSystem;

            multiEditAssetObjectProp = new EditorProp (new SerializedObject(this))["multiEditAssetObject"];
            PacksManagerEditor.UpdateAssetObjectParametersIfDifferentFromDefaults(multiEditAssetObjectProp, packProp, true, true);  
        
            EditorProp aoParameters = multiEditAssetObjectProp[AOStateMachineEditor.paramsField];
            int paramsLength = aoParameters.arraySize;
            
            parameterLabels = paramsLength.Generate( i => new GUIContent(aoParameters[i]["name"].stringValue) ).ToArray(); 
            
            int ySize = minSizeWithoutParameters + (parameterYSize * paramsLength);
            position = new Rect (256, 256, 400, ySize);
            return this;            
        }

        public static AssetObjectWindow ShowAssetObjectWindow(AOStateMachineEditor stateMachineEditor, EditorProp packProp, ElementSelectionSystem selectionSystem)
        {
            return ((AssetObjectWindow)GetWindow(typeof(AssetObjectWindow), true, "Asset Object:", false)).InitializeInternal(stateMachineEditor, packProp, selectionSystem);
        }

        public void SetCurrentState ( EditorProp currentState) {
            this.currentState = currentState;
        }
        
        void DrawAOParameters (bool drawingSingle, EditorProp ao) {
            
            for (int i = 0; i < parameterLabels.Length; i++) {
                EditorGUILayout.BeginHorizontal();
                
                //multi set button
                GUI.enabled = !drawingSingle;
                if (GUIUtils.SmallButton(multiSetValueGUI)) {
                    multiSetParameterIndex = i;
                }
                GUI.enabled = true;
                
                GUIUtils.Label(parameterLabels[i], parameterLabelWidth);
                GUIUtils.DrawProp( CustomParameterEditor.GetParamValueProperty( ao[AOStateMachineEditor.paramsField][i] ));
                
                EditorGUILayout.EndHorizontal();
            }
        }

        void OnDestroy () {
            if (stateMachineEditor != null) {
                stateMachineEditor.showAOWindow = false;
            }
        }

        public void OnGUI()
        {
            multiSetParameterIndex = -1;
                
            if (CloseIfError()) {
                Close();
                return;
            }
            
            GUIUtils.StartBox(1);
            
            if (!selectionSystem.hasSelection){
                GUIUtils.Label(noAOsGUI);
            }
            else {

                bool drawingSingle = selectionSystem.singleSelected;
                
                SelectionElement firstSelected = selectionSystem.firstSelected;
                
                EditorProp assetObjectProp = drawingSingle ? AOStateMachineEditor.GetAOatPoolID(currentState, firstSelected.refIndex) : multiEditAssetObjectProp;
                
                if (assetObjectProp == null) {
                    GUIUtils.Label(noAOsGUI);
                }
                else {
                    // GUIUtils.Label(drawingSingle ? new GUIContent("<b>" + firstSelected.path + "</b>") : selectEditGUI);
                    GUIUtils.Label(drawingSingle ? new GUIContent("<b>" + firstSelected.gui.text + "</b>") : selectEditGUI);
                    
                    GUILayout.FlexibleSpace();
                    
                    GUIUtils.BeginIndent();
                    DrawAOParameters(drawingSingle, assetObjectProp);
                    GUIUtils.EndIndent();
                                    
                    GUILayout.FlexibleSpace();
                    
                    bool setMessages = DrawMessaging (AOStateMachineEditor.messageBlocksField, sendMessagesGUI, assetObjectProp, drawingSingle);
                    bool setConditions = DrawMessaging (AOStateMachineEditor.conditionsBlockField, conditionsGUI, assetObjectProp, drawingSingle);

                    if (!drawingSingle) {
                        if (multiSetParameterIndex != -1) {

                            CopyParameters(
                                selectionSystem.GetReferenceIndiciesInSelectionOrAllShown(false).Generate(i=>AOStateMachineEditor.GetAOatPoolID(currentState, i)), 
                                multiEditAssetObjectProp, 
                                multiSetParameterIndex
                            );
                        }
                        if (setMessages || setConditions) {

                            CopyTextBlock(
                                selectionSystem.GetReferenceIndiciesInSelectionOrAllShown(false).Generate(i=>AOStateMachineEditor.GetAOatPoolID(currentState, i)), 
                                multiEditAssetObjectProp, 
                                setMessages ? AOStateMachineEditor.messageBlocksField : AOStateMachineEditor.conditionsBlockField
                            );
                        }
                    }
                }
            }
            
            GUILayout.FlexibleSpace();
            if (GUIUtils.Button(closeGUI, GUIStyles.button, Colors.blue, Colors.black )) {
                Close();
            }

            GUIUtils.EndBox(1);
            stateMachineEditor.so.SaveObject();
        }

        bool DrawMessaging (string fieldName, GUIContent label, EditorProp ao, bool drawingSingle) {
            EditorGUILayout.BeginHorizontal();
            bool doSet = !drawingSingle && GUIUtils.SmallButton(multiSetValueGUI);
            GUIUtils.Label(label);
            EditorGUILayout.EndHorizontal();
            GUIUtils.DrawMultiLineExpandableString(ao[fieldName], true, label.text, 40);
            return doSet;    
        }

        void CopyParameters(IEnumerable<EditorProp> aos, EditorProp aoCopy, int paramIndex) {
            foreach (var ao in aos) {
                if (ao != null) {
                    CustomParameterEditor.CopyParameter (ao[AOStateMachineEditor.paramsField][paramIndex], aoCopy[AOStateMachineEditor.paramsField][paramIndex] );      
                }
            }
        }
        void CopyTextBlock(IEnumerable<EditorProp> aos, EditorProp aoCopy, string fieldName) {
            foreach (var ao in aos) {
                if (ao != null) {
                    ao[fieldName].SetValue(aoCopy[fieldName].stringValue);
                }
            }
        }
        
        void OnInspectorUpdate()
        {
            Repaint();
        }
    }
}
