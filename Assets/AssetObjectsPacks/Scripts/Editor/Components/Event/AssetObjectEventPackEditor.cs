using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
namespace AssetObjectsPacks {
    [CustomEditor(typeof(Event))]
    public class EventEditor : Editor {
        PopupList.InputData packsPopup;    
        EditorProp multiEditAO, hiddenIDsProp;

        

        static void AddAOsAndSubAOsToBaseList(EditorProp baseEventState, EditorProp eventState) {
            EditorProp aos = eventState[EventState.assetObjectsField];
            EditorProp baseAOs = baseEventState[EventState.assetObjectsField];
            int c = aos.arraySize;
            for (int i = 0; i < c; i++) AssetObjectEditor.CopyAssetObject(baseAOs.AddNew(), aos[i]);
            
            EditorProp subStates = eventState[EventState.subStatesField];
            c = subStates.arraySize;
            for (int i = 0; i < c; i++) AddAOsAndSubAOsToBaseList(baseEventState, subStates[i]);
        }

        static bool DeleteState (EditorProp baseEventState, EditorProp parentState, EditorProp eventState, ref int preDeleteSelection) {
            if (preDeleteSelection == -1) {

                preDeleteSelection = EditorUtility.DisplayDialogComplex(
                    "Delete State", 
                    "Delete state(s) and asset objects? If keeping asset objects they will be moved to base state", 
                    "Delete All", "Cancel", "Delete And Keep"
                );
            }
            switch(preDeleteSelection) {
                case 1: return false;
                case 2: 
                    Debug.Log("Deleted Keep");
                    AddAOsAndSubAOsToBaseList(baseEventState, eventState);
                    break;
                case 0:
                    Debug.Log("Deleted All");
                    break;
            }
            int atIndex = -1;
            EditorProp parentSubstates = parentState[EventState.subStatesField];
            string eventStateName = eventState[EventState.nameField].stringValue;

            int c = parentSubstates.arraySize;

            for (int i = 0; i < c; i++) {
                if (parentSubstates[i][EventState.nameField].stringValue == eventStateName) {
                    atIndex = i;
                    break;
                }
            }
            parentSubstates.DeleteAt(atIndex);
            return true;
        }

        void OnDirectoryCreate (int viewTab, string parentDir) {

            if (viewTab == 0) {

                EditorProp parentState = GetEventStateByPath(parentDir);
                MakeNewEventStateDefault(parentState[EventState.subStatesField].AddNew("New Event State"));
            }
            else {

            }




        }

        static void DrawEventState(EditorProp baseEventState, EditorProp parentState, EditorProp eventState, out bool addedState, out bool deletedState, out bool changedAOState) {
            bool drawingBase = parentState == null;// eventState == baseEventState;
            
            addedState = false;
            changedAOState = false;

            EditorGUILayout.BeginHorizontal();
            GUIUtils.Label(new GUIContent("<b>Current State:</b>"), false);

            GUILayout.FlexibleSpace();



            //if (GUIUtils.Button(new GUIContent("Add Substate"), true, GUIStyles.miniButton)) {
            //    MakeNewEventStateDefault(eventState[EventState.subStatesField].AddNew("New Event State"));
            //    addedState = true;
            //}

            deletedState = false;


            if (!drawingBase){//eventState != baseEventState) {

                if (GUIUtils.Button(new GUIContent("Delete State"), true, GUIStyles.miniButton, Colors.red, Colors.white)) {
                    
                    deletedState = true;
                    
                    

                }
            }
            EditorGUILayout.EndHorizontal();

            GUI.enabled = !drawingBase;// eventState != baseEventState;
            

            GUILayoutOption w = GUILayout.Width(64);
            if (drawingBase) {
                eventState[EventState.nameField].SetValue("Base State");
                eventState[EventState.conditionsBlockField].SetValue("");

            }
            GUIUtils.DrawTextProp(eventState[EventState.nameField], new GUIContent("Name"), w, false);
            if (!drawingBase) {
                GUIUtils.DrawTextProp(eventState[EventState.conditionsBlockField], new GUIContent("Conditions Block"), w, false);
            }
            //GUIUtils.DrawTextProp(eventState[EventState.nameField], new GUIContent("Name"), false, false);
            GUI.enabled = true;
            if (deletedState) {

                int deleteOption = -1;
                deletedState = DeleteState(baseEventState, parentState, eventState, ref deleteOption);
                
            }
        }

        static void MakeNewEventStateDefault (EditorProp newEventState) {
            newEventState[EventState.subStatesField].Clear();
            newEventState[EventState.assetObjectsField].Clear();
        }


        
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
            return true;// preview != null;// && !selectionSystem.isDragging; 
        }
        public override void OnInteractivePreviewGUI(Rect r, GUIStyle background) { 
            if (preview != null) preview.OnInteractivePreviewGUI(r, background); 
        }
        public override void OnPreviewSettings() { 
            if (preview != null) preview.OnPreviewSettings();
        }

        void OnEnable () {
            so = new EditorProp (serializedObject);



            baseEventState = so[Event.baseStateField];

            hiddenIDsProp = so[Event.hiddenIDsField];
            multiEditAO = so[Event.multi_edit_instance_field];

            if (AssetObjectsEditor.packManager == null) return;
            
            int packID = so[Event.pack_id_field].intValue;
            int index;
            AssetObjectsEditor.packManager.FindPackByID(packID, out index);

            Reinitialize( index );
        }
        public override void OnInspectorGUI() {
           // base.OnInspectorGUI();

           //Debug.Log("before inspector");
            

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
                //Debug.Log("before draw");

                
                Draw();
                //Debug.Log("after draw");

            }


            GUIUtils.EndCustomEditor(so);
            //Debug.Log("after inspector");

            //if (!HasPreviewGUI()) {

                //Debug.Log(HasPreviewGUI());
            //}
        }

        EditorProp baseEventState;

        static EditorProp GetEventStateByName ( EditorProp subStates, string name ) {
            //Debug.Log("Looking for: " + name);
            for (int i = 0; i < subStates.arraySize; i++) {

                string subStateName = subStates[i][EventState.nameField].stringValue;

                if (subStateName == name) {
                    return subStates[i];
                }
            }
            Debug.LogError("couldnt find event state: " + name);
            return null;
        }
        

        


        ElementSelectionSystem.Element GetPoolElementAtIndex(int i, int viewTab, string atPath) {
            int id;

            //Debug.Log("0");
            string path = "";
            if (viewTab == 0) {
                path = atPath;


                //Debug.Log("Getting pool element at path: " + path);

                EditorProp state = GetEventStateByPath(path); 
                
                int subStateCount = state[EventState.subStatesField].arraySize;
                if (i < subStateCount) {
                    id = -1;

                    path += (path.IsEmpty() ? "" : "/") + state[EventState.subStatesField][i][EventState.nameField].stringValue;

                    path += "/";//trickfolderedview";


                }
                else {

                    id = AssetObjectEditor.GetID(
                        state[EventState.assetObjectsField][i - subStateCount]
                    );
                    path = path + (path.IsEmpty() ? "" : "/") + AssetObjectsEditor.RemoveIDFromPath(id2path[id]).Replace("/", " | ");

                    //Debug.Log("making file " + path);
                }

            }
            else {
                path = allPaths[i];
                
                id = AssetObjectsEditor.GetObjectIDFromPath(path); 
                path = AssetObjectsEditor.RemoveIDFromPath(path);
            }
            return new ElementSelectionSystem.Element(id, path, i);
        }

        void ReinitializeEventStates(EditorProp baseEventState) {
            for (int i = 0; i < baseEventState[EventState.assetObjectsField].arraySize; i++) {
                AssetObjectEditor.MakeAssetObjectDefault(baseEventState[EventState.assetObjectsField][i], packIndex, false);
            }
            for (int i = 0; i < baseEventState[EventState.subStatesField].arraySize; i++) {
                ReinitializeEventStates(baseEventState[EventState.subStatesField][i]);
            }




        }

        void Reinitialize (int index) {
            this.packIndex = index;


            PackEditor.GetErrorsAndWarnings (packIndex, out errorStrings, out warningStrings, out noIDsCount);
            
            paramlabels = new GUIContent[0];
            paramWidths = new GUILayoutOption[0];
                
            if (errorStrings.Length == 0) {

                PackEditor.GetValues(packIndex, out _, out objectsDirectory, out fileExtensions, out assetType);

                InitializeAllFilePaths();

                ReinitializeEventStates(baseEventState);
                
                AssetObjectEditor.MakeAssetObjectDefault(multiEditAO, packIndex, true);
                paramlabels = PackEditor.GUI.GetDefaultParamNameGUIs(packIndex);
                

                ElementSelectionSystem.ViewTabOption[] viewTabOptions = new ElementSelectionSystem.ViewTabOption[] {
                    new ElementSelectionSystem.ViewTabOption( new GUIContent("Event Pack"), false ),
                    new ElementSelectionSystem.ViewTabOption( new GUIContent("Project"), true )
                };
                 
                selectionSystem.Initialize(viewTabOptions, hiddenIDsProp, GetPoolElementAtIndex, GetPoolCount, GetIgnoreIDs, RebuildPreviewEditor, OnDirDragDrop, OnDirectoryCreate, ExtraToolbarButtons);
                initializeAfterPackChange = true;
            }

            PacksManager pm = AssetObjectsEditor.packManager;
            packsPopup = new PopupList.InputData { m_OnSelectCallback = OnSwitchPackCallback };
            int l = pm.packs.Length;
            for (int i = 0; i < l; i++) packsPopup.NewOrMatchingElement(pm.packs[i].name, index == i);        
        }

        void ExtraToolbarButtons (int viewTab, GUIStyle s) {

            if (!selectionSystem.showingHidden) {
            
                GUI.enabled = selectionSystem.justFilesSelected;
                
                if (ImportSettingsButton(s)) OpenImportSettings();
                removeOrAdd = AddRemoveButton(viewTab, s);
                
                GUI.enabled = true;
            }
        }

        void OnDirDragDrop(IEnumerable<int> dragIndicies, string origDir, string targetDir, int viewTab) {
            if (viewTab == 0) {
                //Debug.Log("list view drag to dir " + targetDir);
                MoveAOsToEventState(dragIndicies, origDir, targetDir);
            }
            else if (viewTab == 1) {
                Debug.Log("project view drag to dir " + targetDir);
            }
        
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
                    baseEventState[EventState.assetObjectsField].Clear();
                    baseEventState[EventState.subStatesField].Clear();
                    
                    Reinitialize(i);             
                    break;
                }
            }
        }


        //for building IDs in set ignore for project view build
        void GetAllEventIDs (EditorProp eventState, HashSet<int> ret) {
            EditorProp aos = eventState[EventState.assetObjectsField];
            int c = aos.arraySize;
            for (int i = 0; i < c; i++) ret.Add( AssetObjectEditor.GetID(aos[i]));

            EditorProp subStates = eventState[EventState.subStatesField];
            c = subStates.arraySize;
            for (int i = 0; i < c; i++) GetAllEventIDs(subStates[i], ret);
        }
        
        HashSet<int> GetIgnoreIDs (int viewTab) {
            if (viewTab == 1) {
                HashSet<int> ret = new HashSet<int>();
                GetAllEventIDs(baseEventState, ret);
                return ret;
            }
            return null;
        }


        int GetPoolCount (int viewTab, string directory) {
            if (viewTab == 0) {
                EditorProp eventState = GetEventStateByPath(directory);
                return eventState[EventState.assetObjectsField].arraySize + eventState[EventState.subStatesField].arraySize;
            }
            return allPaths.Length;
        }

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

        void InitializeAllFilePaths () {
            allPaths = AssetObjectsEditor.GetAllAssetObjectPaths (objectsDirectory, fileExtensions, false, out id2path);
        }

        void CustomToolbar (EditorProp curParentEventState, EditorProp curEventState, int viewTab, out bool addedState, out bool deletedState, out bool changedAOState){
            addedState = deletedState = changedAOState = false;
            if (viewTab == 0) {
                GUIUtils.StartBox(0);
                DrawMultiEditGUI(curParentEventState, curEventState, out addedState, out deletedState, out changedAOState);
                GUIUtils.EndBox(0);
            }
        }


        GUILayoutOption iconWidth = GUILayout.Width(20);
        bool ImportSettingsButton (GUIStyle s) {
            GUIContent c = EditorGUIUtility.IconContent("_Popup", "Open import settings on selection (if any)");
            return GUIUtils.Button(c, s, iconWidth);
        }
        bool AddRemoveButton (int viewTab, GUIStyle s) {
            GUIContent c = null;
            if (viewTab == 0) c = EditorGUIUtility.IconContent("Toolbar Minus", "Remove Selected From Event");    
            else if (viewTab == 1) c = EditorGUIUtility.IconContent("Toolbar Plus", "Add Selected To Event");
            return GUIUtils.Button(c, s, iconWidth);
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
            return (!singleSelection || selectedID == -1) ? multiEditAO : eventState[EventState.assetObjectsField][selectedPoolID - eventState[EventState.subStatesField].arraySize];
        }
        void GetCurrentAndParentStates (out EditorProp parentState, out EditorProp curState) {
            parentState = null;
            curState = GetEventStateByPath(selectionSystem.curPath);
            if (curState != baseEventState) parentState = GetEventStateByPath(selectionSystem.parentPath);
        }
        EditorProp GetEventStateByPath(string path) {
            if (path.IsEmpty()) return baseEventState;
            EditorProp lastState = baseEventState;
            string[] split = path.Split('/');
            for (int i = 0; i < split.Length; i++) lastState = GetEventStateByName(lastState[EventState.subStatesField], split[i]);
            return lastState;
        }




        
        void DrawMultiEditGUI (EditorProp curParentEventState, EditorProp curEventState, out bool addedState, out bool deletedState, out bool changedAOState){

            DrawEventState(baseEventState, curParentEventState, curEventState, out addedState, out deletedState, out changedAOState);
                        
            if (deletedState) {
                selectionSystem.ForceBackFolder();
            }

            bool hasSelection = selectionSystem.hasSelection;
            bool singleSelection = selectionSystem.singleSelection;
            ElementSelectionSystem.Element selectedElement = selectionSystem.selectedElement;

            GUIUtils.Space(1);
            int setProp = -1;
            GUI.enabled = !(hasSelection && singleSelection && selectedElement.id == -1);

            GUIUtils.Label(GetEditToolbarTitle (hasSelection, singleSelection, selectedElement.path), false);

            bool setCondition;            
            AssetObjectEditor.GUI.DrawAssetObjectMultiEditView(hasSelection && singleSelection, GetAOPropForEditToolbar (curEventState, singleSelection, selectedElement.id, selectedElement.poolIndex), paramWidths, out setCondition, out setProp, paramlabels);
            GUI.enabled = true;
            
            if (setProp != -1) AssetObjectEditor.CopyParameters(GetAOPropsSelectOrAll( curEventState ), multiEditAO, setProp);
        }

        

        void Draw (){
            InitializeAfterPackChange();

            KeyboardListener kbListener = new KeyboardListener();
            ElementSelectionSystem.Inputs inputs = new ElementSelectionSystem.Inputs();

            int viewTab = selectionSystem.viewTab;

            bool generateNewIDs = PackEditor.GUI.DrawErrorsAndWarnings(errorStrings, warningStrings, noIDsCount, packIndex);
           
            if (errorStrings.Length != 0) return;
                        
            selectionSystem.DrawToolbar(kbListener, inputs);

            bool addedState, deletedState, changedAOState;

            EditorProp curParentEventState = null, curEventState = null;
            if (viewTab == 0) GetCurrentAndParentStates(out curParentEventState, out curEventState);
            
            CustomToolbar(curParentEventState, curEventState, viewTab, out addedState, out deletedState, out changedAOState);
        
            selectionSystem.DrawElements(viewTab, inputs, kbListener);
            selectionSystem.DrawPages (inputs, kbListener);
            
    
            bool listChanged = false;
            bool remove = (viewTab == 0) && (kbListener[KeyCode.Delete] || kbListener[KeyCode.Backspace]);
            bool add = (viewTab == 1) && kbListener[KeyCode.Return];

            
                
            if (removeOrAdd || add || remove) {
                if ((removeOrAdd && viewTab == 0) || remove) listChanged = DeleteSelectionFromList(curEventState);
                if ((removeOrAdd && viewTab == 1) || add) listChanged = AddSelectionToSet();
            }
            
            if (generateNewIDs) {
                PackEditor.GetErrorsAndWarnings (packIndex, out errorStrings, out warningStrings, out noIDsCount);
                InitializeAllFilePaths();
            }
            
            selectionSystem.HandleInputs(inputs, 
                generateNewIDs || listChanged || addedState || deletedState || changedAOState
            );
            
            removeOrAdd = false;

        }   

        IEnumerable<EditorProp> GetAOPropsSelectOrAll (EditorProp curEventState) {
            int currentEventSubstateCount = curEventState[EventState.subStatesField].arraySize;
            return new HashSet<EditorProp>().Generate(selectionSystem.GetPoolIndiciesInSelectionOrAllShown(), i => curEventState[EventState.assetObjectsField] [i - currentEventSubstateCount]);
        }

        bool AddSelectionToSet () {
            bool reset_i = true;            
            HashSet<int> idsInSelection = selectionSystem.GetIDsInSelection(false, out _);
            if (idsInSelection.Count == 0) return false;
            
            EditorProp aoArray = baseEventState[EventState.assetObjectsField];
            foreach (var id in idsInSelection) {
                EditorProp newAO = aoArray.AddNew();
                Object objRef = GetObjectRefForID(id);
                AssetObjectEditor.InitializeNewAssetObject(newAO, id, objRef, reset_i, packIndex);    
                reset_i = false;
            }
            return true;
        }

        
        void MoveAOsToEventState(IEnumerable<int> poolIndicies, string origDirectory, string targetDir)
        {
            
            if (poolIndicies.Count() == 0) return;
            Debug.Log("moving selection from " + origDirectory + " to " + targetDir);

            EditorProp origParentState = GetEventStateByPath(origDirectory);
            EditorProp targetState = GetEventStateByPath(targetDir);
            
            EditorProp parentAOs = origParentState[EventState.assetObjectsField];
            EditorProp targetAOs = targetState[EventState.assetObjectsField];

            int parentSubstateCount = origParentState[EventState.subStatesField].arraySize;
            int parentTotalSize = parentAOs.arraySize + parentSubstateCount;

            for (int i = parentTotalSize - 1; i >= 0; i--) {

                if (poolIndicies.Contains(i)) {

                    if (i >= parentSubstateCount) {
                        int aoIndex = i-parentSubstateCount;

                        //Debug.Log(" moving " + aoIndex);
                        EditorProp ao = parentAOs[aoIndex];

                        //Debug.Log("copying to new state");
                        EditorProp newAO = targetAOs.AddNew();
                        AssetObjectEditor.CopyAssetObject(newAO, ao);
                        
                        //Debug.Log("deleting original");
                        parentAOs.DeleteAt(aoIndex);
        
                    }
                    else {
                        int stateIndex = i;
                        Debug.Log("moving an event state to sustate");
                        //curState[EventState.subStatesField].DeleteAt(i);
                    }
                }
            }
        }


        bool DeleteSelectionFromList (EditorProp eventState) {        
            Debug.Log("deleting selection from list");    
            HashSet<int> deleteIndicies = selectionSystem.GetPoolIndiciesInSelection();
            Debug.Log( deleteIndicies.Count);
            
            if (deleteIndicies.Count == 0) return false;

            int currentEventSubstateCount = eventState[EventState.subStatesField].arraySize;
            int currentEventTotalSize = eventState[EventState.assetObjectsField].arraySize + currentEventSubstateCount;

            Debug.Log(currentEventSubstateCount + " / " + deleteIndicies.Count);

            int deleteOption = -1;
            for (int i = currentEventTotalSize - 1; i >= 0; i--) {
                if (deleteIndicies.Contains(i)) {

                    if (i >= currentEventSubstateCount) {
                                                Debug.Log("Deleting ao");

                        eventState[EventState.assetObjectsField].DeleteAt(i - currentEventSubstateCount);
                    }
                    else {

                        Debug.Log("Deleting events state");



                        bool deletedState = DeleteState(baseEventState, eventState, eventState[EventState.subStatesField][i], ref deleteOption);

                        //eventState[EventState.subStatesField].DeleteAt(i);

                    }

                }
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