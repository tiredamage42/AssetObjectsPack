using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
namespace AssetObjectsPacks {
    [CustomEditor(typeof(Event))]
    public class EventEditor : Editor {
        PopupList.InputData packsPopup;    
        //EditorProp multiEditAO, AOList, hiddenIDsProp;
        EditorProp multiEditAO, hiddenIDsProp;

        //const string conditionsBlockField = "conditionsBlock";


        static void AddAOsAndSubAOsToBaseList(EditorProp baseEventState, EditorProp eventState) {
            for (int i = 0; i < eventState[EventState.assetObjectsField].arraySize; i++) {
                AssetObjectEditor.CopyAssetObject(baseEventState[EventState.assetObjectsField].AddNew(), eventState[EventState.assetObjectsField][i]);
            }
            for (int i = 0; i < eventState[EventState.subStatesField].arraySize; i++) {
                AddAOsAndSubAOsToBaseList(baseEventState, eventState[EventState.subStatesField][i]);
            }

        }


        
        static bool DeleteState (EditorProp baseEventState, EditorProp parentState, EditorProp eventState) {
            int selection = EditorUtility.DisplayDialogComplex(
                "Delete State", 
                "Delete state and asset objects? If keeping asset objects they will be moved to base state", 
                "Delete And Keep", "Cancel", "Delete All"
            );
            switch(selection) {
                case 1: return false;
                case 0: 
                    Debug.LogError("Deleted Keep");
                    AddAOsAndSubAOsToBaseList(baseEventState, eventState);
                    break;
                case 2:
                    Debug.LogError("Deleted All");
                    break;
            }
            int atIndex = -1;
            for (int i = 0; i < parentState[EventState.subStatesField].arraySize; i++) {
                string name = parentState[EventState.subStatesField][i][EventState.nameField].stringValue;
                if (name == eventState[EventState.nameField].stringValue) {
                    atIndex = i;
                    break;
                }
            }
            parentState[EventState.subStatesField].DeleteAt(atIndex);
            //selectionSystem.ForceBackFolder();
            return true;
        }

        static void DrawEventState(EditorProp baseEventState, EditorProp parentState, EditorProp eventState, out bool addedState, out bool deletedState, out bool changedAOState) {
            bool drawingBase = parentState == null;// eventState == baseEventState;
            
            addedState = false;
            changedAOState = false;

            EditorGUILayout.BeginHorizontal();
            GUIUtils.Label(new GUIContent("<b>Current State:</b>"), false);

            GUILayout.FlexibleSpace();



            if (GUIUtils.Button(new GUIContent("Add Substate"), true, GUIStyles.miniButton)) {
                MakeNewEventStateDefault(eventState[EventState.subStatesField].AddNew("New Event State"));
                addedState = true;
            }

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
                deletedState = DeleteState(baseEventState, parentState, eventState);
                
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



            baseEventState = so[Event.baseStateField];

            //AOList = so[Event.asset_objs_field];
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


        /*
        EditorProp curParentEventState {
            get {
                if (eventStatesSelected.Count <= 1) {
                    return baseEventState;
                }
                return eventStatesSelected[eventStatesSelected.Count - 2];
            }
        }
        */
        EditorProp baseEventState;
/*
        EditorProp curEventState {
            get {
                if (eventStatesSelected.Count == 0) {
                    return baseEventState;
                }
                return eventStatesSelected.Last();
            }
        }
        List<EditorProp> eventStatesSelected = new List<EditorProp>();

 */

        static EditorProp GetEventStateByName ( EditorProp subStates, string name ) {
            for (int i = 0; i < subStates.arraySize; i++) {

                string subStateName = subStates[i][EventState.nameField].stringValue;

                if (subStateName == name) {
                    return subStates[i];
                }
            }
            Debug.LogError("couldnt find event state: " + name);
            return null;
        }
        

        //void OnMoveFolder (int viewTab, string addName, string newPath, bool isReset, bool backFirst ) {
        void OnMoveFolder (int viewTab, string toPath, string newPath, bool isReset ) {

            bool movingBack = toPath == null;//addName == null;
            if (viewTab == 0) {

                if (isReset) {
                    //eventStatesSelected.Clear();
                    return;
                }

                if (movingBack) {
                    //eventStatesSelected.Remove(eventStatesSelected.Last());
                }
                else {
                    //if (backFirst){

                    //    eventStatesSelected.Remove(eventStatesSelected.Last());
                    //}

                    //eventStatesSelected = GetEventStatesByPath()

                    //eventStatesSelected.Add( GetEventStateByPath(newPath) );// GetEventStateByName(curEventState[EventState.subStatesField], addName) );
                }
            }
        }



        ElementSelectionSystem.Element GetPoolElementAtIndex(int i, int viewTab, string atPath) {
            int id;

            //Debug.Log("0");
            string path = "";
            if (viewTab == 0) {
                path = atPath;


                Debug.Log("Getting pool element at path: " + path);

                //for (int x = 0; x < eventStatesSelected.Count; x++) {
                //    path += eventStatesSelected[i][EventState.nameField].stringValue;
                    //if (x != eventStatesSelected.Count -1) {
                //        path += "/";
                    //}
                //}


                //Debug.Log("starting with path: " + path);


                EditorProp state = GetEventStateByPath(path); //curEventState;


                int subStateCount = state[EventState.subStatesField].arraySize;
                if (i < subStateCount) {
                    id = -1;

                    path += state[EventState.subStatesField][i][EventState.nameField].stringValue;

                    path += "/trickfolderedview";

                    Debug.Log("making folder: " + path);



                }
                else {


                    id = AssetObjectEditor.GetID(
                        state[EventState.assetObjectsField][i - subStateCount]
                    );
                    path = path + id2path[id].Replace("/", "*");

                    Debug.Log("making file " + path);
                }



                //id = AssetObjectEditor.GetID(
                //    AOList[i]
                //);


            }
            else {
                path = allPaths[i];
                
                id = AssetObjectsEditor.GetObjectIDFromPath(path); 
            }
            return new ElementSelectionSystem.Element(id, AssetObjectsEditor.RemoveIDFromPath(path), i);
        }

        void ReinitializeEventStates(EditorProp baseEventState) {

            Debug.Log("initializing event states aos");
            for (int i = 0; i < baseEventState[EventState.assetObjectsField].arraySize; i++) {
                AssetObjectEditor.MakeAssetObjectDefault(baseEventState[EventState.assetObjectsField][i], packIndex, false);
            }
            Debug.Log("initializing event states subStates");
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

                //for (int i = 0; i < AOList.arraySize; i++) AssetObjectEditor.MakeAssetObjectDefault(AOList[i], packIndex, false);
                
                AssetObjectEditor.MakeAssetObjectDefault(multiEditAO, packIndex, true);
                paramlabels = PackEditor.GUI.GetDefaultParamNameGUIs(packIndex);
                

                ElementSelectionSystem.ViewTabOption[] viewTabOptions = new ElementSelectionSystem.ViewTabOption[] {
                    new ElementSelectionSystem.ViewTabOption( new GUIContent("Event Pack"), false ),
                    new ElementSelectionSystem.ViewTabOption( new GUIContent("Project"), true )
                };
                 
                selectionSystem.Initialize(
                    viewTabOptions, hiddenIDsProp, 
                    GetPoolElementAtIndex, GetPoolCount, GetIgnoreIDs, 
                    RebuildPreviewEditor, 
                    OnMoveFolder,
                    OnDirDragDrop
                );
                
                initializeAfterPackChange = true;
            }

            PacksManager pm = AssetObjectsEditor.packManager;
            packsPopup = new PopupList.InputData { m_OnSelectCallback = OnSwitchPackCallback };
            int l = pm.packs.Length;
            for (int i = 0; i < l; i++) packsPopup.NewOrMatchingElement(pm.packs[i].name, index == i);        
        }

        void OnDirDragDrop(HashSet<int> dragIndicies, string curPath, ElementSelectionSystem.Element dirElement, int viewTab) {
            if (viewTab == 0) {
                MoveAOsToEventState(dragIndicies, curPath, dirElement.path);// EditorProp targetState)
            }
            else if (viewTab == 1) {
                Debug.Log("project view drag to dir " + dirElement.path);
            }
            

            
            //foreach (var de in draggedElements) {
            //    Debug.Log(de.id + " : " + de.path);
            //}
            //Debug.Log("Dir:");
            //Debug.Log(dirElement.id + " : " + dirElement.path);
        
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
                    
                    //AOList.Clear();



                    Reinitialize(i);             
                    break;
                }
            }
        }

        void GetAllEventIDs (EditorProp eventState, ref HashSet<int> ret) {
            for (int i = 0; i < eventState[EventState.assetObjectsField].arraySize; i++) {
                ret.Add( AssetObjectEditor.GetID(eventState[EventState.assetObjectsField][i]));

            }
for (int i = 0; i < eventState[EventState.subStatesField].arraySize; i++) {

    GetAllEventIDs(eventState[EventState.subStatesField][i], ref ret);

            }



        }
        
        HashSet<int> GetIgnoreIDs (int viewTab) {
            HashSet<int> ret = new HashSet<int>();
            if (viewTab == 1) {

                GetAllEventIDs(baseEventState, ref ret);

            }
            return ret;
            /*
            return viewTab != 1 ? new HashSet<int>() : new HashSet<int>().Generate(
                



                AOList.arraySize, 
                
                i => AssetObjectEditor.GetID(
                    AOList[i]
                ) 
            ); 
            */
        }

        int GetPoolCount (int viewTab, string dirPath) {

            if (viewTab == 0) {

                
                EditorProp eventState = GetEventStateByPath(dirPath);
                int currentEventSubstateCount = eventState[EventState.subStatesField].arraySize;

                int c = eventState[EventState.assetObjectsField].arraySize + currentEventSubstateCount;
                Debug.Log("Getting pool count (" + c + ") at path: " + dirPath);
                return c;

            }
        //    return viewTab == 0 ? AOList.arraySize : allPaths.Length;
        //    return viewTab == 0 ? currentEventTotalSize : allPaths.Length;
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

        void CustomToolbar (int viewTab, out bool addedState, out bool deletedState, out bool changedAOState){
            addedState = deletedState = changedAOState = false;
            if (!selectionSystem.showingHidden) {
            GUIUtils.StartBox(0);
            
            EditorGUILayout.BeginHorizontal();
            GUI.enabled = selectionSystem.justFilesSelected;

            if (ImportSettingsButton(GUIStyles.miniButtonLeft)) OpenImportSettings();

            removeOrAdd = AddRemoveButton(GUIStyles.miniButtonRight);
            
            GUI.enabled = true;
            
            EditorGUILayout.EndHorizontal();



            if (viewTab == 0) DrawMultiEditGUI(out addedState, out deletedState, out changedAOState);

            GUIUtils.EndBox(1);
            }
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
                    if (selectedElement.id != -1) {
                        EditorProp curEventState;
            GetCurrentAndParentStates(out _, out curEventState);
            int currentEventSubstateCount = curEventState[EventState.subStatesField].arraySize;
            //int currentEventTotalSize = curEventState[EventState.assetObjectsField].arraySize + currentEventSubstateCount;
            

                        p = curEventState[EventState.assetObjectsField][selectedElement.poolIndex - currentEventSubstateCount];
                        // AOList[selectedElement.poolIndex];
                    }
                }
            }
            return p;
        }


        string curFolderPath {
            get {
                return selectionSystem.currentFolderPath;
            }
        }


        void GetCurrentAndParentStates (out EditorProp parentState, out EditorProp curState) {
            string path = curFolderPath;
            parentState = null;

            if (path == "") {
                //Debug.Log("base event");
                curState = baseEventState;
                //return baseEventState;
                return;
            }

            //Debug.Log("getting event state at path: " + path);
            string[] split = path.Split('/');


            parentState = null;
            curState = baseEventState;

            for (int i = 0; i < split.Length; i++) {
                //Debug.Log("checking (l)" + split[i]);
                parentState = curState;
                curState = GetEventStateByName(parentState[EventState.subStatesField], split[i]);
            }
            //return curState;


        }


        
        EditorProp GetEventStateByPath(string path) {

            if (path == "") {
                Debug.Log("base event");
                return baseEventState;
            }
            Debug.Log("getting event state at path: " + path);
            string[] split = path.Split('/');

            EditorProp lastState = baseEventState;
            for (int i = 0; i < split.Length; i++) {
                Debug.Log("checking (l)" + split[i]);
                lastState = GetEventStateByName(lastState[EventState.subStatesField], split[i]);
            }
            return lastState;



        }




        
        void DrawMultiEditGUI (out bool addedState, out bool deletedState, out bool changedAOState){

            EditorProp curParentEventState, curEventState;
            GetCurrentAndParentStates(out curParentEventState, out curEventState);
            DrawEventState(
                baseEventState,
                curParentEventState, curEventState, out addedState, out deletedState, out changedAOState);
if (deletedState) {
                    selectionSystem.ForceBackFolder();

                }

            bool hasSelection = selectionSystem.hasSelection;
            bool singleSelection = selectionSystem.singleSelection;
            ElementSelectionSystem.Element selectedElement = selectionSystem.selectedElement;

            GUIUtils.Space(1);


            int setProp = -1;
            GUI.enabled = !(hasSelection && singleSelection && selectedElement.id == -1);

            
            
                GUIUtils.Label(GetEditToolbarTitle (hasSelection, singleSelection, selectedElement), false);
                
                //bool multiConditionAdd, multiConditionReplace;            
                bool setCondition;            
                
                AssetObjectEditor.GUI.DrawAssetObjectMultiEditView(

                    hasSelection && singleSelection,

                    GetAOPropForEditToolbar (hasSelection, singleSelection, selectedElement), 
                    
                    
                    paramWidths, 

                    out setCondition,
                    //out multiConditionAdd, 
                    //out multiConditionReplace, 
                    out setProp, 
                    paramlabels
                );

                GUI.enabled = true;
            

            //if (multiConditionAdd || multiConditionReplace) AssetObjectEditor.CopyConditions(GetAOPropsSelectOrAll(), false);//, multiConditionAdd);
            //if (setCondition) AssetObjectEditor.CopyConditions(GetAOPropsSelectOrAll(), multiEditAO, false);//, multiConditionAdd);

            if (setProp != -1) AssetObjectEditor.CopyParameters(GetAOPropsSelectOrAll(), multiEditAO, setProp);
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
            
            CustomToolbar(viewTab, out addedState, out deletedState, out changedAOState);
        
            selectionSystem.DrawElements(viewTab, inputs, kbListener);
            selectionSystem.DrawPages (inputs, kbListener);
            
    
            bool listChanged = false;
            bool remove = (viewTab == 0) && (kbListener[KeyCode.Delete] || kbListener[KeyCode.Backspace]);
            bool add = (viewTab == 1) && kbListener[KeyCode.Return];
                
            if (removeOrAdd || add || remove) {
                if ((removeOrAdd && viewTab == 0) || remove) listChanged = DeleteSelectionFromList();
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

        IEnumerable<EditorProp> GetAOPropsSelectOrAll () {

            EditorProp curEventState;
            GetCurrentAndParentStates(out _, out curEventState);
            int currentEventSubstateCount = curEventState[EventState.subStatesField].arraySize;
            //int currentEventTotalSize = curEventState[EventState.assetObjectsField].arraySize + currentEventSubstateCount;
            
            return new HashSet<EditorProp>().Generate(
                selectionSystem.GetPoolIndiciesInSelectionOrAllShown(), 
                
                
                i => curEventState[EventState.assetObjectsField] [i - currentEventSubstateCount] 
                //i => AOList[i] 


            );
        }

        
        bool AddSelectionToSet () {
            Debug.Log("adding selection to list");    
            
            bool reset_i = true;            
            HashSet<int> idsInSelection = selectionSystem.GetIDsInSelection(false, out _);
            if (idsInSelection.Count == 0) return false;
            foreach (var id in idsInSelection) {
                AssetObjectEditor.InitializeNewAssetObject(
                    //AOList.AddNew(), 
                    baseEventState[EventState.assetObjectsField].AddNew(),
                    id, GetObjectRefForID(id), reset_i, packIndex
                );    
                reset_i = false;
            }
            return true;
        }





/*

        int currentEventTotalSize {
            get {
                return curEventState[EventState.assetObjectsField].arraySize + currentEventSubstateCount;
            }
        }
        int currentEventSubstateCount {
            get {
                return curEventState[EventState.subStatesField].arraySize;
                
            }
        }

*/


        
        bool MoveAOsToEventState(HashSet<int> poolIndicies, string curPath, string targetPath)// EditorProp targetState)
        {
            Debug.Log("moving selection to new state");    

            EditorProp curState = GetEventStateByPath(curPath);
            EditorProp targetState = GetEventStateByPath(targetPath);


                

            int currentEventSubstateCount = curState[EventState.subStatesField].arraySize;
            int currentEventTotalSize = curState[EventState.assetObjectsField].arraySize + currentEventSubstateCount;
            
            
            
        
            //HashSet<int> moveIndicies = selectionSystem.GetPoolIndiciesInSelection();
            if (poolIndicies.Count == 0) return false;


            for (int i = currentEventTotalSize - 1; i >= 0; i--) {

                if (poolIndicies.Contains(i)) {

                    if (i >= currentEventSubstateCount) {
                        int aoIndex = i-currentEventSubstateCount;

                        EditorProp ao = curState[EventState.assetObjectsField][aoIndex];
                        EditorProp newAO = targetState[EventState.assetObjectsField].AddNew();

                        Debug.Log("copying ao: " + AssetObjectEditor.GetName(ao));


                        AssetObjectEditor.CopyAssetObject(
                            targetState[EventState.assetObjectsField].AddNew(), ao
                        );




                        curState[EventState.assetObjectsField].DeleteAt(aoIndex);
                    
                    
                    }
                    //else {
                    //    curState[EventState.subStatesField].DeleteAt(i);
                    //}

                    //AOList.DeleteAt(i);
                }
            }
            return true;
            
        }
        bool DeleteSelectionFromList () {        
            Debug.Log("deleting selection from list");    
            HashSet<int> deleteIndicies = selectionSystem.GetPoolIndiciesInSelection();
            if (deleteIndicies.Count == 0) return false;


            EditorProp curEventState;
            GetCurrentAndParentStates(out _, out curEventState);
            int currentEventSubstateCount = curEventState[EventState.subStatesField].arraySize;
            int currentEventTotalSize = curEventState[EventState.assetObjectsField].arraySize + currentEventSubstateCount;
            



            for (int i = currentEventTotalSize - 1; i >= 0; i--) {
                if (deleteIndicies.Contains(i)) {

                    if (i >= currentEventSubstateCount) {
                        curEventState[EventState.assetObjectsField].DeleteAt(i - currentEventSubstateCount);
                    }
                    else {
                        curEventState[EventState.subStatesField].DeleteAt(i);
                    }

                    //AOList.DeleteAt(i);
                }
            }
            

            //for (int i = AOList.arraySize - 1; i >= 0; i--) {
            //    if (deleteIndicies.Contains(i)) AOList.DeleteAt(i);
            //}
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