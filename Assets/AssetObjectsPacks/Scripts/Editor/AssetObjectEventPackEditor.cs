using UnityEngine;
using UnityEditor;
namespace AssetObjectsPacks {
    [CustomEditor(typeof(AssetObjectEventPack))]
    public class AssetObjectEventPackEditor : Editor {
        PacksManager packsManager;   
        EditorProp eventDefsProp;
        PopupList.InputData packsPopup;    
        AssetObjectListGUI oe = new AssetObjectListGUI();
        GUIContent pack_type_gui;
        public override bool HasPreviewGUI() { return oe.HasPreviewGUI(); }
        public override void OnInteractivePreviewGUI(Rect r, GUIStyle background) { oe.OnInteractivePreviewGUI(r, background); }
        public override void OnPreviewSettings() { oe.OnPreviewSettings(); }

        void OnEnable () {
            packsManager = AssetObjectsEditor.GetPackManager();
            if (packsManager == null) {
                oe.RealOnEnable(null, serializedObject);
                return;
            }

            eventDefsProp = new EditorProp ( new SerializedObject ( packsManager ).FindProperty( PacksManager.packsField ) );

            oe.RealOnEnable(eventDefsProp, serializedObject);

            int packID = serializedObject.FindProperty(AssetObjectEventPack.pack_id_field).intValue;
            int index;
            InitializeObjectExplorer(packsManager.FindPackByID(packID, out index), index);
            RepopulatePopupList( packID );
            
        }
        public override void OnInspectorGUI() {
            //base.OnInspectorGUI();
            GUIUtils.CustomEditor(
                this, 
                () => {
                    bool force_change = false;
                    if (packsManager != null) {            
                        if (GUIUtils.Button(pack_type_gui, true, GUI.skin.button)) GUIUtils.ShowPopUpAtMouse(packsPopup);
                        force_change = oe.Draw();
                    }
                }
            );
        }

        void RepopulatePopupList (int currentPackID) {
            packsPopup = new PopupList.InputData { m_OnSelectCallback = OnSwitchPackCallback };
            int l = packsManager.packs.Length;
            for (int i = 0; i < l; i++) packsPopup.NewOrMatchingElement(packsManager.packs[i].name, currentPackID == packsManager.packs[i].id);
        }

        public void OnSwitchPackCallback(PopupList.ListElement element) {//, bool little_button_pressed) {
            int l = packsManager.packs.Length;
            for (int i = 0; i < l; i++) {
                if (packsManager.packs[i].name == element.m_Content.text) {

                    int new_id = packsManager.packs[i].id;
                    
                    //new pack id
                    serializedObject.FindProperty(AssetObjectEventPack.pack_id_field).intValue = new_id;

                    //reset hidden ids
                    serializedObject.FindProperty(AssetObjectEventPack.hiddenIDsField).ClearArray();

                    //reset asset objects
                    serializedObject.FindProperty(AssetObjectEventPack.asset_objs_field).ClearArray();

                    //list gui initialization resets multi edit instance anyways
                    //serializedObject.FindProperty(AssetObjectEventPack.multi_edit_instance_field);

                    serializedObject.ApplyModifiedProperties();
                
                    InitializeObjectExplorer(packsManager.packs[i], i);
                    RepopulatePopupList (new_id);
                    break;
                }
            }
        }
       
        void InitializeObjectExplorer(AssetObjectPack pack, int packIndex){
            pack_type_gui = new GUIContent( pack == null ? "Pack Type" : "Pack Type: " + pack.name);
            oe.InitializeWithPack(pack, (packIndex < 0) ? null : eventDefsProp [ packIndex ] );            
        }
    }
}