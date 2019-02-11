using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace AssetObjectsPacks {
    [CustomEditor(typeof(AssetObjectEventPack))]
    public class AssetObjectEventPackEditor : Editor
    {

        new AssetObjectEventPack target;
        string pack_help_string = "";

        AssetObjectPacks event_defs_object;
        
        PopupList.InputData eventTypesPopupData;

        bool current_pack_explorer_valid;
        AssetObjectListGUI oe;

        
        public override bool HasPreviewGUI() { return oe != null && current_pack_explorer_valid; }
        public override void OnInteractivePreviewGUI(Rect r, GUIStyle background) { 
            if (oe == null || !current_pack_explorer_valid) return;
            oe.OnInteractivePreviewGUI(r, background); 
        }
        public override void OnPreviewSettings() { 
            if (oe == null || !current_pack_explorer_valid) return;
            oe.OnPreviewSettings(); 
        }
        
        void OnEnable () {
            this.target = base.target as AssetObjectEventPack;
            AssetObjectsManager instance = AssetObjectsManager.instance;
            if (instance != null) {
                event_defs_object = AssetObjectsManager.instance.packs;
                if (event_defs_object != null) {
                    Debug.Log("intializing obejct explorer");
                    InitializeObjectExplorer();
                    RepopulatePopupList();
                }
            }
        }


        void SwitchPackType(int pack_id) {
            //serializedObject.FindProperty(AssetObjectEventPack.multi_edit_instance_field);
            serializedObject.FindProperty(AssetObjectEventPack.hidden_ids_field).ClearArray();
            
            serializedObject.FindProperty(AssetObjectEventPack.asset_objs_field).ClearArray();
            serializedObject.FindProperty(AssetObjectEventPack.pack_id_field).intValue = pack_id;
        }

            

        public override void OnInspectorGUI() {
            bool changed = false;
            base.OnInspectorGUI();
            EditorGUILayout.Space();
            
            EditorGUI.BeginChangeCheck();
        
            if (event_defs_object != null) {
                EditorGUILayout.Space();
                EditorGUILayout.Space();

    
                EditorGUILayout.BeginHorizontal();
                DrawPopupButton(new GUIContent("Pack Type"), eventTypesPopupData, false, false);
    
                EditorGUILayout.EndHorizontal();
                changed = DrawAssetObjectEvent();
            }
            if (EditorGUI.EndChangeCheck() || changed) {                
                EditorUtility.SetDirty(target);
                serializedObject.ApplyModifiedProperties();
            }
        }



       

        public void RepopulatePopupList () {
            eventTypesPopupData = new PopupList.InputData { m_OnSelectCallback = AssetLabelListCallback, m_MaxCount = event_defs_object.packs.Count };
            for (int i = 0; i < event_defs_object.packs.Count; i++) {
                PopupList.ListElement element = eventTypesPopupData.NewOrMatchingElement(event_defs_object.packs[i].name);
                element.selected = target.assetObjectPackID == event_defs_object.packs[i].id;
            }
        }

        
        public void AssetLabelListCallback(PopupList.ListElement element, bool little_button_pressed) {
            for (int i = 0; i < event_defs_object.packs.Count; i++) {
                if (event_defs_object.packs[i].name == element.text) {

                    SwitchPackType(event_defs_object.packs[i].id);
                    InitializeObjectExplorer(!current_pack_explorer_valid);   
                    serializedObject.ApplyModifiedProperties();
                    RepopulatePopupList ();
            
                    break;
                }
            }
        }
       
        
        void DrawPopupButton(GUIContent label, PopupList.InputData popup_input, bool draw_search, bool draw_remove) {
            EditorGUILayout.BeginHorizontal();            
            if (GUILayout.Button(label)) {
                GUIUtils.ShowPopUpAtMouse(popup_input, draw_search, draw_remove);
            }
            EditorGUILayout.EndHorizontal();
        }
        void OnDisable () {
            if (oe != null && current_pack_explorer_valid) {
                
                oe.OnDisable();
                serializedObject.ApplyModifiedProperties();

            }
        }
        
        void InitializeObjectExplorer(bool skip_disable = false) {

            if (oe == null) oe = new AssetObjectListGUI();
            else {
                if (!skip_disable) {

                    oe.OnDisable();
                    serializedObject.ApplyModifiedProperties();
                }
            }

            int pack_id = serializedObject.FindProperty(AssetObjectEventPack.pack_id_field).intValue;

                AssetObjectPack pack = AssetObjectsManager.instance.packs.FindPackByID(pack_id);

                if (pack != null) {

                    current_pack_explorer_valid = pack.assetType.IsValidTypeString() && pack.objectsDirectory.IsValidDirectory();

                    Debug.Log(pack.name + " wafdsfsdfs");
                    if (current_pack_explorer_valid) {
                        pack_help_string = "";
                        Debug.Log("enabled oe 0");


                        oe.OnEnable(serializedObject, pack);

                        Debug.Log("enabled oe");
                        serializedObject.ApplyModifiedProperties();
                    }
                    else {
                        if (!pack.assetType.IsValidTypeString()) {
                            pack_help_string = pack.name + " pack doesnt have a valid asset type to target!";
                        }
                        else if (!pack.assetType.IsValidDirectory()) {
                            pack_help_string = pack.name + " pack doesnt have a valid object directory!";
                        }
                    }
                }
                else {
                    Debug.LogWarning("choose a pack type");
                }

        }
        bool DrawAssetObjectEvent () {
            
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            if (current_pack_explorer_valid) return oe.Draw();

            EditorGUILayout.HelpBox(pack_help_string, MessageType.Error);
            return false;
        }


    }

}



















