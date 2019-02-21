using UnityEngine;
using UnityEditor;
namespace AssetObjectsPacks {
    [CustomEditor(typeof(AssetObjectEventPack))]
    public class AssetObjectEventPackEditor : Editor {
        PopupList.InputData packsPopup;    
        AssetObjectListGUI oe = new AssetObjectListGUI();
        GUIContent pack_type_gui;
        GUIContent changePackTypeGUI = new GUIContent("<b>Pack Type : </b>");
        GUIContent helpGUI = new GUIContent(" Help ");
        
        public override bool HasPreviewGUI() { return oe.HasPreviewGUI(); }
        public override void OnInteractivePreviewGUI(Rect r, GUIStyle background) { oe.OnInteractivePreviewGUI(r, background); }
        public override void OnPreviewSettings() { oe.OnPreviewSettings(); }

        int curPackIndex;

        void OnEnable () {
            oe.RealOnEnable(serializedObject);
            if (AssetObjectsEditor.packManager == null) return;
            
            int packID = serializedObject.FindProperty(AssetObjectEventPack.pack_id_field).intValue;
            int index;
            AssetObjectsEditor.packManager.FindPackByID(packID, out index);
            Reinitialize( index );
        }
        public override void OnInspectorGUI() {
            //base.OnInspectorGUI();

            GUIUtils.StartCustomEditor();
            bool force_change = false;
            if (AssetObjectsEditor.packManager != null) {    

                GUIUtils.StartBox(0);
                EditorGUILayout.BeginHorizontal();        
                GUIUtils.Label(changePackTypeGUI, true);
                if (GUIUtils.Button(pack_type_gui, true, GUIStyles.toolbarButton)) {

                    GUIUtils.ShowPopUpAtMouse(packsPopup);
                }

                GUILayout.FlexibleSpace();

                if (GUIUtils.Button(helpGUI, true, GUIStyles.toolbarButton, EditorColors.selected, EditorColors.black)) {
                    HelpWindow.Init();
                }


                EditorGUILayout.EndHorizontal();


                GUIUtils.EndBox(0);
                
                force_change = oe.Draw();
            }
            GUIUtils.EndCustomEditor(this, force_change);
        }
        void Reinitialize (int index) {
            this.curPackIndex = index;
            PacksManager pm = AssetObjectsEditor.packManager;

            pack_type_gui = new GUIContent(index == -1 ? "None" : pm.packs[index].name);
            oe.InitializeWithPack(index);

            packsPopup = new PopupList.InputData { m_OnSelectCallback = OnSwitchPackCallback };
            int l = pm.packs.Length;
            for (int i = 0; i < l; i++) packsPopup.NewOrMatchingElement(pm.packs[i].name, index == i);        
        }

        public void OnSwitchPackCallback(PopupList.ListElement element) {


            PacksManager pm = AssetObjectsEditor.packManager;
            int l = pm.packs.Length;
            for (int i = 0; i < l; i++) {
                if (pm.packs[i].name == element.m_Content.text) {
                    if (i != curPackIndex) {

                        if (EditorUtility.DisplayDialog("Switch Pack", "Are you sure you want to change packs?\n\nThis will reset the event.", "Switch Pack", "Cancel")) {
                            //new pack id
                            serializedObject.FindProperty(AssetObjectEventPack.pack_id_field).intValue = pm.packs[i].id;
                            //reset hidden ids
                            serializedObject.FindProperty(AssetObjectEventPack.hiddenIDsField).ClearArray();
                            //reset asset objects
                            serializedObject.FindProperty(AssetObjectEventPack.asset_objs_field).ClearArray();

                            //list gui initialization resets multi edit instance

                            serializedObject.ApplyModifiedProperties();                
                            Reinitialize(i);
                            break;
                        }
                    }
                }
            }
        }
       
    }
}