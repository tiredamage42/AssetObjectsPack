using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
namespace AssetObjectsPacks {
    [CustomEditor(typeof(Event))]
    public class EventEditor : Editor {
        PopupList.InputData packsPopup;    
        EditorProp multiEditAO, AOList, hiddenIDsProp;
        Editor preview;
        Dictionary<int, string> id2path;
        GUIContent[] paramlabels;
        GUILayoutOption[] paramWidths;
        string[] allPaths, errorStrings, warningStrings;
        string objectsDirectory, fileExtensions, assetType;
        bool initializeAfterPackChange;
        int noIDsCount, packIndex;
        ElementSelectionSystem selectionSystem = new ElementSelectionSystem();

        EditorProp so;

        public override bool HasPreviewGUI() { 
            return preview != null; 
        }
        public override void OnInteractivePreviewGUI(Rect r, GUIStyle background) { 
            if (preview != null) preview.OnInteractivePreviewGUI(r, background); 
        }
        public override void OnPreviewSettings() { 
            if (preview != null) preview.OnPreviewSettings();
        }

        void OnEnable () {
            so = new EditorProp (serializedObject);

            AOList = so[Event.asset_objs_field];
            hiddenIDsProp = so[Event.hiddenIDsField];
            multiEditAO = so[Event.multi_edit_instance_field];

            if (AssetObjectsEditor.packManager == null) return;
            
            int packID = so[Event.pack_id_field].intValue;
            int index;
            AssetObjectsEditor.packManager.FindPackByID(packID, out index);

            Reinitialize( index );
        }
        public override void OnInspectorGUI() {
            //base.OnInspectorGUI();

            GUIUtils.StartCustomEditor();
            PacksManager pm = AssetObjectsEditor.packManager;
            if (pm != null) {    

                GUIUtils.StartBox(0);
                EditorGUILayout.BeginHorizontal();        
                GUIUtils.Label(new GUIContent("<b>Pack Type : </b>"), true);

                if (GUIUtils.Button(new GUIContent(packIndex == -1 ? "None" : pm.packs[packIndex].name), true, GUIStyles.toolbarButton)) {

                    GUIUtils.ShowPopUpAtMouse(packsPopup);
                }

                GUILayout.FlexibleSpace();

                if (GUIUtils.Button(new GUIContent(" Help "), true, GUIStyles.toolbarButton, Colors.selected, Colors.black)) HelpWindow.Init();

                EditorGUILayout.EndHorizontal();

                GUIUtils.EndBox(0);
                
                Draw();
            }
            GUIUtils.EndCustomEditor(so);
        }

        ElementSelectionSystem.Element GetPoolElementAtIndex(int i, int viewTab) {
            int id;
            string path;
            if (viewTab == 0) {
                id = AssetObjectEditor.GetID(AOList[i]);
                path = id2path[id];
            }
            else {
                path = allPaths[i];
                id = AssetObjectsEditor.GetObjectIDFromPath(path); 
            }
            //GUIContent gui = new GUIContent( AssetObjectsEditor.RemoveIDFromPath(folderedView && path.Contains("/") ? path.Split('/').Last() : path) );
            return new ElementSelectionSystem.Element(id, AssetObjectsEditor.RemoveIDFromPath(path), i);
        }

        void Reinitialize (int index) {
            this.packIndex = index;

            PackEditor.GetErrorsAndWarnings (packIndex, out errorStrings, out warningStrings, out noIDsCount);
            
            paramlabels = new GUIContent[0];
            paramWidths = new GUILayoutOption[0];
                
            if (errorStrings.Length == 0) {

                PackEditor.GetValues(packIndex, out _, out objectsDirectory, out fileExtensions, out assetType);
                
                InitializeAllFilePaths();

                for (int i = 0; i < AOList.arraySize; i++) AssetObjectEditor.MakeAssetObjectDefault(AOList[i], packIndex, false);
                
                AssetObjectEditor.MakeAssetObjectDefault(multiEditAO, packIndex, true);
                paramlabels = PackEditor.GUI.GetDefaultParamNameGUIs(packIndex);
                
                GUIContent[] tab_guis = new GUIContent[] { 
                    new GUIContent("Event Pack"), 
                    new GUIContent("Project") 
                };

                selectionSystem.Initialize(tab_guis, hiddenIDsProp, GetPoolElementAtIndex, GetPoolCount, GetIgnoreIDs, 
                //DrawElement, 
                RebuildPreviewEditor);
                
                initializeAfterPackChange = true;
            }

            PacksManager pm = AssetObjectsEditor.packManager;
            packsPopup = new PopupList.InputData { m_OnSelectCallback = OnSwitchPackCallback };
            int l = pm.packs.Length;
            for (int i = 0; i < l; i++) packsPopup.NewOrMatchingElement(pm.packs[i].name, index == i);        
        }

        public void OnSwitchPackCallback(PopupList.ListElement element) {

            PacksManager pm = AssetObjectsEditor.packManager;
            int l = pm.packs.Length;
            for (int i = 0; i < l; i++) {
                if (i == packIndex) continue;
                if (pm.packs[i].name != element.m_Content.text) continue;

                if (EditorUtility.DisplayDialog("Switch Pack", "Are you sure you want to change packs?\n\nThis will reset the event.", "Switch Pack", "Cancel")) {
                    //new pack id
                    so[Event.pack_id_field].SetValue( pm.packs[i].id );
                    //reset hidden ids
                    hiddenIDsProp.Clear();
                    //reset asset objects
                    AOList.Clear();
                    Reinitialize(i);             
                    break;
                }
            }
        }
        
        HashSet<int> GetIgnoreIDs (int viewTab) {
            return viewTab != 1 ? new HashSet<int>() : new HashSet<int>().Generate(AOList.arraySize, i => AssetObjectEditor.GetID(AOList[i]) ); 
        }
        int GetPoolCount (int viewTab) {
            return viewTab == 0 ? AOList.arraySize : allPaths.Length;
        }


/*
        bool hasSelection = selectionSystem.hasSelection;
        bool singleSelection = selectionSystem.singleSelection;
        ElementSelectionSystem.Element selectedElement = selectionSystem.selectedElement;


 */

       



        UnityEngine.Object GetObjectRefForID(int id) {
            return EditorUtils.GetAssetAtPath(objectsDirectory + id2path[id], assetType);  
        }
    
        void InitializeAfterPackChange () {
            if (!initializeAfterPackChange) return;
            initializeAfterPackChange = false;
            int props_count = paramlabels.Length;
            paramWidths = new GUILayoutOption[props_count];
            for (int i = 0; i < props_count; i++) paramWidths[i] = paramlabels[i].CalcWidth(GUIStyles.label);
        }

        //bool DrawElement(ElementSelectionSystem.Element element, GUIContent display, bool selected, bool hidden, int viewTab, out bool drawNormal) {

                            //bool selectedElement = ElementSelectionSystem.SelectionSystemElementGUI (display, selected, hidden, false);                    

        //    drawNormal = viewTab != 0;
        //    return drawNormal ? false : AssetObjectEditor.GUI.DrawAssetObjectEventView(AOList[element.poolIndex], display, selected, hidden, paramWidths);
        //}
        
        void InitializeAllFilePaths () {
            allPaths = AssetObjectsEditor.GetAllAssetObjectPaths (objectsDirectory, fileExtensions, false, out id2path);
        }

        void CustomToolbar (int viewTab){
            GUIUtils.StartBox(0);
            
            EditorGUILayout.BeginHorizontal();
            GUI.enabled = selectionSystem.justFilesSelected;
            if (ImportSettingsButton(GUIStyles.miniButtonLeft)) OpenImportSettings();
            removeOrAdd = AddRemoveButton(GUIStyles.miniButtonRight);
            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();

            if (viewTab == 0) DrawMultiEditGUI();

            GUIUtils.EndBox(1);
        }
        static bool ImportSettingsButton (GUIStyle s) {
            return GUIUtils.Button(new GUIContent("Import Settings", "Open import settings on selection (if any)"), true, s);
        }
        static bool AddRemoveButton (GUIStyle s) {
            return GUIUtils.Button(new GUIContent("Add/Remove", "Add Selected (When in project view)\nRemove Selected (When in event view)"), true, s);
        }



        GUIContent GetEditToolbarTitle (bool hasSelection, bool singleSelection, ElementSelectionSystem.Element selectedElement) {
            string title = "Multi-Edit <b>All</b> Objects";
            if (hasSelection) {
                if (singleSelection) {
                    title = "Editing: <b>" + selectedElement.path + "</b>";
                }
                else {
                    title = "Multi-Edit <b>Selected</b> Objects";
                }
            }
            return new GUIContent(title);
        }
        EditorProp GetAOPropForEditToolbar (bool hasSelection, bool singleSelection, ElementSelectionSystem.Element selectedElement) {
            EditorProp p = multiEditAO;
            if (hasSelection) {
                if (singleSelection) {
                    p = AOList[selectedElement.poolIndex];
                }
            }
            return p;
        }


        
        void DrawMultiEditGUI (){
             bool hasSelection = selectionSystem.hasSelection;
            bool singleSelection = selectionSystem.singleSelection;
            ElementSelectionSystem.Element selectedElement = selectionSystem.selectedElement;

            
            GUIUtils.Space(1);
            //GUIUtils.Label(new GUIContent("<b>Multi-Object Editing</b>",""), false);
            GUIUtils.Label(GetEditToolbarTitle (hasSelection, singleSelection, selectedElement), false);
            
            int setProp = -1;
            //bool showParamsChanged, showConditionsChanged, multiConditionAdd, multiConditionReplace;
            bool multiConditionAdd, multiConditionReplace;
            
            AssetObjectEditor.GUI.DrawAssetObjectMultiEditView(

                hasSelection && singleSelection,
                //out showConditionsChanged, 
                //out showParamsChanged, 
                GetAOPropForEditToolbar (hasSelection, singleSelection, selectedElement),

                //multiEditAO, 
                paramWidths, 
                out multiConditionAdd, 
                out multiConditionReplace, 
                out setProp, 
                paramlabels
            );




            //if (showParamsChanged || showConditionsChanged) AssetObjectEditor.CopyShowValues(GetAOPropsSelectOrAll(), multiEditAO, showParamsChanged);
            if (multiConditionAdd || multiConditionReplace) AssetObjectEditor.CopyConditions(GetAOPropsSelectOrAll(), multiEditAO, multiConditionAdd);
            if (setProp != -1) AssetObjectEditor.CopyParameters(GetAOPropsSelectOrAll(), multiEditAO, setProp);
        }

        void Draw (){
            InitializeAfterPackChange();

            KeyboardListener kbListener = new KeyboardListener();
            ElementSelectionSystem.Inputs inputs = new ElementSelectionSystem.Inputs();

            int viewTab = selectionSystem.viewTab;
            //bool useListView = viewTab == 0;

            bool generateNewIDs = PackEditor.GUI.DrawErrorsAndWarnings(errorStrings, warningStrings, noIDsCount, packIndex);
           
            if (errorStrings.Length != 0) return;
                        
            selectionSystem.DrawToolbar(kbListener, inputs);
            
            CustomToolbar(viewTab);
        
            selectionSystem.DrawElements(viewTab, inputs, kbListener);
            selectionSystem.DrawPages (inputs, kbListener);
            
    
            bool listChanged = false;
            bool remove = (viewTab == 0) && (kbListener[KeyCode.Delete] || kbListener[KeyCode.Backspace]);
            bool add = !(viewTab == 1) && kbListener[KeyCode.Return];
                
            if (removeOrAdd || add || remove) {
                if ((removeOrAdd && viewTab == 0) || remove) listChanged = DeleteSelectionFromList();
                if ((removeOrAdd && viewTab == 1) || add) listChanged = AddSelectionToSet();
            }
            
            if (generateNewIDs) {
                PackEditor.GetErrorsAndWarnings (packIndex, out errorStrings, out warningStrings, out noIDsCount);
                InitializeAllFilePaths();
            }
            
            selectionSystem.HandleInputs(inputs, generateNewIDs || listChanged);

            removeOrAdd = false;
            
        }   

        IEnumerable<EditorProp> GetAOPropsSelectOrAll () {
            return new HashSet<EditorProp>().Generate(selectionSystem.GetPoolIndiciesInSelectionOrAllShown(), i => AOList[i] );
        }
        
        bool AddSelectionToSet () {
            bool reset_i = true;            
            HashSet<int> idsInSelection = selectionSystem.GetIDsInSelection(false, out _);
            if (idsInSelection.Count == 0) return false;
            foreach (var id in idsInSelection) {
                AssetObjectEditor.InitializeNewAssetObject(AOList.AddNew(), id, GetObjectRefForID(id), reset_i, packIndex);    
                reset_i = false;
            }
            return true;
        }
        bool DeleteSelectionFromList () {            
            HashSet<int> deleteIndicies = selectionSystem.GetPoolIndiciesInSelection();
            if (deleteIndicies.Count == 0) return false;
            for (int i = AOList.arraySize - 1; i >= 0; i--) {
                if (deleteIndicies.Contains(i)) AOList.DeleteAt(i);
            }
            return true;
        }
     
        void OpenImportSettings () {

            HashSet<int> idsInSelection = selectionSystem.GetIDsInSelection(false, out _);

            
            Object[] rootAssets = new Object[idsInSelection.Count].Generate(idsInSelection, id => AssetDatabase.LoadAssetAtPath(objectsDirectory + id2path[id], typeof(Object)));

            Animations.EditImportSettings.CreateWizard(rootAssets);
        }
        
        bool removeOrAdd;
        



        void RebuildPreviewEditor () {
            if (preview != null) Editor.DestroyImmediate(preview);
            HashSet<int> idsInSelection = selectionSystem.GetIDsInSelection(false, out _);
            if (idsInSelection == null) return;
            int c = idsInSelection.Count;
            if (c == 0) return;
            
            preview = Editor.CreateEditor(new UnityEngine.Object[c].Generate(idsInSelection, id => GetObjectRefForID(id) ));
            
            preview.HasPreviewGUI();
            preview.OnInspectorGUI();
            preview.OnPreviewSettings();

            //auto play single selection for animations
            if (c == 1) {
                if (assetType == "UnityEngine.AnimationClip") {     
                    
                    // preview_editor.m_AvatarPreview.timeControl.playing = true

                    var flags = System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
                    var avatarPreview = preview.GetType().GetField("m_AvatarPreview", flags).GetValue(preview);
                    var timeControl = avatarPreview.GetType().GetField("timeControl", flags).GetValue(avatarPreview);
                    var setter = timeControl.GetType().GetProperty("playing", flags).GetSetMethod(true);
                    setter.Invoke(timeControl, new object[] { true });
                }
            }
        }

        
    }
}