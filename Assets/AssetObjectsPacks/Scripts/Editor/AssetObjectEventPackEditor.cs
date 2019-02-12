using UnityEngine;
using UnityEditor;

namespace AssetObjectsPacks {
    [CustomEditor(typeof(AssetObjectEventPack))]
    public class AssetObjectEventPackEditor : Editor {

        new AssetObjectEventPack target;
        string pack_help_string = "";
        AssetObjectPacks event_defs_object;   
        PopupList.InputData eventTypesPopupData;
        bool current_pack_explorer_valid;
        AssetObjectListGUI oe;
        GUIContent pack_type_gui;

        
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

        
        public override void OnInspectorGUI() {
            //base.OnInspectorGUI();

            GUIUtils.StartCustomEditor();
            bool force_change = false;
            if (event_defs_object != null) {            
                if (GUIUtils.Button(pack_type_gui, true)) GUIUtils.ShowPopUpAtMouse(eventTypesPopupData, false, false); 
                if (current_pack_explorer_valid)
                    force_change = oe.Draw();
                else
                    EditorGUILayout.HelpBox(pack_help_string, MessageType.Error);
            }
            GUIUtils.EndCustomEditor(this, force_change);

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
                    
                    //serializedObject.FindProperty(AssetObjectEventPack.multi_edit_instance_field);
                    serializedObject.FindProperty(AssetObjectEventPack.asset_objs_field).ClearArray();
                    serializedObject.FindProperty(AssetObjectEventPack.pack_id_field).intValue = event_defs_object.packs[i].id;
                    serializedObject.ApplyModifiedProperties();
                
                    InitializeObjectExplorer();//!current_pack_explorer_valid);   
                    RepopulatePopupList ();
                    break;
                }
            }
        }
       
       /*
        void OnDisable () {
            if (oe != null && current_pack_explorer_valid) {
                oe.OnDisable();
                serializedObject.ApplyModifiedProperties();
            }
        }
        */
        
        void InitializeObjectExplorer(){//bool skip_disable = false) {

            if (oe == null) oe = new AssetObjectListGUI();
            //else {
                //if (!skip_disable) {
                    //oe.OnDisable();
                    //serializedObject.ApplyModifiedProperties();
                //}
            //}

            int pack_id = serializedObject.FindProperty(AssetObjectEventPack.pack_id_field).intValue;
            AssetObjectPack pack = AssetObjectsManager.instance.packs.FindPackByID(pack_id);
            if (pack == null) {
                current_pack_explorer_valid = false;
                pack_help_string = "Please Choose a Pack Type";
                pack_type_gui = new GUIContent("Pack Type");
                return;
            }
            current_pack_explorer_valid = pack.assetType.IsValidTypeString() && pack.objectsDirectory.IsValidDirectory();
            pack_type_gui = new GUIContent("Pack Type: " + pack.name);
            if (current_pack_explorer_valid) {
                pack_help_string = "";
                oe.OnEnable(serializedObject, pack);
                serializedObject.ApplyModifiedProperties();
            }
            else {
                if (!pack.assetType.IsValidTypeString()) pack_help_string = pack.name + " pack doesnt have a valid asset type to target!";
                else if (!pack.assetType.IsValidDirectory()) pack_help_string = pack.name + " pack doesnt have a valid object directory!";
            }
            
        }
    }
}