using UnityEngine;
using UnityEditor;
namespace AssetObjectsPacks {
    [CustomEditor(typeof(AssetObjectEventPack))]
    public class AssetObjectEventPackEditor : Editor {
        AssetObjectPacks event_defs_object;   
        PopupList.InputData packsPopup;    
        AssetObjectListGUI oe = new AssetObjectListGUI();
        GUIContent pack_type_gui;

        public override bool HasPreviewGUI() { return oe.HasPreviewGUI(); }
        public override void OnInteractivePreviewGUI(Rect r, GUIStyle background) { oe.OnInteractivePreviewGUI(r, background); }
        public override void OnPreviewSettings() { oe.OnPreviewSettings(); }
        
        


        void OnEnable () {
            oe.RealOnEnable(serializedObject);
            event_defs_object = AssetObjectsEditor.GetAssetObjectsPacksObject();
            if (event_defs_object != null) {
                int packID = serializedObject.FindProperty(AssetObjectEventPack.pack_id_field).intValue;
                InitializeObjectExplorer(event_defs_object.FindPackByID(packID));
                RepopulatePopupList( packID );
            }
        }
        public override void OnInspectorGUI() {
            //base.OnInspectorGUI();
            GUIUtils.StartCustomEditor();
            bool force_change = false;
            if (event_defs_object != null) {            
                if (GUIUtils.Button(pack_type_gui, true, GUI.skin.button)) GUIUtils.ShowPopUpAtMouse(packsPopup, false, false);     
                force_change = oe.Draw();
            }
            GUIUtils.EndCustomEditor(this, force_change);
        }

        void RepopulatePopupList (int currentPackID) {
            int l = event_defs_object.packs.Length;
            packsPopup = new PopupList.InputData { m_OnSelectCallback = OnSwitchPackCallback, m_MaxCount = l };
            for (int i = 0; i < l; i++) {
                PopupList.ListElement element = packsPopup.NewOrMatchingElement(event_defs_object.packs[i].name);
                element.selected = currentPackID == event_defs_object.packs[i].id;
            }
        }

        public void OnSwitchPackCallback(PopupList.ListElement element, bool little_button_pressed) {
            int l = event_defs_object.packs.Length;
            for (int i = 0; i < l; i++) {
                if (event_defs_object.packs[i].name == element.text) {

                    int new_id = event_defs_object.packs[i].id;
                    
                    //new pack id
                    serializedObject.FindProperty(AssetObjectEventPack.pack_id_field).intValue = new_id;

                    //reset hidden ids
                    serializedObject.FindProperty(AssetObjectEventPack.hiddenIDsField).ClearArray();

                    //reset asset objects
                    serializedObject.FindProperty(AssetObjectEventPack.asset_objs_field).ClearArray();

                    //list gui initialization resets multi edit instance anyways
                    //serializedObject.FindProperty(AssetObjectEventPack.multi_edit_instance_field);

                    serializedObject.ApplyModifiedProperties();
                
                    InitializeObjectExplorer(event_defs_object.packs[i]);
                    RepopulatePopupList (new_id);
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