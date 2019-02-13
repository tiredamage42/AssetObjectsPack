using UnityEngine;
using UnityEditor;

namespace AssetObjectsPacks {
    [CustomEditor(typeof(AssetObjectEventPack))]
    public class AssetObjectEventPackEditor : Editor {

        new AssetObjectEventPack target;
        AssetObjectPacks event_defs_object;   
        PopupList.InputData packsPopup;    
        AssetObjectListGUI oe = new AssetObjectListGUI();
        GUIContent pack_type_gui;

        public override bool HasPreviewGUI() { return oe.HasPreviewGUI(); }
        public override void OnInteractivePreviewGUI(Rect r, GUIStyle background) { oe.OnInteractivePreviewGUI(r, background); }
        public override void OnPreviewSettings() { oe.OnPreviewSettings(); }
        
        void OnEnable () {
            this.target = base.target as AssetObjectEventPack;
            oe.RealOnEnable(serializedObject);
            AssetObjectsManager instance = AssetObjectsManager.instance;
            if (instance != null) {
                event_defs_object = AssetObjectsManager.instance.packs;
                if (event_defs_object != null) {
                    InitializeObjectExplorer(event_defs_object.FindPackByID(serializedObject.FindProperty(AssetObjectEventPack.pack_id_field).intValue));
                    RepopulatePopupList();
                }
            }
        }
        public override void OnInspectorGUI() {
            //base.OnInspectorGUI();
            GUIUtils.StartCustomEditor();
            bool force_change = false;
            if (event_defs_object != null) {            
                if (GUIUtils.Button(pack_type_gui, true)) GUIUtils.ShowPopUpAtMouse(packsPopup, false, false);     
                force_change = oe.Draw();
            }
            GUIUtils.EndCustomEditor(this, force_change);
        }

        void RepopulatePopupList () {
            int l = event_defs_object.packs.Length;
            packsPopup = new PopupList.InputData { m_OnSelectCallback = OnSwitchPackCallback, m_MaxCount = l };
            for (int i = 0; i < l; i++) {
                PopupList.ListElement element = packsPopup.NewOrMatchingElement(event_defs_object.packs[i].name);
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
                
                    InitializeObjectExplorer(event_defs_object.packs[i]);
                    RepopulatePopupList ();
                    break;
                }
            }
        }
       
        void InitializeObjectExplorer(AssetObjectPack pack){
            pack_type_gui = new GUIContent( pack == null ? "Pack Type" : "Pack Type: " + pack.name);
            oe.InitializeWithPack(pack);            
        }
    }
}