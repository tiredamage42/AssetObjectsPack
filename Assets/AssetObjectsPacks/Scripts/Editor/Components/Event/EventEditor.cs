using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System;

namespace AssetObjectsPacks {

    [CustomEditor(typeof(Event))]
    public class EventEditor : Editor {

        public const string objRefField = "objRef", idField = "id", paramsField = "parameters", isCopyField = "isCopy";
        public const string packIDField = "packID";        
        public const string mainPackIDField = "mainPackID";
        public const string multi_edit_instance_field = "multi_edit_instance";
        public const string hiddenIDsField = "hiddenIDs";

        const string messageBlocksField = "messageBlock";
        
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
            "Add New State", 
            "Open import settings for selection",
            "Add Selected To Event",
            "Remove Selected From Event",
            "Duplicate",
            "Toggle the hidden status of the selection (if any, else all shown elements)",
            "Open Preview",
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
        int projectPackIndex, projectPackID;
        Dictionary<int, EventStatePackInfo> packInfos;
        string errors;
        EditorProp so;
        
        ElementSelectionSystem selectionSystem = new ElementSelectionSystem();

        EventStatePackInfo projectViewPack { get { return packInfos[projectPackID]; } }
        EditorProp baseState { get { return allStates[0]; }} 
        EditorProp multiAO { get { return so[multi_edit_instance_field]; } }
        int mainPackID { get { return so[mainPackIDField].intValue; } }

        PacksManager packsManager;
        EditorProp curState, curParentState, packsProp;
        StateToggler hiddenIDsToggler = new StateToggler();
        bool createdDirectory, hiddenToggleSuccess;

        bool namedNewDir;
        int viewTab;
        public bool showingHidden { get { return viewTab == 2; } }
        GUIContent[] viewTabGUIs = new GUIContent[] {
            new GUIContent("Event Pack"),
            new GUIContent("Project"),
            new GUIContent("Hidden"),
        };
        GUIContent packtypegui = new GUIContent("<b>Main Pack Type:</b> ");
        GUIContent helpGUI = new GUIContent(" Help ");

        bool didAOCheck;
        GUIContent[] packTabsGUIs;
        string mainPackName;
        
        SoloMute soloMute = new SoloMute();



        public override bool HasPreviewGUI() { 
            return previewHandler.previewToggler.previewOpen;
        }
        public override void OnInteractivePreviewGUI(Rect r, GUIStyle background) { 
            previewHandler.OnInteractivePreviewGUI(r, background);
        }
        public override void OnPreviewSettings() { 
            previewHandler.OnPreviewSettings();
        }

        

        void OnEnable () {
            so = new EditorProp (serializedObject);
            if (allStates.arraySize == 0) {
                MakeBaseState ();
            }
            packsManager = PacksManager.instance;
            packsProp = new EditorProp (new SerializedObject ( packsManager ) )[ PacksManagerEditor.packsField ];
            errors = PacksManagerEditor.CheckPacksErrors(packsManager, packsProp);
            
            packInfos = GetPacksInfos();
            
            selectionSystem.OnEnable(GetPoolElements, GetElementsInDirectory, previewHandler.RebuildPreviewEditor, OnDirDragDrop, ExtraToolbarButtons, OnCurrentPathChange);
            hiddenIDsToggler.OnEnable(so[hiddenIDsField]);
            previewHandler.OnEnable(packInfos, selectionSystem);
                
            packsPopup = new PopupList.InputData { m_OnSelectCallback = OnSwitchPackCallback };
            
            foreach (var p in packInfos.Keys) {
                packsPopup.NewOrMatchingElement(packInfos[p].pack.name, p == mainPackID);
            }
            multiAO[packIDField].SetValue(-1);   
            
            UpdatEventStatesAgainstDefaults(packInfos);
            CheckAllAOsForNullObjectRefs (packInfos);
            ResetNewRecursive();

            so.SaveObject(); 

            packTabsGUIs = packInfos.Keys.Generate(k => new GUIContent( packInfos[k].pack.name ) ).ToArray();
            mainPackName = PacksManager.ID2Name(mainPackID, false);
        }

        void MakeBaseState() {
            EditorProp state = allStates.AddNew();
            state[nameField].SetValue("Base State");
            state[conditionsBlockField].SetValue("");
            state[stateIDField].SetValue(0);      
            so.SaveObject();      
        }
        
        public override void OnInspectorGUI() {
            //base.OnInspectorGUI();
            GUIUtils.StartCustomEditor();
            
            FixErrorsAutomatic();
            
            PacksManagerEditor.DrawPacksErrors(errors);
            
            bool forceRebuild, forceReset;
            
            DrawEvent(out forceRebuild, out forceReset);          
            
            selectionSystem.CheckRebuild (forceRebuild, forceReset);
            
            GUIUtils.EndCustomEditor(so);
        }

        void FixErrorsAutomatic () {
            if (didAOCheck) return;
            didAOCheck = true;
            //UpdatEventStatesAgainstDefaults(packInfos);
            //CheckAllAOsForNullObjectRefs (packInfos);
            //ResetNewRecursive();
            //so.SaveObject();
        }

        void CopyAssetObject(EditorProp ao, EditorProp toCopy) {    
            ao.CopySubProps ( toCopy, new string[] { idField, objRefField, packIDField } );
            CustomParameterEditor.CopyParameterList(ao[paramsField], toCopy[paramsField]);
        }


        //TODO: handle gen ids

        void DrawEvent (out bool forceRebuild, out bool forceReset) {
            forceRebuild = false;
            forceReset = false;

            bool hasMainPack = DrawMainPackTypeAndHelp();

            if (!hasMainPack) {
                GUIUtils.HelpBox("\nChoose the event's main pack type.\n", MessageType.Info);
                return;
            }

            if (viewTab == 0) 
                forceRebuild = DrawEditToolbar();

            bool changedTabView = GUIUtils.Tabs(viewTabGUIs, ref this.viewTab);
                
            if (viewTab != 0) {
                GUIUtils.Space();
                if (GUIUtils.Tabs(packTabsGUIs, ref projectPackIndex)) {
                    projectPackID = packInfos[projectPackIndex].pack.id;
                    changedTabView = true;
                }
            }
            
            selectionSystem.DrawElements(CheckHotKeys);
            
            if (removeOrAdd) {
                if (viewTab == 0) {
                    removeOrAdd = DeleteIndiciesFromState(curState, selectionSystem.GetPoolIndiciesInSelection(true, -1));    
                }
                else if (viewTab == 1) {
                    //no repeats (should be none anywyas)     
                    removeOrAdd = AddElementsToState (baseState, selectionSystem.GetElementsInSelectionDeep("Add Objects", "Selection contains directories, Add all sub-objects?"));               
                }
            }


            forceRebuild = forceRebuild || namedNewDir || changedTabView || removeOrAdd || duplicated || createdDirectory || hiddenToggleSuccess;
            forceReset = forceRebuild && changedTabView;

            duplicated = false;
            removeOrAdd = false;
            createdDirectory = false;
            hiddenToggleSuccess = false;
            namedNewDir = false;
        }

        bool DrawMainPackTypeAndHelp () {

            GUIUtils.StartBox();
            EditorGUILayout.BeginHorizontal();        

            GUIUtils.Label(packtypegui, true);

            bool hasMainPack = mainPackName != null;
            if (GUIUtils.Button(new GUIContent(hasMainPack ? mainPackName : "None"), GUIStyles.toolbarButton, true))
                GUIUtils.ShowPopUpAtMouse(packsPopup);
            
            GUILayout.FlexibleSpace();

            if (GUIUtils.Button(helpGUI, GUIStyles.toolbarButton, Colors.selected, Colors.black, true)) 
                HelpWindow.Init();

            EditorGUILayout.EndHorizontal();

            GUIUtils.EndBox();
            return hasMainPack;
        }
            

        bool ContainsID(int id) {
            for (int i = 0; i < so[allStatesField].arraySize; i++) {
                if (so[allStatesField][i][stateIDField].intValue == id) {
                    return true;
                }
            }
            return false;
        }
        int CreateNewID () {
            int id = 0;
            while (ContainsID(id)) {
                id++;
            }
            return id;
        }

        void OnDirectoryCreate () {
            if (viewTab == 0) {
                NewEventState(curState, CreateNewID());
            }
        }
        
        void OnNameNewDirectory(int poolIndex, string newName) {
            if (viewTab == 0) {
                
                QuickRenameNewEventState(curState, poolIndex, newName);
                so.SaveObject();
                namedNewDir = true;
                
            }            
        }

        void OnCurrentPathChange () {
            if (viewTab == 0) {
                curState = GetEventStateByPath(selectionSystem.curPath);
                curParentState = (curState != baseState) ? GetEventStateByPath(selectionSystem.parentPath) : null;
            }
        }


        //for building IDs in set ignore for project view build
        void GetIgnoreIDs (HashSet<int> ret, int packIDFilter) {
            for (int i = 0; i < allStates.arraySize; i++) {
                EditorProp aos = allStates[i][assetObjectsField];
                for (int x = 0; x < aos.arraySize; x++) {
                    EditorProp ao = aos[x];
                    int packID = ao[packIDField].intValue;
                    if (packID != packIDFilter) continue;
                    int objID = ao[idField].intValue;
                    if (ret.Contains(objID)) continue;
                    ret.Add(objID);
                }
            }
        }
        
        void GetAllEventElements_ (EditorProp state, bool useRepeats, HashSet<Vector2Int> ret) {
            for (int x = 0; x < state[assetObjectsField].arraySize; x++) {
                Vector2Int el = new Vector2Int(state[assetObjectsField][x][idField].intValue, state[assetObjectsField][x][packIDField].intValue);
                if (!useRepeats && ret.Contains(el)) continue;
                ret.Add(el);
            }
            for (int i = 0; i < state[subStateIDsField].arraySize; i++) {
                GetAllEventElements_(GetEventByID(state[subStateIDsField][i].intValue), useRepeats, ret);
            }
        }
        
        void GetElementsInDirectory (string directory, bool useRepeats, HashSet<Vector2Int> r) {
            if (viewTab == 0) {
                GetAllEventElements_(GetEventStateByPath(directory), useRepeats, r);            
            }
            else {
                HashSet<int> ignoreIDs = new HashSet<int>();
                GetIgnoreIDs(ignoreIDs, projectPackID);

                for (int i = 0; i < projectViewPack.allPaths.Length; i++) {
                    string p = projectViewPack.allPaths[i];
                    if (!p.StartsWith(directory)) continue;
                    int id = AssetObjectsEditor.GetObjectIDFromPath(p); 
                    if (hiddenIDsToggler.IsState(new Vector2Int(id, projectPackID)) != showingHidden) continue;
                    if (ignoreIDs.Contains(id)) continue;
                    r.Add( new Vector2Int(id, projectPackID) );
                }
            }
        }

        IEnumerable<SelectionElement> GetPoolElements (string atDisplayPath) {

            int c = projectViewPack.allPaths.Length;
            
            EditorProp displayedEvent = null;
            HashSet<int> ignoreIDs = new HashSet<int>();
            
            if (viewTab == 0) {
                displayedEvent = GetEventStateByPath(atDisplayPath);
                
                c = GetEventTotalCount(displayedEvent);

            } 
            else {
                if (c != 0) {
                    GetIgnoreIDs(ignoreIDs, projectPackID);
                }
            }
            
            for (int i = 0; i < c; i++) {
                int id;
                string displayPath = "";
                bool isNewDir = false, isCopy = false;
                string renameName = "";
                Action<int> elementPrefix = null;
                Action<int, string> onRename = null;

                int packID = -1;

                if (viewTab == 0) {
                    string elName = null;
                    bool isSubstate = i < displayedEvent[subStateIDsField].arraySize;
                    
                    isCopy = false;
                    if (isSubstate) {
                        id = -1;
                        EditorProp subState = GetEventByID( displayedEvent[subStateIDsField][i].intValue);
                        isNewDir = subState[isNewField].boolValue;
                        elName = subState[nameField].stringValue;
                    }
                    else {
                        EditorProp ao = GetAOatPoolID(displayedEvent, i);
                        id = ao[idField].intValue;
                        packID =  ao[packIDField].intValue;
                        isCopy = ao[isCopyField].boolValue;
                    }            

                    renameName = elName;                    
                    displayPath = atDisplayPath + (atDisplayPath.IsEmpty() ? "" : "/");
                    
                    if (isSubstate) {   
                        displayPath = displayPath + elName + "/";
                    }
                    else {
                        string originalPath = AssetObjectsEditor.RemoveIDFromPath(packInfos[packID].GetOriginalPath(id));
                        //string[] dirName = EditorUtils.DirectoryNameSplit(originalPath);

                        displayPath = displayPath + originalPath.Replace("/", "-");
                    }
                    

                    elementPrefix = ExtraElementPrefix;
                    onRename = OnNameNewDirectory;
                }
                else {
                    packID = projectPackID;
                    if (!atDisplayPath.IsEmpty() && !packInfos[packID].allPaths[i].StartsWith(atDisplayPath)) continue;
                    id = AssetObjectsEditor.GetObjectIDFromPath(packInfos[packID].allPaths[i]); 
                    if (ignoreIDs.Contains(id)) continue;
                    
                    if (hiddenIDsToggler.IsState(new Vector2Int(id, packID)) != showingHidden) continue;
                    
                    displayPath = AssetObjectsEditor.RemoveIDFromPath(packInfos[packID].allPaths[i]);   
                }
                yield return new SelectionElement(id, displayPath, i, isNewDir, isCopy, elementPrefix, onRename, renameName, packID, packID == -1 ? null : packInfos[packID].icon);
            }
        }

        
        void ExtraElementPrefix (int poolIndex) {
            if (viewTab == 0) {
                int c = curState[subStateIDsField].arraySize;
                if (poolIndex >= c) {
                    soloMute.DrawEventStateSoloMuteElement(curState[assetObjectsField], poolIndex - c);
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
                OpenImportSettingsButton();
                AddOrRemoveButtons( viewTab );
                GUI.enabled = true;
                if (viewTab == 0) DuplicateButton();
            }
            if (viewTab != 0) {
                ToggleHiddenButtonGUI();    
            }
            CheckPreviewToggle();
        }


        void OpenImportSettingsButton () {
            if (GUIUtils.Button(toolbarGUIs[1], GUIStyles.toolbarButton, iconWidth)) OpenImportSettings(selectionSystem.GetElementsInSelection());
        }

        void CheckHotKeys (KeyboardListener k) {        
            if (!removeOrAdd) {
                if (viewTab == 0) removeOrAdd = (k[KeyCode.Delete] || k[KeyCode.Backspace]);
                else if (viewTab == 1) removeOrAdd = k[KeyCode.Return];
            }      
            if (!duplicated) {
                duplicated = selectionSystem.hasSelection && (k[KeyCode.D] && (k.command || k.ctrl));
                if (duplicated) DuplicateIndiciesInState(curState, selectionSystem.GetSelectionEnumerable().ToHashSet());
       
            }  

            if (!toggleHiddenAttempt) {
                toggleHiddenAttempt = selectionSystem.hasSelection && k[KeyCode.H];

                if (toggleHiddenAttempt) {
                    HashSet<Vector2Int> selectedElements = selectionSystem.GetElementsInSelectionDeep(
                        "Hide/Unhide Directory", 
                        "Selection contains directories, hidden status of all sub elements will be toggled"
                    ).ToHashSet();
                    hiddenToggleSuccess = hiddenIDsToggler.ToggleState(selectedElements);
                }
            }
            if (!togglePreview) {
                togglePreview = (k[KeyCode.P]);                
                if (togglePreview) {
                    previewHandler.PreviewToggle();
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

        PreviewHandler previewHandler = new PreviewHandler();


        void OnDirDragDrop(IEnumerable<int> dragIndicies, string origDir, string targetDir) {
            if (viewTab == 0) {
                MoveAOsToEventState(baseState, dragIndicies, origDir, targetDir);
            }
        }        
        
        public void OnSwitchPackCallback(PopupList.ListElement element) {
            int newMainPackID = PacksManager.Name2ID(element.m_Content.text);
            so[mainPackIDField].SetValue(newMainPackID);
            foreach (var p in packInfos.Keys) packsPopup.NewOrMatchingElement(packInfos[p].pack.name, p == newMainPackID);
            so.SaveObject();
        }
        
        GUIContent selectEditGUI = new GUIContent("Multi-Edit <b>Selected</b> Objects");
        
        bool DrawEditToolbar (){
            string newStateName=null;
            bool deletedState=false, changedStateName=false, shouldRebuild = false;
            bool drawingBase = StateIsBase(curState);

            if (!drawingBase) {
                GUIUtils.StartBox(1);
                DrawEventState(curState, drawingBase, out deletedState, out changedStateName, out newStateName);
                GUIUtils.EndBox(1);
            }
            DrawAOEditToolbar();
            if (changedStateName) {
                selectionSystem.ChangeCurrentDirectoryName(newStateName);
                shouldRebuild = true;
            }
            if (deletedState) {
                selectionSystem.ForceBackFolder();
                shouldRebuild = true;
            }
            
            return shouldRebuild;
        }

            
        void DrawAOEditToolbar () {

            if (selectionSystem.hasSelection) {

                IEnumerable<int> selected = selectionSystem.GetSelectionEnumerable();

                //int selectionCollection = -1;
                SelectionElement firstSelected = null;
                foreach (var i in selected) {
                    SelectionElement e = selectionSystem.elements[1][i];
                    if (e.id == -1) {
                        return;
                    }
                    if (firstSelected == null) {
                        firstSelected = e;
                    }
                    else {
                        if (e.collectionID != firstSelected.collectionID) {
                            return;
                        }
                    }
                }
                int c = selected.Count();

                //if (selectionSystem.selectionIsOnlyFiles) {
                    //if (selectionSystem.selectionIsSingleCollection) {

                        bool singleFile = c == 1;// selectionSystem.selectionIsSingleFile;
                        GUIContent gui = selectEditGUI;
                        EditorProp ao = multiAO;
                        int packID = firstSelected.collectionID;// selectionSystem.selectionCollection;
                        EventStatePackInfo packInfo = packInfos[packID];
                        
                        if (singleFile) {
                            gui = new GUIContent("<b>" + firstSelected.displayPath + "</b>");
                            ao = GetAOatPoolID(curState, firstSelected.poolIndex);
                            packID = ao[packIDField].intValue;
                        }
                        else {
                            if (ao[packIDField].intValue != packID) {
                                ao[packIDField].SetValue(packID);
                                PacksManagerEditor.AdjustAOParametersToPack(multiAO, packInfo.packProp, true);
                            }
                        }
                        

                        int setParam = -1;
                        GUIUtils.StartBox();
                        GUIUtils.Label(gui);
                        GUIUtils.BeginIndent();
                        DrawAOParamLabels(packInfo, singleFile, out setParam);
                        DrawAOParamFields(packInfo, ao);
                        /*
                        */

                        EditorGUILayout.BeginHorizontal();


                        EditorGUILayout.BeginVertical();
                        EditorGUILayout.BeginHorizontal();
                        GUIUtils.Label(new GUIContent ("Send Messages On Play"));
                        bool setMessages = !singleFile && GUIUtils.SmallButton(new GUIContent("", "Set Values"));
                        EditorGUILayout.EndHorizontal();

                        GUIUtils.DrawMultiLineExpandableString(ao[messageBlocksField], true, "mesage block", 50);
                        //GUIUtils.EndBox(1);
                        EditorGUILayout.EndVertical();

                        EditorGUILayout.BeginVertical();
                        EditorGUILayout.BeginHorizontal();
                        GUIUtils.Label(new GUIContent ("Conditions"));
                        bool setConditions = !singleFile && GUIUtils.SmallButton(new GUIContent("", "Set Values"));
                        EditorGUILayout.EndHorizontal();
                        
                        GUIUtils.DrawMultiLineExpandableString(ao[conditionsBlockField], true, "conditions block", 50);
                        //GUIUtils.EndBox(1);
                        EditorGUILayout.EndVertical();



                        EditorGUILayout.EndHorizontal();
                        

















                        GUIUtils.EndIndent();
                        GUIUtils.EndBox(1);
                        
                        if (setParam != -1) {
                            CopyParameters(selectionSystem.GetPoolIndiciesInSelectionOrAllShown(multiAO[packIDField].intValue).Generate(i=>GetAOatPoolID(curState, i)), multiAO, setParam);
                        }
                        if (setConditions) {
                            CopyConditions(selectionSystem.GetPoolIndiciesInSelectionOrAllShown(multiAO[packIDField].intValue).Generate(i=>GetAOatPoolID(curState, i)), multiAO);

                        }
                        if (setMessages) {
                            CopyMessages(selectionSystem.GetPoolIndiciesInSelectionOrAllShown(multiAO[packIDField].intValue).Generate(i=>GetAOatPoolID(curState, i)), multiAO);
                        }
                    //}
                //}
            }
        }
        
        void CopyParameters(IEnumerable<EditorProp> aos, EditorProp aoCopy, int paramIndex) {
            foreach (var ao in aos) {
                CustomParameterEditor.CopyParameter (ao[paramsField][paramIndex], aoCopy[paramsField][paramIndex] );      
            }
        }
        void CopyMessages(IEnumerable<EditorProp> aos, EditorProp aoCopy) {
            foreach (var ao in aos) {
                ao[messageBlocksField].SetValue(aoCopy[messageBlocksField].stringValue);
            }
        }
        void CopyConditions(IEnumerable<EditorProp> aos, EditorProp aoCopy) {
            foreach (var ao in aos) {
                ao[conditionsBlockField].SetValue(aoCopy[conditionsBlockField].stringValue);
            }
        }    
        
        void DrawAOParamLabels (EventStatePackInfo packInfo, bool drawingSingle, out int setParam) {
            setParam = -1;
            EditorGUILayout.BeginHorizontal();
            for (int i = 0; i < packInfo.paramlabels.Length; i++) {
                GUIUtils.Label(packInfo.paramlabels[i], packInfo.paramWidths[i]);
                //multi set button
                GUI.enabled = !drawingSingle;
                if (GUIUtils.SmallButton(new GUIContent("", "Set Values"))) setParam = i;
                GUI.enabled = true;
            }
            EditorGUILayout.EndHorizontal();
        }
        void DrawAOParamFields (EventStatePackInfo packInfo, EditorProp ao) {
            EditorGUILayout.BeginHorizontal();
            int pc = ao[paramsField].arraySize;
            int pl = packInfo.paramlabels.Length;
            /*
            if (pc != pl) {
                Debug.Log(pc + " ao params");
                if (ao == multiAO) {
                    Debug.Log("draw multi");
                }
            }
             */

            for (int i = 0; i < pl; i++) {
                GUIUtils.DrawProp( CustomParameterEditor.GetParamValueProperty( ao[paramsField][i] ), packInfo.paramWidths[i]);
                GUIUtils.SmallButtonClear();   
            }

            EditorGUILayout.EndHorizontal();
        }

        public const string assetObjectsField = "assetObjects", nameField = "name", conditionsBlockField = "conditionBlock";
        
        public const string subStateIDsField = "subStatesIDs";
        public const string stateIDField = "stateID";
        public const string allStatesField = "allStates";

        
        

        EditorProp GetEventByID (int id) {
            for (int i = 0; i < allStates.arraySize; i++) {
                if (allStates[i][stateIDField].intValue == id) {
                    return allStates[i];
                }
            }
            return null;
        }

        public const string isNewField = "isNew";


        EditorProp GetEventStateByPath(string path) {
            if (path.IsEmpty()) return baseState;
            string[] split = path.Split('/');
            int l = split.Length;
            EditorProp lastState = baseState;

            for (int i = 0; i < l; i++) {
                string checkName = split[i];

                for (int x =0; x < lastState[subStateIDsField].arraySize; x++) {
                    int subID = lastState[subStateIDsField][x].intValue;
                    EditorProp subState = GetEventByID(subID);
                    if (subState[nameField].stringValue == checkName) {
                        lastState = subState;
                        break;
                    }
                }
            }
            return lastState;
        }

        
        void ResetNewRecursive (){
            for (int i = 0; i < so[allStatesField].arraySize; i++) {
                so[allStatesField][i][isNewField].SetValue(false);
            }
        }

        EditorProp GetAOatPoolID (EditorProp state, int poolIndex) {
            return state[assetObjectsField][poolIndex - state[subStateIDsField].arraySize];
        }
        
        void DuplicateIndiciesInState (EditorProp parentState, HashSet<int> indicies) {
            List<Vector2Int> insertIDsAtIndex = new List<Vector2Int>();

            int offset = 0;
            foreach (var i in indicies) {

                int adjustedIndex = i + offset;

                if (adjustedIndex >= parentState[subStateIDsField].arraySize) {

                    int aoIndex = adjustedIndex - parentState[subStateIDsField].arraySize;

                    EditorProp aoList = parentState[assetObjectsField];
                    EditorProp aoToDuplicate = aoList[aoIndex];

                    EditorProp newAO = aoList.InsertAtIndex(aoIndex + 1);

                    CopyAssetObject(newAO, aoToDuplicate);

                    newAO[isCopyField].SetValue(true);

                }
                else {
                    EditorProp stateToDuplicate = GetEventByID( parentState[subStateIDsField][adjustedIndex].intValue);
Debug.Log("0");
                    
                    EditorProp newEventState = allStates.AddNew(stateToDuplicate[nameField].stringValue + " Copy");
                    
                    int newID = CreateNewID();


                    Debug.Log("1");
                    parentState[subStateIDsField].InsertAtIndex(adjustedIndex+1).SetValue(newID);
                    Debug.Log("2");
                    
                    CopyEventState(newEventState, stateToDuplicate, false, false, newID );    
            Debug.Log("3");
                    

                    
                }
                offset++;
            }
        }

        void CopyList (EditorProp list, EditorProp copyList, System.Action<EditorProp, EditorProp> copyFn ) {
            list.Clear();
            for (int i = 0; i < copyList.arraySize; i++) copyFn(list.AddNew(), copyList[i]);
        }

        void CopyEventState(EditorProp es, EditorProp toCopy, bool doName, bool doAOs, int newID) {
            es.CopySubProps(toCopy, doName ? new string[] { nameField, conditionsBlockField, isNewField } : new string[] { conditionsBlockField, isNewField });
            if (doAOs) CopyList(es[assetObjectsField], toCopy[assetObjectsField], CopyAssetObject);
            else {
                es[assetObjectsField].Clear();
            }


            es[stateIDField].SetValue(newID);

            //copy childern


            es[subStateIDsField].Clear();


            for (int i = 0; i < toCopy[subStateIDsField].arraySize; i++) {

                int newSubID = CreateNewID();

                es[subStateIDsField].AddNew().SetValue(newSubID);




                EditorProp newEventState = allStates.AddNew();


                CopyEventState(newEventState, GetEventByID(toCopy[subStateIDsField][i].intValue), true, doAOs, newSubID);
            }
            
        }

        

        void UpdatEventStatesAgainstDefaults(Dictionary<int, EventStatePackInfo> packInfos) {
        
            for (int i = 0; i < allStates.arraySize; i++) {
                EditorProp aos = allStates[i][assetObjectsField];
                for (int x = 0; x < aos.arraySize; x++) {
                    PacksManagerEditor.AdjustAOParametersToPack(aos[x], packInfos[aos[x][packIDField].intValue].packProp, false);
                }
            }
            
        }

        int GetEventTotalCount (EditorProp state) {
            return state[assetObjectsField].arraySize + state[subStateIDsField].arraySize;
        }
        
        void NewEventState (EditorProp parentState, int newID) {
            ResetNewRecursive();
            
            parentState[subStateIDsField].AddNew().SetValue(newID);

            MakeNewEventStateDefault(so[allStatesField].AddNew("New Event State"), newID);
        }
        
        void MakeNewEventStateDefault (EditorProp newEventState, int newID) {
            newEventState[isNewField].SetValue(true);
            newEventState[conditionsBlockField].SetValue("");
            newEventState[assetObjectsField].Clear();
            newEventState[subStateIDsField].Clear();
            newEventState[stateIDField].SetValue(newID);
        }


        bool DeleteIndiciesFromState (EditorProp state, IEnumerable<int> deleteIndicies) {                    
            if (deleteIndicies.Count() == 0) return false;
            int deleteOption = -1;
            for (int i = GetEventTotalCount(state) - 1; i >= 0; i--) {
                if (deleteIndicies.Contains(i)) {
                    if (i >= state[subStateIDsField].arraySize) state[assetObjectsField].DeleteAt(i - state[subStateIDsField].arraySize);
                    else DeleteState(state, GetEventByID( state[subStateIDsField][i].intValue), ref deleteOption);
                }
            }
            return true;
        }
                
        void AddAOsAndSubAOsToBaseList(EditorProp state) {
            for (int i = 0; i < state[assetObjectsField].arraySize; i++) {
                CopyAssetObject(baseState[assetObjectsField].AddNew(), state[assetObjectsField][i]);
            }
            for (int i = 0; i < state[subStateIDsField].arraySize; i++) {
                EditorProp subState = GetEventByID( state[subStateIDsField][i].intValue );
                AddAOsAndSubAOsToBaseList( subState);
            }
        }
        void DeleteSubStatesRecursive (EditorProp deleteState) {

            int childCount = deleteState[subStateIDsField].arraySize;
            int deleteStateID = deleteState[stateIDField].intValue;

            for (int subIndex = 0; subIndex < childCount; subIndex++) {
                int subID = deleteState[subStateIDsField][subIndex].intValue;

                EditorProp subState = GetEventByID(subID);
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
            allStates.DeleteAt(indexInAll);


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
            parentState[subStateIDsField].DeleteAt(indexInParent);
            


            DeleteSubStatesRecursive(stateToDelete);
            return true;
        }

        void QuickRenameNewEventState (EditorProp parentState, int index, string newName) {
            EditorProp renamedState = GetEventByID( parentState[subStateIDsField][index].intValue );

            renamedState[isNewField].SetValue(false);
            if (newName.Contains("*")) {
                string[] split = newName.Split('*');
                renamedState[nameField].SetValue(split[0]);
                renamedState[conditionsBlockField].SetValue(split[1]);
            }
            else renamedState[nameField].SetValue(newName);
        }
        
        void MoveAOsToEventState(EditorProp baseState, IEnumerable<int> indicies, string origDir, string targetDir)
        {
            if (indicies == null || indicies.Count() == 0) return;
            EditorProp origState = GetEventStateByPath(origDir);
            EditorProp targState = GetEventStateByPath(targetDir);

            for (int i = GetEventTotalCount(origState) - 1; i >= 0; i--) {
            
                if (indicies.Contains(i)) {

                    if (i >= origState[subStateIDsField].arraySize) {
                        int aoIndex = i-origState[subStateIDsField].arraySize;
                        CopyAssetObject(targState[assetObjectsField].AddNew(), origState[assetObjectsField][aoIndex]);
                        origState[assetObjectsField].DeleteAt(aoIndex);
                    }
                    
                    else {
                        int stateIndex = i;

                        int copiedIDatIndex = origState[subStateIDsField][stateIndex].intValue;

                        //add to new state
                        targState[subStateIDsField].AddNew().SetValue(copiedIDatIndex);

                        //delete movedState from orig state children
                        origState[subStateIDsField].DeleteAt(stateIndex);
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


        bool StateIsBase (EditorProp state) {
            return state[stateIDField].intValue == 0;
        }
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
        
        void DrawEventState(EditorProp state, bool drawingBase, out bool deletedState, out bool changedName, out string changeName) {
            
            
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
        


        Dictionary<int, EventStatePackInfo> GetPacksInfos () {
            if (packsManager == null) return null;
            int l = packsManager.packs.Length;
            Dictionary<int, EventStatePackInfo> infos = new Dictionary<int, EventStatePackInfo>(l);
            for (int i = 0; i < l; i++) {
                infos.Add(packsManager.packs[i].id, new EventStatePackInfo(packsManager.packs[i], packsProp[i]));
            }
            return infos;
        }

        bool AddElementsToState (EditorProp state, IEnumerable<Vector2Int> elements){
            if (elements == null || elements.Count() == 0) return false;
            bool reset_i = true;        
            foreach (var e in elements) {
                UnityEngine.Object objRef = packInfos[e.y].GetObjectRefForID(e.x);
                Debug.Log(objRef.name);
                InitializeNewAssetObject(
                    state[assetObjectsField].AddNew(), e.x, objRef, reset_i, packInfos[e.y].packProp);    
                reset_i = false;
            }
            return true;
        }

        void InitializeNewAssetObject (EditorProp ao, int id, UnityEngine.Object obj, bool makeDefault, EditorProp packProp) {
            ao[idField].SetValue ( id );
            ao[objRefField].SetValue ( obj );
            ao[packIDField].SetValue ( packProp[PacksManagerEditor.idField].intValue );
            
            //only need to default first one added, the rest will copy the last one 'inserted' into the
            //serialized property array
            if (!makeDefault) return;
            PacksManagerEditor.AdjustAOParametersToPack(ao, packProp, true);
        }

        void CheckAllAOsForNullObjectRefs (Dictionary<int, EventStatePackInfo> packInfos){
            for (int i = 0; i < allStates.arraySize; i++) {
                EditorProp aos = allStates[i][assetObjectsField];
                for (int x = 0; x < aos.arraySize; x++) {
                    CheckForNullObjectRefs(aos[x], packInfos);
                }
            }
        }
        
        
        void CheckForNullObjectRefs(EditorProp ao, Dictionary<int, EventStatePackInfo> packInfos) {
            if (ao[objRefField].objRefValue == null) {
                UnityEngine.Object o = packInfos[ao[packIDField].intValue].GetObjectRefForID(ao[idField].intValue);
                Debug.Log("Getting new obj: " + o.name);
                ao[objRefField].SetValue( o );
            }
        }

        void OpenImportSettings (IEnumerable<Vector2Int> elementsInSelection) {
            int packID = elementsInSelection.First().y;
            //check if all same pack
            foreach (var i in elementsInSelection) {
                if (packID != i.y) return;
            }
            UnityEngine.Object[] rootAssets = elementsInSelection.Generate( e => packInfos[packID].GetRootAssetForID(e.x)).ToArray();

            Animations.EditImportSettings.CreateWizard(rootAssets);
        }

    }


    

    public class EventStatePackInfo {
        Dictionary<int, int> id2PathIndex = new Dictionary<int, int>();
        public AssetObjectPack pack;
        public EditorProp packProp;
        public string[] allPaths = new string[0];
        public GUIContent[] paramlabels = new GUIContent[0];
        public bool hasErrors;
        public Texture icon;
        GUILayoutOption[] _pw;
        public GUILayoutOption[] paramWidths {
            get {
                if (!hasErrors && _pw == null) 
                    _pw = paramlabels.Length.Generate(i => paramlabels[i].CalcWidth(GUIStyles.label)).ToArray();
                return _pw;
            }
        }

        
        public EventStatePackInfo (AssetObjectPack pack, EditorProp packProp) {
            this.pack = pack;
            this.packProp = packProp;
            
            hasErrors = PacksManagerEditor.PackHasErrors(pack, packProp);
            if (hasErrors) return;

            icon = EditorGUIUtility.ObjectContent(null, pack.assetType.ToType()).image;
            
            InitializeAllFilePaths();
            
            paramlabels = pack.defaultParameters.Length.Generate( i => new GUIContent(pack.defaultParameters[i].name) ).ToArray(); 
        }

        public string GetOriginalPath (int id) {
            return allPaths[id2PathIndex[id]];
        }
        string FullOriginalPath (int id) {
            return pack.dir + GetOriginalPath(id);
        }
        public UnityEngine.Object GetObjectRefForID(int id) {
            return EditorUtils.GetAssetAtPath(FullOriginalPath(id), pack.assetType);  
        }
        public UnityEngine.Object GetRootAssetForID(int id) {
            return AssetDatabase.LoadAssetAtPath(FullOriginalPath(id), typeof(UnityEngine.Object));  
        }
        public void InitializeAllFilePaths () {
            if (hasErrors) return;
            allPaths = AssetObjectsEditor.GetAllAssetObjectPaths (pack.dir, pack.extensions, false, out id2PathIndex);
        }
    }
}