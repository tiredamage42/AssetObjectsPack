using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System;

namespace AssetObjectsPacks {

    [CustomEditor(typeof(Event))]
    public class AOStateMachineEditor : Editor {

        int GetStateParentBrute (int checkStateID) {
            for (int i =0 ; i < so[allStatesField].arraySize; i++) {
                EditorProp state = so[allStatesField][i];
                int stateID = state[stateIDField].intValue;

                if (stateID != checkStateID) {

                    for (int x = 0; x < state[subStateIDsField].arraySize; x++) {
                        int childID = state[subStateIDsField][x].intValue;
                        if (checkStateID == childID) {
                            return stateID;
                        }
                    }

                }
            }
            return -1;
        }



        Dictionary<int, SelectionElement> stateMachineDirectoryTree = new Dictionary<int, SelectionElement>();

            
        Dictionary<int, SelectionElement> BuildDirectoryTreeElementsForStateMachineView () {

            stateMachineDirectoryTree.Clear();

            for (int i =0 ; i < so[allStatesField].arraySize; i++) {
                EditorProp state = so[allStatesField][i];

                string displayName = state[nameField].stringValue;
                int stateID = state[stateIDField].intValue;

                
                SelectionElement newElement = new SelectionElement(false, true, stateID, new GUIContent(displayName), -1);

                stateMachineDirectoryTree.Add(stateID, newElement);

                for (int x = 0; x < state[subStateIDsField].arraySize; x++) {
                    int childID = state[subStateIDsField][x].intValue;
                    newElement.directoryChildIDs.Add(childID);
                }

                for (int x = 0; x < state[assetObjectsField].arraySize; x++) {
                    int childID = state[assetObjectsField][x][idField].intValue;
                    if (newElement.objectChildIDsWithoutDuplicates.Contains(childID)) {
                        newElement.objectChildIDsWithoutDuplicates.Add(childID);
                    }
                }

                newElement.parentID = GetStateParentBrute(stateID);

            }
            return stateMachineDirectoryTree;
            
        }
        Dictionary<int, SelectionElement> BuildDirectoryTreeElements () {

            if (viewTab == 0) {
                return BuildDirectoryTreeElementsForStateMachineView();
            }
            else {
                return BuildDirectoryTreeElementsForProjectView();//selectionSystem);
            }
        }



        public const string paramsField = "parameters", messageBlocksField = "messageBlock";
        string objRefField = "objRef", idField = "id", isCopyField = "isCopy";
        string mainPackIDField = "mainPackID";
        string hiddenIDsField = "hiddenIDs";
        
        
        static readonly string[] toolbarIcons = new string[] {
            "Folder Icon", //add directory
            "_Popup", //import settings
            "Toolbar Plus", 
            "Toolbar Minus", //add / remove from list
            "ScaleConstraint Icon", //duplicate
            "animationvisibilitytoggleon", //hide / unhide
            "SceneViewFx", // open preview
        };
        static readonly string[] toolbarIconsHints = new string[] {
            "Add New State.\nWhen naming the new state add '*' after the name\nto start typing the conditional for the new state as well", 
            "Open import settings for selection (if available...)",
            "Add Selected To State Machine",
            "Remove Selected From State Machine",
            "Duplicate Selection",
            "Toggle the hidden status of the selection (if any, else all shown elements)",
            "Open Preview View",
        };

        GUIContent[] _tGUIs;
        GUIContent[] toolbarGUIs {
            get {
                if (_tGUIs == null) {
                    int l = toolbarIcons.Length;
                    _tGUIs = new GUIContent[l];
                    for (int i = 0; i < l; i++) _tGUIs[i] = new GUIContent(EditorGUIUtility.IconContent(toolbarIcons[i]).image, toolbarIconsHints[i]);
                }
                return _tGUIs;
            }
        }

        PopupList.InputData packsPopup;
        GUILayoutOption iconWidth = GUILayout.Width(20);
        
        
        bool removeOrAdd, duplicated;
        AssetObjectPackInfo packInfo;
        
        string errors;
        public EditorProp so;
        
        ElementSelectionSystem selectionSystem = new ElementSelectionSystem();

        EditorProp baseState { get { return allStates[0]; }} 
        
        EditorProp curState, curParentState;//, packsProp;
        StateToggler hiddenIDsToggler = new StateToggler();
        bool createdDirectory, hiddenToggleSuccess;

        bool namedNewDir;
        int viewTab;
        public bool showingHidden { get { return viewTab == 2; } }
        GUIContent[] viewTabGUIs = new GUIContent[] {
            new GUIContent("State Machine"),
            new GUIContent("Project"),
            new GUIContent("Hidden"),
        };

        GUIContent packtypegui = new GUIContent("<b>Type:</b> ");
        GUIContent helpGUI = new GUIContent(" Help ");

        

        public override bool HasPreviewGUI() { 
            return previewOpen;
        }
        public override void OnInteractivePreviewGUI(Rect r, GUIStyle background) { 
            if (preview != null) preview.OnInteractivePreviewGUI(r, background); 
        }
        public override void OnPreviewSettings() { 
            
            if (preview != null) preview.OnPreviewSettings();
        }

        public bool previewOpen;
        Editor preview;
        BindingFlags getPreviewFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
        Type inspectorWindowType = Assembly.GetAssembly(typeof(Editor)).GetType("UnityEditor.InspectorWindow");

        
        //no repeat ids (force same pack)
        public void RebuildPreviewEditor () {
        
            if (preview != null) Editor.DestroyImmediate(preview);
            
            if (packInfo == null) {
                return;
            } 
            if (!previewOpen) {
                return;
            }

            IEnumerable<int> elementsInSelection = selectionSystem.GetIDsInSelection_NoRepeats();

            if (elementsInSelection == null || elementsInSelection.Count() == 0) return;
        
            if (packInfo.pack.isCustom) {
                return;
            }
            
            preview = Editor.CreateEditor( elementsInSelection.Generate(e => packInfo.GetObjectRefForID(e) ).ToArray());

            preview.HasPreviewGUI();
            preview.OnInspectorGUI();
            preview.OnPreviewSettings();



            //auto play single selection for animations
            // if (packInfo.pack.assetType == "AnimationClip") {     
            if (packInfo.assetType == typeof(AnimationClip)) {     
            
                if (elementsInSelection.Count() == 1) {
                    // preview_editor.m_AvatarPreview.timeControl.playing = true
                    var avatarPreview = preview.GetType().GetField("m_AvatarPreview", getPreviewFlags).GetValue(preview);
                    var timeControl = avatarPreview.GetType().GetField("timeControl", getPreviewFlags).GetValue(avatarPreview);
                    var setter = timeControl.GetType().GetProperty("playing", getPreviewFlags).GetSetMethod(true);
                    setter.Invoke(timeControl, new object[] { true });
                }
            }
        }
        public void PreviewToggle () {
            TogglePreview(!previewOpen);
            if (previewOpen) {
                RebuildPreviewEditor();
            }
        }

        
        object[] nullParams = new object[] {};
        void TogglePreview(bool enabled)
        {
            previewOpen = enabled;
            //get first inspector window with a preview window
            var inspector = Resources.FindObjectsOfTypeAll(inspectorWindowType).Where( o => o.GetType().GetField("m_PreviewWindow", getPreviewFlags) != null).ToArray()[0];

            //get the preview resizer on that window
            var previewResizer = inspector.GetType().GetField("m_PreviewResizer", getPreviewFlags).GetValue(inspector);
            
            //toggle expand if not already expanded
            var t = previewResizer.GetType();
            bool expanded = (bool)t.GetMethod("GetExpanded", getPreviewFlags).Invoke(previewResizer, nullParams);
            if (expanded != enabled) t.GetMethod("ToggleExpanded", getPreviewFlags).Invoke(previewResizer, nullParams);    
        }

        void MakeBaseState() {
            EditorProp state = allStates.AddNew();
            state[nameField].SetValue("Base State");
            state[conditionsBlockField].SetValue(string.Empty);
            state[stateIDField].SetValue(0);            
        }

        GUIContent mainPackGUI;

        
        
        
        void RebuildForCurrentMainPack(int packID) {

            if (allStates.arraySize == 0) {
                MakeBaseState ();
            }
            PacksManager packsManager = PacksManager.instance;

            bool errored = packsManager == null || packID == -1;

            packInfo = null;
            

            if (!errored) {
                int ourPackIndex = -1;
                for (int i = 0; i < packsManager.packs.Length; i++) {
                    bool isOurs = packsManager.packs[i].id == packID;
                    if (isOurs) {
                        ourPackIndex = i;
                    }
                    packsPopup.NewOrMatchingElement(packsManager.packs[i].name, isOurs);
                }

                EditorProp packsProp = new EditorProp (new SerializedObject ( packsManager ) )[ PacksManagerEditor.packsField ];

                packInfo = new AssetObjectPackInfo(packsManager.packs[ourPackIndex], packsProp[ourPackIndex] );

            }

            mainPackGUI = new GUIContent(!errored ? packInfo.pack.name : "None");
            if (!errored) 
                mainPackGUI.image = EditorGUIUtility.ObjectContent(null, packInfo.assetType).image;            

            hiddenIDsToggler.OnEnable(so[hiddenIDsField]);
        }

        
        void ResetStateMachine () {
            so[hiddenIDsField].Clear();
            so[allStatesField].Clear();
        }

        

        void UpdateAllAssetObjectParametersIfDifferentFromDefaults() {
            bool debug = true;
            for (int i = 0; i < allStates.arraySize; i++) {
                EditorProp aos = allStates[i][assetObjectsField];
                for (int x = 0; x < aos.arraySize; x++) {
                    PacksManagerEditor.UpdateAssetObjectParametersIfDifferentFromDefaults(aos[x], packInfo.packProp, false, debug);
                    debug = false;
                }
            }
        }
        
        void OnEnable () {
            

            so = new EditorProp (serializedObject);
            packsPopup = new PopupList.InputData { m_OnSelectCallback = OnSwitchPackCallback };
            errors = PacksManagerEditor.GetPacksErrors();
            
            RebuildForCurrentMainPack(so[mainPackIDField].intValue);
                        
            UpdateAllAssetObjectParametersIfDifferentFromDefaults();
            ResetNewRecursive();

            CheckAllAOsForNullObjectRefs ();
            so.SaveObject(); 

            selectionSystem.OnEnable(so, GetPoolElements, BuildDirectoryTreeElements, GetIDsInDirectory, RebuildPreviewEditor, OnDirDragDrop, ExtraToolbarButtons, OnCurrentPathChange);
            
        }

        
        public override void OnInspectorGUI() {

            


            
            base.OnInspectorGUI();

            GUIUtils.StartCustomEditor();
            
            PacksManagerEditor.DrawPacksErrors(errors);
            
            
            bool forceRebuild, forceReset;
            DrawStateMachine(out forceRebuild, out forceReset);  
            
            if (CheckForPackSwitch()) {
                forceRebuild = forceReset = true;
            }

            selectionSystem.CheckRebuild (forceRebuild, forceReset);
            


            
            GUIUtils.EndCustomEditor(so, true);//forceRebuild);            
        }
        

        void CopyAssetObject(EditorProp ao, EditorProp toCopy) {    
            ao.CopySubProps ( toCopy, new string[] { 
                idField, 
                objRefField, 
                messageBlocksField, 
                conditionsBlockField, 
                isNewField, 
                nameField 
            } );
            CustomParameterEditor.CopyParameterList(ao[paramsField], toCopy[paramsField]);
        }

        //TODO: handle gen ids

        void DrawStateMachine (out bool forceRebuild, out bool forceReset) {
            forceRebuild = false;
            forceReset = false;

            DrawMainPackTypeAndHelp();

            if (so[mainPackIDField].intValue == -1) {
                GUIUtils.HelpBox("\nChoose the State Machine's Asset Object type.\n", MessageType.Info);
                return;
            }

            bool changedTabView = false;
            if (packInfo.pack.isCustom) {
                if (viewTab != 0) {
                    changedTabView = true;
                    viewTab = 0;
                }
            }
            else {
                changedTabView = GUIUtils.Tabs(viewTabGUIs, ref this.viewTab);

            }

            if (viewTab == 0) 
                forceRebuild = DrawEditToolbar();
            else {
                if (currentAOWindow != null) {
                    currentAOWindow.Close();
                }
            }

            
            selectionSystem.DrawElements(CheckHotKeys);
            
            if (removeOrAdd) {
                if (viewTab == 0) {
                    removeOrAdd = DeleteIndiciesFromState(curState, selectionSystem.GetReferenceIndiciesInSelection(true));
                }
                else if (viewTab == 1) {
                    //no repeats (should be none anywyas)     
                    removeOrAdd = AddElementsToState (baseState, selectionSystem.GetIDsInDirectoryAndSubDirs_NoRepeats("Add Objects", "Selection contains directories, Add all sub-objects?"));               
                }
            }
            
            AddNewCustomAO();

            forceRebuild = forceRebuild || addedCustomAO || namedNewDir || changedTabView || removeOrAdd || duplicated || createdDirectory || hiddenToggleSuccess;
            forceReset = forceRebuild && changedTabView;
            
            // if (addedCustomAO) {
            //     handleRepaintErrors = true;
            // }

            addedCustomAO = false;

            duplicated = false;
            removeOrAdd = false;
            createdDirectory = false;
            hiddenToggleSuccess = false;
            namedNewDir = false;
        }
        bool addedCustomAO;

        GUIContent showAOWindowGUI = new GUIContent("Show AO Window", "Show Asset Object Edit Window");
        GUIContent showSoloMuteGUI = new GUIContent("Show Solo/Mute", "Show Solo/Mute Options");
        

        void DrawMainPackTypeAndHelp () {

            GUIUtils.StartBox();
            EditorGUILayout.BeginHorizontal();        

            GUIUtils.Label(packtypegui, true);
            
            if (GUIUtils.Button(mainPackGUI, GUIStyles.toolbarButton, true)) {
                GUIUtils.ShowPopUpAtMouse(packsPopup);
            }

            GUILayout.FlexibleSpace();
            
            showAOWindow = GUIUtils.ToggleButton(showAOWindowGUI, GUIStyles.toolbarButton, showAOWindow, out _);
            showSoloMute = GUIUtils.ToggleButton(showSoloMuteGUI, GUIStyles.toolbarButton, showSoloMute, out _);
            
            GUILayout.FlexibleSpace();

            if (GUIUtils.Button(helpGUI, GUIStyles.toolbarButton, Colors.selected, Colors.black, true)) {
                HelpWindow.Init();
            }

            EditorGUILayout.EndHorizontal();
            GUIUtils.EndBox();
        }
            

        bool ContainsStateID(int id) {
            for (int i = 0; i < so[allStatesField].arraySize; i++) {
                if (so[allStatesField][i][stateIDField].intValue == id) {
                    return true;
                }
            }
            return false;
        }
        int CreateNewStateID () {
            int id = 0;
            while (ContainsStateID(id)) {
                id++;
            }
            return id;
        }

        void OnDirectoryCreate () {
            if (viewTab == 0) {
                AddNewState(curState, CreateNewStateID());
            }
        }
        
        void OnNameNewElement(int poolIndex, string newName) {
            if (viewTab == 0) {
    
                QuickRenameNewElement(curState, poolIndex, newName);
                so.SaveObject();
                namedNewDir = true;
                
            }            
        }

        void OnCurrentPathChange () {
            if (viewTab == 0) {
                curState = GetStateByID(selectionSystem.currentShownDirectoryID);
                curParentState = FindParentState(curState);
            }
        }


        //for building IDs in set ignore for project view build
        void GetIDsInSet (HashSet<int> ret){

            for (int i = 0; i < allStates.arraySize; i++) {
                
                EditorProp aos = allStates[i][assetObjectsField];
                
                for (int x = 0; x < aos.arraySize; x++) {
                    EditorProp ao = aos[x];
                    
                    
                    int objID = ao[idField].intValue;
                    
                    if (ret.Contains(objID)) continue;
                    
                    ret.Add(objID);
                }
            }
        }
        





        List<SelectionElement> GetPoolElements (int currentDirectoryID){
            List<SelectionElement> toReturn = new List<SelectionElement>();
            if (viewTab == 0) {

                EditorProp displayedState = GetStateByID(currentDirectoryID);
                
                int elementsCount = GetStateTotalCount(displayedState);
            
                for (int i = 0; i < elementsCount; i++) {
                    int id;
                    bool isNew = false, isCopy = false;
                    Action<int> elementPrefix = null;
                    
                    string elName = null;
                    bool isSubstate = i < displayedState[subStateIDsField].arraySize;
                    
                    if (isSubstate) {
                        EditorProp associatedProp = GetStateByID( displayedState[subStateIDsField][i].intValue);
                        id = displayedState[subStateIDsField][i].intValue;
                        isNew = associatedProp[isNewField].boolValue;
                        elName = associatedProp[nameField].stringValue;
                
                    }
                    else {
                        EditorProp associatedProp = GetAOatPoolID(displayedState, i);
                        id = associatedProp[idField].intValue;
                        isCopy = associatedProp[isCopyField].boolValue;
                        isNew = associatedProp[isNewField].boolValue;
                        
                        if (!packInfo.pack.isCustom) {
                            string originalPath = AssetObjectsEditor.RemoveIDFromPath(packInfo.GetOriginalPath(id));    
                            elName = originalPath.Replace("/", "-");
                        }
                        else {
                            elName = associatedProp[nameField].stringValue;
                        }
                        elementPrefix = ExtraElementPrefix;
                    }            
    
                    toReturn.Add(new SelectionElement(true, isSubstate, id, new GUIContent(elName), i, isNew, isCopy, elementPrefix, OnNameNewElement, elName));
                }

            }
            else {

                SelectionElement directoryTreeElement = projectDirectoryTreeElements[currentDirectoryID];
                HashSet<int> ignoreIDs = new HashSet<int>();
                GetIDsInSet(ignoreIDs);
                if (!showingHidden) {
                    ignoreIDs.AddRange(hiddenIDsToggler.stateList, false);
                }

                int i = 0;
                foreach (var directoryID in directoryTreeElement.directoryChildIDs) {
                    if (DirectoryShown(directoryID, ignoreIDs, projectDirectoryTreeElements)) {
                        toReturn.Add(new SelectionElement(true, true, directoryID, projectDirectoryTreeElements[directoryID].gui, i));
                    }
                    i++;
                }
                foreach (var objID in directoryTreeElement.objectChildIDsWithoutDuplicates) {
                    if (ignoreIDs.Contains(objID)) continue;
                    if (hiddenIDsToggler.IsState(objID) != showingHidden) continue;
                    toReturn.Add(new SelectionElement(true, false, objID, new GUIContent( AssetObjectsEditor.RemoveIDFromPath( packInfo.GetFileName(objID) ) ) , i));
                    i++;
                }
            
            }
            return toReturn;
        }

        

        static void GetAllObjectIDsInDirectorySelectionElement (int directoryID, HashSet<int> ret, HashSet<int> ignoreIDs, Dictionary<int, SelectionElement> directoryTree) {
            SelectionElement directory = directoryTree[directoryID];
            foreach (var id in directory.objectChildIDsWithoutDuplicates) {
                if (ignoreIDs.Contains(id)) continue;
                ret.Add(id);
            }
            foreach (var id in directory.directoryChildIDs) {
                GetAllObjectIDsInDirectorySelectionElement(id, ret, ignoreIDs, directoryTree);
            }
        }
        
        //used for hide toggle and add
        void GetIDsInDirectory (int directoryID, HashSet<int> r) {
        
            HashSet<int> ignoreIDs = new HashSet<int>();
            if (viewTab == 0) {

                GetAllObjectIDsInDirectorySelectionElement (directoryID, r, ignoreIDs, stateMachineDirectoryTree);
            }
            else {
                GetIDsInSet(ignoreIDs);

                if (!showingHidden) {
                    ignoreIDs.AddRange(hiddenIDsToggler.stateList, false);
                }

                GetAllObjectIDsInDirectorySelectionElement (directoryID, r, ignoreIDs, projectDirectoryTreeElements);
            }
        }

        static bool DirectoryShown(int directoryID, HashSet<int> ignoreIDs, Dictionary<int, SelectionElement> treeElements) {

            SelectionElement directoryElement = treeElements[directoryID];

            foreach (var id in directoryElement.objectChildIDsWithoutDuplicates) {

                if (!ignoreIDs.Contains(id)) {
                    return true;
                }
            }

            foreach (var id in directoryElement.directoryChildIDs) {

                bool subDirShown = DirectoryShown(treeElements[id].elementID, ignoreIDs, treeElements);
                if (subDirShown) {
                    return true;
                }
            }
                
            return false;
        }





        bool showSoloMute = true;

        
        void ExtraElementPrefix (int poolIndex) {
            if (showSoloMute) {

                int curSubStateCount = curState[subStateIDsField].arraySize;
                if (poolIndex >= curSubStateCount) {
                    DrawSoloMuteElement(curState[assetObjectsField], poolIndex - curSubStateCount);
                }
            }
        }
    
        void ExtraToolbarButtons () {

            hiddenToggleSuccess = createdDirectory = duplicated = removeOrAdd = false;
            
            if (!showingHidden) {
                //add directory
                if (GUIUtils.Button(toolbarGUIs[0], GUIStyles.toolbarButton, iconWidth)) {
                    OnDirectoryCreate();
                    createdDirectory = true;
                }
                

                GUI.enabled = selectionSystem.hasSelection;
                if (!packInfo.pack.isCustom) {
                    OpenImportSettingsButton();
                }
                AddOrRemoveButtons( viewTab );
                GUI.enabled = true;
                
                if (viewTab == 0) DuplicateButton();
            }



            if (viewTab != 0) {
                ToggleHiddenButtonGUI();    
            }
            else {
                if (packInfo.pack.isCustom) {

                    AddCustomObjectButton();
                }
            }
            if (!packInfo.pack.isCustom) {
                CheckPreviewToggle();
            }
        }


        void OpenImportSettingsButton () {
            if (GUIUtils.Button(toolbarGUIs[1], GUIStyles.toolbarButton, iconWidth)) 
                OpenImportSettings(selectionSystem.GetIDsInSelection_NoRepeats());
        }

        void CheckHotKeys (KeyboardListener k) {        
            if (!removeOrAdd) {
                if (viewTab == 0) removeOrAdd = (k[KeyCode.Delete] || k[KeyCode.Backspace]);
                else if (viewTab == 1) removeOrAdd = k[KeyCode.Return];
            }      
            if (!duplicated) {
                duplicated = selectionSystem.hasSelection && (k[KeyCode.D] && (k.command || k.ctrl));
                if (duplicated) DuplicateSelectedElementsInCurrentState();//selectionSystem.selectionSystem.selectedElements);
            }  

            if (!toggleHiddenAttempt) {

                if (!packInfo.pack.isCustom) {

                    toggleHiddenAttempt = selectionSystem.hasSelection && k[KeyCode.H];

                    if (toggleHiddenAttempt) {
                        HashSet<int> selectedElements = selectionSystem.GetIDsInDirectoryAndSubDirs_NoRepeats(
                            "Hide/Unhide Directory", 
                            "Selection contains directories, hidden status of all sub elements will be toggled"
                        ).ToHashSet();
                        hiddenToggleSuccess = hiddenIDsToggler.ToggleState(selectedElements);
                    }
                }
            }
            if (!togglePreview) {
                if (!packInfo.pack.isCustom) {

                    togglePreview = (k[KeyCode.P]);                
                    if (togglePreview) {
                        PreviewToggle();
                    }
                }
            }
        
        }
        bool toggleHiddenAttempt;

        
        void AddFilesButton () {                
            removeOrAdd = GUIUtils.Button(toolbarGUIs[2], GUIStyles.toolbarButton, iconWidth);
        }
        void RemoveFilesButton () {                
            removeOrAdd = GUIUtils.Button(toolbarGUIs[3], GUIStyles.toolbarButton, iconWidth);
        }
        void AddOrRemoveButtons (int viewTab) {
            if (viewTab == 0) RemoveFilesButton();   
            else if (viewTab == 1) AddFilesButton();
        }

        void DuplicateButton () {
            GUI.enabled = selectionSystem.hasSelection;
            duplicated = GUIUtils.Button(toolbarGUIs[4], GUIStyles.toolbarButton, iconWidth);
            GUI.enabled = true;
        }            

        void ToggleHiddenButtonGUI () {    
            GUI.enabled = selectionSystem.hasSelection;
            toggleHiddenAttempt = GUIUtils.Button(toolbarGUIs[5], GUIStyles.toolbarButton, iconWidth);
            GUI.enabled = true;
        }
        bool togglePreview;
        void CheckPreviewToggle () {
            togglePreview = GUIUtils.Button(toolbarGUIs[6], GUIStyles.toolbarButton, iconWidth);
        }
        void AddCustomObjectButton () {
            if (GUIUtils.Button(new GUIContent("CO"), GUIStyles.toolbarButton, iconWidth)) {
                newCustomAOPackID = packInfo.pack.id;
            }
        }

        void OnDirDragDrop(SelectionElement receiver, HashSet<SelectionElement> droppedElements) {
            if (viewTab == 0) {
                
                MoveElementsFromCurrentStateToState(receiver, droppedElements);
            }
        }





        string switchedToPackName = null;

        bool CheckForPackSwitch () {
            if (!switchedToPackName.IsEmpty()) {

                int newMainPackID = PacksManager.Name2ID(switchedToPackName);
                so[mainPackIDField].SetValue(newMainPackID);

                ResetStateMachine();
                RebuildForCurrentMainPack(newMainPackID);
                                
                so.SaveObject();
                switchedToPackName = null;

                return true;
            }
            return false;
        }    



        
        public void OnSwitchPackCallback(PopupList.ListElement element) {
            if (EditorUtility.DisplayDialog("Switch Asset Object Type", "Are you sure you want to switch the Asset Object type for this State Machine? this will reset the State Machine", "Ok", "Cancel")) {
                switchedToPackName = element.m_Content.text;
            }
        }





        int newCustomAOPackID = -1;
        void AddNewCustomAO () {
            if (newCustomAOPackID != -1) {
                int id = int.MaxValue;
                ResetNewRecursive();
                
                InitializeNewAssetObject(curState[assetObjectsField].AddNew("New " + packInfo.pack.name), id, null, true, packInfo.packProp, true);
                
                so.SaveObject();
                
                addedCustomAO = true;
                newCustomAOPackID = -1;
            }

        }

        public void OnAddCustomAOCallback(PopupList.ListElement element) {
            newCustomAOPackID = PacksManager.Name2ID(element.m_Content.text);
            
        }

        
        bool DrawEditToolbar (){
            string newStateName=null;
            bool deletedState=false, changedStateName=false, shouldRebuild = false;
            bool drawingBase = curState[stateIDField].intValue == 0;

            if (!drawingBase) {
                GUIUtils.StartBox(1);
                DrawState(curState, drawingBase, out deletedState, out changedStateName, out newStateName);
                GUIUtils.EndBox(1);
            }

            DrawAOEditToolbar();
            
            if (changedStateName) {
                shouldRebuild = true;
            }



            if (deletedState) {
                selectionSystem.ForceBackFolder();
                shouldRebuild = true;
            }
            
            return shouldRebuild;
        }


AssetObjectWindow currentAOWindow;
public bool showAOWindow;
            
        void DrawAOEditToolbar () {
            if (!showAOWindow) {
                if (currentAOWindow != null) {
                    currentAOWindow.Close();
                }
            }
            else {
                if (currentAOWindow == null) {
                    currentAOWindow = AssetObjectWindow.ShowAssetObjectWindow(this, packInfo.packProp, selectionSystem);            
                }
                currentAOWindow.SetCurrentState (curState);
            }
        }

        public const string assetObjectsField = "assetObjects", nameField = "name", conditionsBlockField = "conditionBlock";
        
        public const string subStateIDsField = "subStatesIDs";
        public const string stateIDField = "stateID";
        public const string allStatesField = "allStates";
        public const string isNewField = "isNew";

        
        EditorProp GetStateByID (int id) {
            for (int i = 0; i < allStates.arraySize; i++) {
                if (allStates[i][stateIDField].intValue == id) {
                    return allStates[i];
                }
            }
            return null;
        }



        
        void ResetNewRecursive (){
            for (int i = 0; i < so[allStatesField].arraySize; i++) {
                so[allStatesField][i][isNewField].SetValue(false);
                for (int x = 0; x < so[allStatesField][i][assetObjectsField].arraySize; x++) {
                    so[allStatesField][i][assetObjectsField][x][isNewField].SetValue(false);
                }
            }
        }

        public static EditorProp GetAOatPoolID (EditorProp state, int poolIndex) {
            int aoIndex = poolIndex - state[subStateIDsField].arraySize;
            if (aoIndex < 0) {
                return null;
            }
            return state[assetObjectsField][aoIndex];
        }


        
        void DuplicateSelectedElementsInCurrentState (){

            IEnumerable<int> elementRefIndiciesInSelection = selectionSystem.GetReferenceIndiciesInSelection(true);
            
            int newAddedElementsOffset = 0;
            
            foreach (var elementRefIndex in elementRefIndiciesInSelection) {

                int duplicateIndex = elementRefIndex;// elementToDuplicate.refIndex;
                int adjustedIndex = duplicateIndex + newAddedElementsOffset;


                if (adjustedIndex >= curState[subStateIDsField].arraySize) {

                    int aoIndex = adjustedIndex - curState[subStateIDsField].arraySize;

                    EditorProp aoList = curState[assetObjectsField];
                    EditorProp aoToDuplicate = aoList[aoIndex];

                    EditorProp newAO = aoList.InsertAtIndex(aoIndex + 1);

                    CopyAssetObject(newAO, aoToDuplicate);

                    newAO[isCopyField].SetValue(true);

                }
                else {
                    EditorProp stateToDuplicate = GetStateByID( curState[subStateIDsField][adjustedIndex].intValue);

                    string stateToDuplicateName = stateToDuplicate[nameField].stringValue;

                    string newName = stateToDuplicateName + " Copy";
                    
                    EditorProp newState = allStates.AddNew(newName);
                    

                    int newID = CreateNewStateID();

                    curState[subStateIDsField].InsertAtIndex(adjustedIndex+1).SetValue(newID);
                    
                    CopyState(newState, stateToDuplicate, false, false, newID );    

                    
                }
                newAddedElementsOffset++;
            }
        }

        void CopyList (EditorProp list, EditorProp copyList, System.Action<EditorProp, EditorProp> copyFn ) {
            list.Clear();
            for (int i = 0; i < copyList.arraySize; i++) copyFn(list.AddNew(), copyList[i]);
        }

        void CopyState(EditorProp thisState, EditorProp toCopy, bool doName, bool doAOs, int newID) {
            thisState.CopySubProps(
                toCopy, 
                doName ? 
                    new string[] { 
                        nameField, conditionsBlockField, isNewField
                        
                    } 
                    : 
                    new string[] { 
                        conditionsBlockField, isNewField
                        
                    }
            );
            thisState[stateIDField].SetValue(newID);


            if (doAOs) {
                CopyList(thisState[assetObjectsField], toCopy[assetObjectsField], CopyAssetObject);
            }
            else {
                thisState[assetObjectsField].Clear();
            }

            //copy childern states
            thisState[subStateIDsField].Clear();

            for (int i = 0; i < toCopy[subStateIDsField].arraySize; i++) {

                int newSubID = CreateNewStateID();
                thisState[subStateIDsField].AddNew().SetValue(newSubID);
                CopyState(allStates.AddNew(), GetStateByID(toCopy[subStateIDsField][i].intValue), true, doAOs, newSubID);
            }
            
        }



        


        int GetStateTotalCount (EditorProp state) {
            return state[assetObjectsField].arraySize + state[subStateIDsField].arraySize;
        }
        
        void AddNewState (EditorProp parentState, int newID) {
            ResetNewRecursive();
            
            parentState[subStateIDsField].AddNew().SetValue(newID);
            
            MakeNewStateDefault(so[allStatesField].AddNew("New State"), newID);
        }
        
        void MakeNewStateDefault (EditorProp newState, int newID){
            newState[isNewField].SetValue(true);
            newState[conditionsBlockField].SetValue(string.Empty);
            newState[assetObjectsField].Clear();
            newState[subStateIDsField].Clear();
            newState[stateIDField].SetValue(newID);
        }


        bool DeleteIndiciesFromState (EditorProp state, IEnumerable<int> deleteIndicies) {                    
            if (deleteIndicies.Count() == 0) return false;
            int deleteOption = -1;
            for (int i = GetStateTotalCount(state) - 1; i >= 0; i--) {
                if (deleteIndicies.Contains(i)) {
                    if (i >= state[subStateIDsField].arraySize) {
                        int aoIndex = i - state[subStateIDsField].arraySize;
                        state[assetObjectsField].DeleteAt(aoIndex, "manual delete");
                    }
                    else {
                        DeleteState(state, GetStateByID( state[subStateIDsField][i].intValue), ref deleteOption);
                    }
                }
            }
            return true;
        }
                
        void AddAOsAndSubAOsToBaseList(EditorProp state) {
            for (int i = 0; i < state[assetObjectsField].arraySize; i++) {
                CopyAssetObject(baseState[assetObjectsField].AddNew(), state[assetObjectsField][i]);
            }
            for (int i = 0; i < state[subStateIDsField].arraySize; i++) {
                EditorProp subState = GetStateByID( state[subStateIDsField][i].intValue );
                AddAOsAndSubAOsToBaseList( subState);
            }
        }



        void DeleteSubStatesRecursive (EditorProp deleteState) {

            int childCount = deleteState[subStateIDsField].arraySize;
            int deleteStateID = deleteState[stateIDField].intValue;

            for (int subIndex = 0; subIndex < childCount; subIndex++) {
                int subID = deleteState[subStateIDsField][subIndex].intValue;

                EditorProp subState = GetStateByID(subID);
                //delete recursive first
                DeleteSubStatesRecursive(subState);

            }
            int indexInAll = -1;
            for (int i = 0; i < allStates.arraySize; i++ ) {
                if (allStates[i][stateIDField].intValue == deleteStateID) {
                    indexInAll = i;
                    break;
                }
            }
            allStates.DeleteAt(indexInAll, "deleted state recursive");
        }


        bool DeleteState (EditorProp parentState, EditorProp stateToDelete, ref int preDeleteSelection) {
            if (preDeleteSelection == -1) {
                preDeleteSelection = EditorUtility.DisplayDialogComplex(
                    "Delete State", 
                    "Delete state(s) and asset objects? If keeping asset objects they will be moved to base state", 
                    "Delete And Keep", "Cancel", "Delete All"
                );
            }
            switch(preDeleteSelection) {
                case 1: return false;
                case 2: break;
                case 0: AddAOsAndSubAOsToBaseList(stateToDelete); break;
            }
            int stateToDeleteID = stateToDelete[stateIDField].intValue;


            int indexInParent = -1;
            for (int i = 0; i < parentState[subStateIDsField].arraySize; i++ ) {
                
                if (parentState[subStateIDsField][i].intValue == stateToDeleteID) {
                    indexInParent = i;
                    break;
                }
            }

            //delete id from parents list
            parentState[subStateIDsField].DeleteAt(indexInParent, "deleted state");
            
            DeleteSubStatesRecursive(stateToDelete);
            return true;
        }

        void QuickRenameNewElement (EditorProp parentState, int refIndex, string newName) {
            
            EditorProp renamed = null;

            // rename asset object
            if (refIndex >= parentState[subStateIDsField].arraySize) {

                int aoIndex = refIndex-parentState[subStateIDsField].arraySize;
                
                renamed = parentState[assetObjectsField][aoIndex];

                renamed[nameField].SetValue(newName);
                        
            }

            // rename state
            else {


                renamed = GetStateByID( parentState[subStateIDsField][refIndex].intValue );
                
                if (newName.Contains("*")) {
                    string[] split = newName.Split('*');
                    renamed[nameField].SetValue(split[0]);
                    renamed[conditionsBlockField].SetValue(split[1]);
                }
                else {
                    renamed[nameField].SetValue(newName);
                } 
            }
            
            renamed[isNewField].SetValue(false);

        }
        
    
        void MoveElementsFromCurrentStateToState(SelectionElement receiver, HashSet<SelectionElement> elementsToMove)
        {
                        
            if (elementsToMove == null || elementsToMove.Count == 0) return;

            HashSet<int> indicies = elementsToMove.Generate( e => e.refIndex ).ToHashSet();


            int targetStateID = receiver.elementID;


            EditorProp targState = GetStateByID(targetStateID);

            int startingSubStatesCount = curState[subStateIDsField].arraySize;

            for (int i = GetStateTotalCount(curState) - 1; i >= 0; i--) {
            
                if (indicies.Contains(i)) {
                    
                    if (i >= startingSubStatesCount) {
                        int aoIndex = i - startingSubStatesCount;
                        CopyAssetObject(targState[assetObjectsField].AddNew(), curState[assetObjectsField][aoIndex]);
                        curState[assetObjectsField].DeleteAt(aoIndex, " because of moves (substate)");
                    }
                    
                    else {
                        int stateIndex = i;

                        int movedStateID = curState[subStateIDsField][stateIndex].intValue;
                        EditorProp stateMoved = GetStateByID(movedStateID);

                        //add to new state
                        targState[subStateIDsField].AddNew().SetValue(movedStateID);

                        //delete movedState from orig state children
                        curState[subStateIDsField].DeleteAt(stateIndex, " because of moves");
                    }
                }
            }
        }
        
        bool DrawStateName (EditorProp state, bool drawingBase, out string changeName) {
            GUI.enabled = !drawingBase;
            bool changedName = GUIUtils.DrawTextProp(state[nameField], new GUIContent("State Name"), GUIUtils.TextFieldType.Delayed, true, "state name", GUILayout.Width(64));
            GUI.enabled = true;
            changeName = state[nameField].stringValue;
            return changedName;
        }
        EditorProp allStates { get { return so[allStatesField]; } }


        EditorProp FindParentState (EditorProp state) {
            int stateID = state[stateIDField].intValue;
            if (stateID == 0) return null;
            for (int i = 0; i < allStates.arraySize; i++) {
                EditorProp parent = allStates[i];
                for (int x = 0; x < parent[subStateIDsField].arraySize; x++) {
                    if (parent[subStateIDsField][x].intValue == stateID) {
                        return parent;
                    }
                }
            }
            return null;
        }
        
        void DrawState(EditorProp state, bool drawingBase, out bool deletedState, out bool changedName, out string changeName) {
            
            EditorGUILayout.BeginHorizontal();
            changedName = DrawStateName (state, drawingBase, out changeName);
            deletedState = GUIUtils.SmallDeleteButton("Delete State");
            
            EditorGUILayout.EndHorizontal();

            //conditions
            GUIUtils.Space();
            GUIUtils.DrawMultiLineStringProp(state[conditionsBlockField], new GUIContent("Conditions Block"), true, "conditions", GUILayout.MinHeight(32));
            
            if (deletedState) {
                int deleteOption = -1;
                deletedState = DeleteState(FindParentState(state), state, ref deleteOption);
            }
        }


        bool AddElementsToState (EditorProp state, HashSet<int> ids){
            if (ids == null || ids.Count() == 0) return false;
            
            bool reset_i = true;        
            
            foreach (var e in ids) {
            
                UnityEngine.Object objRef = packInfo.GetObjectRefForID(e);
                
                InitializeNewAssetObject(
                    state[assetObjectsField].AddNew(), 
                    e, objRef, 
                    reset_i, 
                    packInfo.packProp, false
                );    
                
                reset_i = false;
            }
            return true;
        }

        void InitializeNewAssetObject (EditorProp ao, int id, UnityEngine.Object obj, bool makeDefault, EditorProp packProp, bool setIsNew) {
            ao[idField].SetValue ( id );
            ao[objRefField].SetValue ( obj );
            ao[isNewField].SetValue ( setIsNew );
            
            //only need to default first one added, the rest will copy the last one 'inserted' into the
            //serialized property array
            if (!makeDefault) return;
            ao[messageBlocksField].SetValue(string.Empty);
            ao[conditionsBlockField].SetValue(string.Empty);
            PacksManagerEditor.UpdateAssetObjectParametersIfDifferentFromDefaults(ao, packProp, true, false);
        }

        void CheckAllAOsForNullObjectRefs (){
            if (!packInfo.pack.isCustom) {
                
                for (int i = 0; i < allStates.arraySize; i++) {
                    EditorProp aos = allStates[i][assetObjectsField];
                    for (int x = 0; x < aos.arraySize; x++) {
                        CheckForNullObjectRefs(aos[x]);
                    }
                }
            }
        }
        
        void CheckForNullObjectRefs(EditorProp ao) {
            if (ao[objRefField].objRefValue == null) {
                UnityEngine.Object o = packInfo.GetObjectRefForID(ao[idField].intValue);
                if (o != null) {
                    Debug.Log("Getting new obj: " + o.name);
                }
                ao[objRefField].SetValue( o );
            }
        }





    
        void OpenImportSettings (IEnumerable<int> idsInSelection) {
            if (packInfo.pack.isCustom) {
                return;
            }


            UnityEngine.Object[] rootAssets = idsInSelection.Generate( e => packInfo.GetRootAssetForID(e)).ToArray();
            Animations.EditImportSettings.CreateWizard(rootAssets);
        }

        string soloField = "solo", muteField = "mute";
        GUIContent muteGUI = new GUIContent("", "Mute");
        GUIContent soloGUI = new GUIContent("", "Solo");
        Color32 soloOff = new Color32(84,114,87,255); 
        Color32 muteOff = new Color32(123,97,67,255);
        Color32 soloOn = Colors.green;
        Color32 muteOn = Colors.yellow; 

        public void DrawSoloMuteElement (EditorProp solodMutedList, int i) {
            EditorProp ao = solodMutedList[i];
            bool changedMute = false;

            bool newMute = GUIUtils.SmallToggleButton(muteGUI, ao[muteField].boolValue, muteOn, muteOff, out changedMute );
            
            if (changedMute) {
                ao[muteField].SetValue( newMute );
                if (newMute) ao[soloField].SetValue(false);
            }
            
            bool changedSolo = false;
            bool newSolo = GUIUtils.SmallToggleButton(soloGUI, ao[soloField].boolValue, soloOn, soloOff, out changedSolo );
            
            if (changedSolo) {
                ao[soloField].SetValue(newSolo);
                if (newSolo) {
                    ao[muteField].SetValue( false );
                    for (int x = 0; x < solodMutedList.arraySize; x++) {
                        if (x == i) continue;
                        solodMutedList[x][soloField].SetValue(false);
                    }
                }
            }
        }


        Dictionary<int, SelectionElement> projectDirectoryTreeElements = new Dictionary<int, SelectionElement>();


        Dictionary<int, SelectionElement>  BuildDirectoryTreeElementsForProjectView () {
            if (projectDirectoryTreeElements.Count == 0) {
                int usedID = 0;
                BuildProjectDirectoryTreeSelectionElement("", "Base", ref usedID, 0, -1);
            }
            return projectDirectoryTreeElements;
        }


        int BuildProjectDirectoryTreeSelectionElement (string path, string dirName, ref int usedID, int folderOffset, int parentID) {
            int myID = usedID;
            usedID++;
            SelectionElement element = new SelectionElement(false, true, myID, new GUIContent(dirName), -1);
            element.parentID = parentID;
            projectDirectoryTreeElements[myID] = element;

            HashSet<string> usedSubDirectoryNames = new HashSet<string>();

            string pathPrefix = path + (path.IsEmpty() ? "" : "/");

            for (int i = 0; i < packInfo.allPaths.Length; i++) {
                string filePath = packInfo.allPaths[i];
                
                if (!filePath.StartsWith(path)) continue;
                if (!filePath.Contains("/")) continue;

                string[] sp = filePath.Split('/');
                
                // is file at this depth
                if (folderOffset == sp.Length - 1) {
                    int objId = AssetObjectsEditor.GetObjectIDFromPath(filePath); 
                    element.objectChildIDsWithoutDuplicates.Add(objId);
                    continue;
                }

                string subDirName = sp[folderOffset];

                // already did this directory
                if (usedSubDirectoryNames.Contains(subDirName)) continue;
                usedSubDirectoryNames.Add(subDirName);
                
                element.directoryChildIDs.Add(BuildProjectDirectoryTreeSelectionElement (pathPrefix + subDirName, subDirName, ref usedID, folderOffset+1, myID));
            }
            return myID;
        }


    }


    
    class AssetObjectPackInfo {
        Dictionary<int, int> id2PathIndex = new Dictionary<int, int>();
        public AssetObjectPack pack;
        public EditorProp packProp;
        public string[] allPaths = new string[0];
        public bool hasErrors;
        public System.Type assetType;
       
        
        public AssetObjectPackInfo (AssetObjectPack pack, EditorProp packProp) {
            this.pack = pack;
            this.packProp = packProp;
            
            hasErrors = PacksManagerEditor.PackHasErrors(pack, packProp);
            if (hasErrors) return;
            assetType = pack.assetType.ToType();
            InitializeAllFilePaths();
        }

        public string GetFileName (int id) {
            string path = GetOriginalPath(id);

            return System.IO.Path.GetFileName(path);
        }

        public string GetOriginalPath (int id) {
            if (pack.isCustom) return pack.name + "/";

            int i;
            if (id2PathIndex.TryGetValue(id, out i)) {
                return allPaths[i];
            }
            return pack.name + "/";
        }
        string FullOriginalPath (int id) {
            return pack.dir + GetOriginalPath(id);
        }

        public UnityEngine.Object GetObjectRefForID(int id) {
            if (pack.isCustom) return null;
            return EditorUtils.GetAssetAtPath(FullOriginalPath(id), assetType);
        }

        public UnityEngine.Object GetRootAssetForID(int id) {
            if (pack.isCustom) return null;
            return AssetDatabase.LoadAssetAtPath(FullOriginalPath(id), typeof(UnityEngine.Object));  
        }
        public void InitializeAllFilePaths () {
            if (hasErrors) return;
            if (pack.isCustom) return;

            if (pack.extensions == ".prefab") {
                Debug.Log("gettting prefab");
            }

            allPaths = AssetObjectsEditor.GetAllAssetObjectPaths (pack.dir, pack.extensions, false, out id2PathIndex);
        }
    }

}