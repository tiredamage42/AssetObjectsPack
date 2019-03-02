using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AssetObjectsPacks {
    [CustomEditor(typeof(Event))]
    public class EventEditor : Editor {
        PopupList.InputData packsPopup;    
        Editor preview;
        Dictionary<int, string> id2path;
        GUIContent[] paramlabels;
        GUILayoutOption[] paramWidths {
            get {
                if (_paramWidths == null || _paramWidths.Length == 0) {
                    int c = paramlabels.Length;
                    _paramWidths = new GUILayoutOption[c];
                    for (int i = 0; i < c; i++) _paramWidths[i] = paramlabels[i].CalcWidth(GUIStyles.label);
                }
                return _paramWidths;
            }
        }
        GUILayoutOption[] _paramWidths;
        GUILayoutOption iconWidth = GUILayout.Width(20);
        
        string[] allPaths, errorStrings, warningStrings;
        string objectsDirectory, fileExtensions, assetType;
        bool initializeAfterPackChange, previewOpen, removeOrAdd, duplicated;
        bool singleDirectorySelected { get { return selectionSystem.singleSelection && selectionSystem.selectedElement.id == -1; } }
        int noIDsCount, packIndex;
        ElementSelectionSystem selectionSystem = new ElementSelectionSystem();
        EditorProp so;

        //ScaleConstraint Icon = duplicate

        //_Help = help

        BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
       
        #region CUSTOM_INSPECTOR_METHODS
        public override bool HasPreviewGUI() { 
            return previewOpen;
        }
        public override void OnInteractivePreviewGUI(Rect r, GUIStyle background) { 
            if (preview != null) preview.OnInteractivePreviewGUI(r, background); 
        }
        public override void OnPreviewSettings() { 
            if (preview != null) preview.OnPreviewSettings();
        }
        void OnEnable () {
            so = new EditorProp (serializedObject);
            EventStateEditor.ResetNewRecursive(so[Event.baseStateField], true);    
            EnableCurrentPack();
        }
        public override void OnInspectorGUI() {
            base.OnInspectorGUI();
            GUIUtils.StartCustomEditor();
            DrawEvent();

            if (!(forceRebuild && generateNewIDs)) {
                GUIUtils.EndCustomEditor(so);
            }
            else {
                GUIUtility.ExitGUI();
            }
            selectionSystem.CheckRebuild (forceRebuild, forceReset);
            forceRebuild = false;
            forceReset = false;
        }

        #endregion
        

        #region ELEMENT_SELECT_CALLBACKS
        string OnDirectoryCreate (string parentDir) {
            if (viewTab == 0) {
                return EventStateEditor.NewEventState(so[Event.baseStateField], parentDir);
            }
            else {

            }
            return null;
        }
        void OnSelectionChange() {
            RebuildPreviewEditor();
        }
        
        void OnNameNewDirectory(int poolIndex, string newName) {
            if (viewTab == 0) {
                EventStateEditor.QuickRenameNewEventState(baseState, selectionSystem.curPath, poolIndex, newName);
                namedNewDir = true;
            }            
        }

        EditorProp baseState { get { return so[Event.baseStateField]; } }

        void GetIDsInDirectory (string directory, bool useRepeats, HashSet<int> ids) {
            if (viewTab == 0) {
                EventStateEditor.GetAllEventIDs(baseState, directory, useRepeats, ids);
            }
            else {
                HashSet<int> ignoreIDs = new HashSet<int>();
                EventStateEditor.GetAllEventIDs(baseState, false, ignoreIDs);
                for (int i = 0; i < allPaths.Length; i++) {
                    if (!allPaths[i].StartsWith(directory)) continue;
                    int id = AssetObjectsEditor.GetObjectIDFromPath(allPaths[i]); 
                    if (hiddenIDsToggler.IsState(id) != showingHidden) continue;
                    if (ignoreIDs.Contains(id)) continue;
                    ids.Add(id);
                }
            }
        }

        IEnumerable<ElementSelectionSystem.Element> GetPoolElements (string atPath) {
            int c = allPaths.Length;

            EditorProp currentEventState = null;
            HashSet<int> ignoreIDs = new HashSet<int>();
            if (viewTab == 0) {
                currentEventState = EventStateEditor.GetEventStateByPath(baseState, atPath);
                c = EventStateEditor.GetEventTotalCount(currentEventState);
            } 
            else if (viewTab == 1) {
                EventStateEditor.GetAllEventIDs(baseState, false, ignoreIDs);
            }
            
            for (int i = 0; i < c; i++) {

                int id;
                string path = "";
                bool isNewDir = false, isCopy = false;
                if (viewTab == 0) {
                    string elName;
                    bool isSubstate;
                    EventStateEditor.GetValues(currentEventState, i, out id, out elName, out isNewDir, out isCopy, out isSubstate);
                    path = atPath + (atPath.IsEmpty() ? "" : "/");
                    if (isSubstate) path = path + elName + "/";
                    else path = path + AssetObjectsEditor.RemoveIDFromPath(id2path[id]).Replace("/", "-");
                    
                }
                else {

                    if (!atPath.IsEmpty() && !allPaths[i].StartsWith(atPath)) continue;
                    id = AssetObjectsEditor.GetObjectIDFromPath(allPaths[i]); 
                    
                    if (hiddenIDsToggler.IsState(id) != showingHidden) continue;
                    
                    if (ignoreIDs.Contains(id)) continue;
                    path = AssetObjectsEditor.RemoveIDFromPath(allPaths[i]);   
                }
                yield return new ElementSelectionSystem.Element(id, path, i, isNewDir, isCopy);
            }
        }

        StateToggler hiddenIDsToggler = new StateToggler();
        


        bool createdDirectory, hiddenToggleSuccess;

    
        void ExtraToolbarButtons (string curViewPath, KeyboardListener k) {

            hiddenToggleSuccess = createdDirectory = duplicated = removeOrAdd = false;
            
            if (!showingHidden) {

                //add directory
            
                if (GUIUtils.Button(new GUIContent(EditorGUIUtility.IconContent("Folder Icon").image, "Add Folder"), GUIStyles.toolbarButton, iconWidth)) {
                    //string origDirName = OnDirectoryCreate(viewTab, curViewPath);
                    selectionSystem.OnDirectoryCreate(OnDirectoryCreate(curViewPath));
                    createdDirectory = true;
                }
            
                GUI.enabled = selectionSystem.hasSelection;
                OpenImportSettingsButton("_Popup");
                AddOrRemoveButtons( viewTab, "Toolbar Plus", "Toolbar Minus", k );

            
                GUI.enabled = true;
                if (viewTab == 0) DuplicateButton(k);
            }
            if (viewTab != 0) {
                ToggleHiddenButtonGUI(k);    
            }

            GUI.enabled = selectionSystem.justFilesSelected;
            CheckPreviewToggle(k);
            GUI.enabled = true;
        }
        void OpenImportSettingsButton (string icon) {
            if (GUIUtils.Button(new GUIContent(EditorGUIUtility.IconContent(icon).image, "Open import settings on selection"), GUIStyles.toolbarButton, iconWidth)) OpenImportSettings();
        }
        void AddFilesButton (string icon, KeyboardListener k) {                
            removeOrAdd = GUIUtils.Button(new GUIContent(EditorGUIUtility.IconContent(icon).image, "Add Selected To Event"), GUIStyles.toolbarButton, iconWidth) || k[KeyCode.Return];
        }
        void RemoveFilesButton (string icon, KeyboardListener k) {                
            removeOrAdd = GUIUtils.Button(new GUIContent(EditorGUIUtility.IconContent(icon).image, "Remove Selected From Event"), GUIStyles.toolbarButton, iconWidth) || (k[KeyCode.Delete] || k[KeyCode.Backspace]);
        }   
        void AddOrRemoveButtons (int viewTab, string iconAdd, string iconRemove, KeyboardListener k) {
            if (viewTab == 0) RemoveFilesButton(iconRemove, k);   
            else if (viewTab == 1) AddFilesButton(iconAdd, k);
            if (removeOrAdd) {
                if (viewTab == 0) removeOrAdd = EventStateEditor.DeleteIndiciesFromState(so[Event.baseStateField], selectionSystem.curPath, selectionSystem.GetPoolIndiciesInSelection(true));    
                //no repeats (should be none anywyas)                    
                else if (viewTab == 1) removeOrAdd = EventStateEditor.AddIDsToState(so[Event.baseStateField], 
                selectionSystem.GetIDsInSelectionDeep(
                    "Add Objects", "Selection contains directories, Add all sub objects?"

                ), packIndex, GetObjectRefForID);
            }
        }

        void DuplicateButton (KeyboardListener k) {
            GUI.enabled = selectionSystem.hasSelection;
            duplicated = GUIUtils.Button(EditorGUIUtility.IconContent("ScaleConstraint Icon", "Duplicate"), GUIStyles.toolbarButton, iconWidth) || (k[KeyCode.D] && (k.command || k.ctrl));
            GUI.enabled = true;
            if (duplicated) EventStateEditor.DuplicateIndiciesInState(so[Event.baseStateField], selectionSystem.curPath, selectionSystem.GetSelectionEnumerable());
            so.SaveObject();
        }
        void ToggleHiddenButtonGUI (KeyboardListener k) {
            hiddenIDsToggler.ToggleStateButton(
                EditorGUIUtility.IconContent("animationvisibilitytoggleon", "Toggle the hidden status of the selection (if any, else all shown elements)"),
                GUIStyles.toolbarButton, iconWidth, k[KeyCode.H], GetHiddenToggleSelection, out hiddenToggleSuccess 
            );
        }
        IEnumerable<int> GetHiddenToggleSelection () {
            return selectionSystem.GetIDsInSelectionDeep(
                "Hide/Unhide Directory", "Selection contains directories, hidden status of all sub elements will be toggled"
            );
        }



        void OnDirDragDrop(IEnumerable<int> dragIndicies, string origDir, string targetDir) {
            if (viewTab == 0) {
                //Debug.Log("list view drag to dir " + targetDir);
                EventStateEditor.MoveAOsToEventState(so[Event.baseStateField], dragIndicies, origDir, targetDir);
            }
            else if (viewTab == 1) {
                //Debug.Log("project view drag to dir " + targetDir);
            }
        }        
        #endregion

        void EnableCurrentPack () {
            selectionSystem.OnEnable(GetPoolElements, GetIDsInDirectory, OnSelectionChange, OnDirDragDrop, ExtraToolbarButtons, OnNameNewDirectory);
            hiddenIDsToggler.OnEnable(so[Event.hiddenIDsField]);

            if (AssetObjectsEditor.packManager == null) return;
            int index;
            AssetObjectsEditor.packManager.FindPackByID(so[Event.pack_id_field].intValue, out index);
            Reinitialize( index );
        }

        void TogglePreview(bool enabled)
        {
            previewOpen = enabled;
            System.Type type = Assembly.GetAssembly(typeof(Editor)).GetType("UnityEditor.InspectorWindow");
            var inspector = Resources.FindObjectsOfTypeAll(type).Where( o => o.GetType().GetField("m_PreviewWindow", flags) != null).ToArray()[0];
            var previewResizer = inspector.GetType().GetField("m_PreviewResizer", flags).GetValue(inspector);
            bool expanded = (bool)previewResizer.GetType().GetMethod("GetExpanded", flags).Invoke(previewResizer, new object[] {});
            if (expanded != enabled) previewResizer.GetType().GetMethod("ToggleExpanded", flags).Invoke(previewResizer, new object[] {});
            
            if (enabled) RebuildPreviewEditor();
        }
        

        //to do add button
        void CheckPreviewToggle (KeyboardListener k) {

            //SceneViewFx = preview
        
            bool togglePreview = GUIUtils.Button(EditorGUIUtility.IconContent("SceneViewFx", "Open Preview"), GUIStyles.toolbarButton, iconWidth) || (k[KeyCode.P]);
            
            if (togglePreview) TogglePreview(!previewOpen);
        }
        
        void RebuildPreviewEditor () {
            
            if (preview != null) Editor.DestroyImmediate(preview);
            
            if (!previewOpen) return;

            //no repeats
            IEnumerable<int> idsInSelection = selectionSystem.GetIDsInSelection();
            
            if (idsInSelection == null || idsInSelection.Count() == 0) {
                //TogglePreview(false);
                return;
            } 
            
            preview = Editor.CreateEditor( idsInSelection.Generate(id => GetObjectRefForID(id) ).ToArray());

            preview.HasPreviewGUI();
            preview.OnInspectorGUI();
            preview.OnPreviewSettings();

            //auto play single selection for animations
            if (assetType == "UnityEngine.AnimationClip") {     
                if (idsInSelection.Count() == 1) {
                    // preview_editor.m_AvatarPreview.timeControl.playing = true
                    var flags = System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
                    var avatarPreview = preview.GetType().GetField("m_AvatarPreview", flags).GetValue(preview);
                    var timeControl = avatarPreview.GetType().GetField("timeControl", flags).GetValue(avatarPreview);
                    var setter = timeControl.GetType().GetProperty("playing", flags).GetSetMethod(true);
                    setter.Invoke(timeControl, new object[] { true });
                }
            }
        }
        bool forceRebuild, forceReset, generateNewIDs, namedNewDir;
        int viewTab;
        public bool showingHidden { get { return viewTab == 2; } }
        

        

        void DrawEvent () {

            PacksManager pm = AssetObjectsEditor.packManager;
            if (pm != null) {    

                GUIUtils.StartBox(0);
                EditorGUILayout.BeginHorizontal();        
                GUIUtils.Label(new GUIContent("<b>Pack Type : </b>"), true);

                if (GUIUtils.Button(new GUIContent(packIndex == -1 ? "None" : pm.packs[packIndex].name), GUIStyles.toolbarButton, true)) {

                    GUIUtils.ShowPopUpAtMouse(packsPopup);
                }

                GUILayout.FlexibleSpace();

                if (GUIUtils.Button(new GUIContent(" Help "), GUIStyles.toolbarButton, Colors.selected, Colors.black, true)) HelpWindow.Init();

                EditorGUILayout.EndHorizontal();

                GUIUtils.EndBox(0);

                forceRebuild = false;
                if (!generateNewIDs) {

                    generateNewIDs = PackEditor.GUI.DrawErrorsAndWarnings(errorStrings, warningStrings, noIDsCount, packIndex);
                    if (generateNewIDs) {
                        PackEditor.GetErrorsAndWarnings (packIndex, out errorStrings, out warningStrings, out noIDsCount);
                        InitializeAllFilePaths();
                        forceRebuild = true;
                        
                        //GUIUtility.ExitGUI();
                    }
                }
                //else {

                    if (errorStrings.Length != 0) return;
                    if (viewTab == 0) forceRebuild = DrawEditToolbar() || forceRebuild;
                //}   


                GUIContent[] viewTabGUIs = new GUIContent[] {
                    new GUIContent("Event Pack"),
                    new GUIContent("Project"),
                    new GUIContent("Hidden"),
                };
                            
                bool changedTabView = GUIUtils.Tabs(viewTabGUIs, ref this.viewTab);
            
                    selectionSystem.DrawElements();
                


                forceRebuild = forceRebuild || namedNewDir || changedTabView || removeOrAdd || duplicated || createdDirectory || hiddenToggleSuccess;
                forceReset = forceRebuild && changedTabView;

                duplicated = false;
                removeOrAdd = false;
                createdDirectory = false;
                hiddenToggleSuccess = false;
                namedNewDir = false;
                //changedTabView = false;
            }
        }
            
        void Reinitialize (int index) {
            this.packIndex = index;

            PackEditor.GetErrorsAndWarnings (packIndex, out errorStrings, out warningStrings, out noIDsCount);
            
            paramlabels = new GUIContent[0];
            _paramWidths = null;
                
            if (errorStrings.Length == 0) {

                PackEditor.GetValues(packIndex, out _, out objectsDirectory, out fileExtensions, out assetType);

                InitializeAllFilePaths();

                EventStateEditor.UpdatEventStatesAgainstDefaults(so[Event.baseStateField], packIndex);
                
                AssetObjectEditor.MakeAssetObjectDefault(so[Event.multi_edit_instance_field], packIndex, true);
                paramlabels = PackEditor.GUI.GetDefaultParamNameGUIs(packIndex);
                
                selectionSystem.Initialize();
                hiddenIDsToggler.Initialize();
            
            }

            packsPopup = new PopupList.InputData { m_OnSelectCallback = OnSwitchPackCallback };
            for (int i = 0; i < AssetObjectsEditor.packManager.packs.Length; i++) packsPopup.NewOrMatchingElement(AssetObjectsEditor.packManager.packs[i].name, index == i);        
        }


        public void OnSwitchPackCallback(PopupList.ListElement element) {

            for (int i = 0; i < AssetObjectsEditor.packManager.packs.Length; i++) {
                if (i == packIndex) continue;
                if (AssetObjectsEditor.packManager.packs[i].name != element.m_Content.text) continue;
                if (EditorUtility.DisplayDialog("Switch Pack", "Are you sure you want to change packs?\n\nThis will reset the event.", "Switch Pack", "Cancel")) {
                    //new pack id
                    so[Event.pack_id_field].SetValue( AssetObjectsEditor.packManager.packs[i].id );
                    //reset hidden ids
                    so[Event.hiddenIDsField].Clear();
                    //reset asset objects
                    EventStateEditor.ResetBaseState(so[Event.baseStateField]);
                    Reinitialize(i);       
                    so.SaveObject();      
                }
                break;
            }
        }

        UnityEngine.Object GetObjectRefForID(int id) {
            return EditorUtils.GetAssetAtPath(objectsDirectory + id2path[id], assetType);  
        }
        void InitializeAllFilePaths () {
            allPaths = AssetObjectsEditor.GetAllAssetObjectPaths (objectsDirectory, fileExtensions, false, out id2path);
        }
        GUIContent GetEditToolbarTitle (bool hasSelection, bool singleSelection, string selectedPath){
            string title = "Multi-Edit <b>All</b> Objects";
            if (hasSelection) {
                if (singleSelection) title = "Editing: <b>" + selectedPath + "</b>";
                else title = "Multi-Edit <b>Selected</b> Objects";
            }
            return new GUIContent(title);
        }
        EditorProp GetAOPropForEditToolbar (EditorProp eventState, bool singleSelection, int selectedID, int selectedPoolID) {
            return (!singleSelection || selectedID == -1) ? so[Event.multi_edit_instance_field] : EventStateEditor.GetAOatPoolID(eventState, selectedPoolID);
        }
        
        bool DrawEditToolbar (){
            bool shouldRebuild = false;

            GUIUtils.StartBox(1);
            EditorProp curEventState = EventStateEditor.GetEventStateByPath(so[Event.baseStateField], selectionSystem.curPath);

            string parentPath = selectionSystem.CalcLastFolder(selectionSystem.curPath);
            
            EditorProp curParentEventState = (curEventState != so[Event.baseStateField]) ? EventStateEditor.GetEventStateByPath(so[Event.baseStateField], parentPath) : null;
        
            string newStateName;
            bool deletedState, changedStateName;
            EventStateEditor.GUI.DrawEventState(so[Event.baseStateField], curParentEventState, curEventState, out deletedState, out changedStateName, out newStateName);

            if (changedStateName) {
                selectionSystem.ChangeCurrentDirectoryName(newStateName);
                shouldRebuild = true;
            }
            
            if (deletedState) {
                selectionSystem.ForceBackFolder();
                shouldRebuild = true;
            }
            
            bool hasSelection = selectionSystem.hasSelection;
            bool singleSelection = selectionSystem.singleSelection;
            ElementSelectionSystem.Element selectedElement = selectionSystem.selectedElement;
            GUIUtils.EndBox(1);
            GUIUtils.StartBox(0);
                        
            int setParam = -1;
            GUI.enabled = !singleDirectorySelected;
            GUIUtils.Label(GetEditToolbarTitle (hasSelection, singleSelection, selectedElement.path));
            AssetObjectEditor.GUI.DrawAssetObjectEdit(GetAOPropForEditToolbar (curEventState, singleSelection, selectedElement.id, selectedElement.poolIndex), hasSelection && singleSelection, paramlabels, paramWidths, out setParam);
            GUI.enabled = true;

            GUIUtils.EndBox(1);
            
            if (setParam != -1) {
                AssetObjectEditor.CopyParameters(
                    selectionSystem.GetPoolIndiciesInSelectionOrAllShown().Generate(i=>EventStateEditor.GetAOatPoolID(curEventState, i)),
                    so[Event.multi_edit_instance_field], 
                    setParam
                );
            }
            return shouldRebuild;
        }

        void OpenImportSettings () {
            //no repeat ids
            IEnumerable<int> idsInSelection = selectionSystem.GetIDsInSelection();
            
            
            Object[] rootAssets = idsInSelection.Generate( id => AssetDatabase.LoadAssetAtPath(objectsDirectory + id2path[id], typeof(Object))).ToArray();
            Animations.EditImportSettings.CreateWizard(rootAssets);
        }
    }
}