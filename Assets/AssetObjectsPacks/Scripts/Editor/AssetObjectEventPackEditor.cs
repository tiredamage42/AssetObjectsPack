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
            //base.OnInspectorGUI();
            EditorGUI.BeginChangeCheck();
            
            EditorGUILayout.Space();
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

        void RepopulatePopupList () {
            int l = event_defs_object.packs.Length;
            eventTypesPopupData = new PopupList.InputData { m_OnSelectCallback = OnSwitchPackCallback, m_MaxCount = l };
            for (int i = 0; i < l; i++) {
                PopupList.ListElement element = eventTypesPopupData.NewOrMatchingElement(event_defs_object.packs[i].name);
                element.selected = target.assetObjectPackID == event_defs_object.packs[i].id;
            }
        }

        public void OnSwitchPackCallback(PopupList.ListElement element, bool little_button_pressed) {
            int l = event_defs_object.packs.Length;
            for (int i = 0; i < l; i++) {
                if (event_defs_object.packs[i].name == element.text) {
                    SwitchPackType(event_defs_object.packs[i].id);
                    serializedObject.ApplyModifiedProperties();
                    InitializeObjectExplorer(!current_pack_explorer_valid);   
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
                if (current_pack_explorer_valid) {
                    pack_help_string = "";
                    oe.OnEnable(serializedObject, pack);
                    serializedObject.ApplyModifiedProperties();
                }
                else {
                    if (!pack.assetType.IsValidTypeString()) pack_help_string = pack.name + " pack doesnt have a valid asset type to target!";
                    else if (!pack.assetType.IsValidDirectory()) pack_help_string = pack.name + " pack doesnt have a valid object directory!";
                }
                return;
            }
            Debug.LogWarning("choose a pack type");
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