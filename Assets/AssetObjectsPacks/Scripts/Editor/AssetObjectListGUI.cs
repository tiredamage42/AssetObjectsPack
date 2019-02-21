using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
namespace AssetObjectsPacks {
    public class ListViewElement {
        public GUIContent gui;
        public int id, index;
        public ListViewElement(GUIContent gui, int id, int index) => (this.gui, this.id, this.index) = (gui, id, index);
    }
    
    public class AssetObjectListGUI {
        public bool HasPreviewGUI() { 
            return true; 
        }
        public void OnInteractivePreviewGUI(Rect r, GUIStyle background) { 
            if (preview != null) preview.OnInteractivePreviewGUI(r, background); 
        }
        public void OnPreviewSettings() { 
            if (preview != null) preview.OnPreviewSettings();
        }

        const int elementsPerPage = 25;
        
        bool isListView { get { return ToolbarGUI.listProjectView == 0; } }

        
        bool hasSelection { get { return hi_selection != -1 && lo_selection != -1; } }// selection.Count != 0; } }        
        
        //HashSet<ListViewElement> selection = new HashSet<ListViewElement>();
        //HashSet<int> selection = new HashSet<int>();
        
        HashSet<int> hiddenIds = new HashSet<int>();
        ListViewElement[] elements;
        Editor preview;
        Dictionary<int, string> id2path;
        EditorProp multiEditAO, AOList, hiddenIDsProp;
        GUIContent[] paramlabels;
        GUILayoutOption[] paramWidths;
        GUIContent curPageGUI;
        string[] allPaths, errorStrings, warningStrings;
        string objectsDirectory, fileExtensions, assetType, currentDirectory = "";
        bool initializeAfterPackChange;
        int page, max_pages, foldersHiearchyOffset, lo_selection = -1, hi_selection = -1, noIDsCount, packIndex;
        
        bool ToggleHiddenSelected () {

            bool toggledAny = false;

            for (int i = lo_selection; i <= hi_selection; i++) {

            //foreach (var i in selection) {
                ListViewElement e = elements[i];
                //dont toggle directories
                if (e.id == -1) continue;
                if (hiddenIds.Contains(e.id)) hiddenIds.Remove(e.id);
                else hiddenIds.Add(e.id);
                toggledAny = true;
            }
            if (toggledAny) {
                //save to serialized object
                hiddenIDsProp.Clear();
                foreach (int id in hiddenIds) hiddenIDsProp.AddNew().SetValue(id);
            }
            return toggledAny && !ToolbarGUI.showHidden;

        }
/*
        void RecalculateLowestAndHighestSelections () {
            lo_selection = int.MaxValue;
            hi_selection = int.MinValue;
            int s = selection.Count;
            if (s == 0) return;
            int c = elements.Length;
            for (int i = 0; i < c; i++) {
                if (selection.Contains(i)) {                    
                    if (i < lo_selection) lo_selection = i;
                    if (i > hi_selection) hi_selection = i;
                    if (s == 1) break;
                }
            }
        }
*/

        /*
  
        void OnObjectSelection (ListViewElement selected, bool was_selected) {
            Event e = Event.current;
            if (e.shift) {
                selection.Add(selected);
                RecalculateLowestAndHighestSelections();
                selection.Clear();
                for (int i = lo_selection; i <= hi_selection; i++) selection.Add(elements[i]);
            }
            else if (e.command || e.control) {
                if (was_selected) selection.Remove(selected);
                else selection.Add(selected);
            }
            else {
                selection.Clear();
                if (!was_selected) selection.Add(selected);
            }
            OnSelectionChange();
        }
        */

        /*
        void OnObjectSelection (int selectedIndex, bool was_selected) {
            Event e = Event.current;
            if (e.shift) {
                selection.Add(selectedIndex);
                RecalculateLowestAndHighestSelections();
                selection.Clear();
                for (int i = lo_selection; i <= hi_selection; i++) selection.Add(i);
            }
            else if (e.command || e.control) {
                if (was_selected) selection.Remove(selectedIndex);
                else selection.Add(selectedIndex);
            }
            else {
                selection.Clear();
                if (!was_selected) selection.Add(selectedIndex);
            }
            OnSelectionChange();
        }

         */

        //bool hasSelection;
        void OnObjectSelection (int selectedIndex){//, bool was_selected) {

            Event e = Event.current;
            if (e.shift) {
                if (hasSelection) {
                    if (selectedIndex < lo_selection) {
                        lo_selection = selectedIndex;
                    }
                    if (selectedIndex > hi_selection) {
                        hi_selection = selectedIndex;
                    }
                }
                else {
                    hi_selection = lo_selection = selectedIndex;
                }

                //selection.Add(selectedIndex);
                //RecalculateLowestAndHighestSelections();
                //selection.Clear();
                //for (int i = lo_selection; i <= hi_selection; i++) selection.Add(i);
            }
            else {
                if (hi_selection == lo_selection && selectedIndex == lo_selection) {

                    hi_selection = lo_selection = -1;
                }
                else {
                    
                    hi_selection = lo_selection = selectedIndex;
                }

            }
            /*
            else if (e.command || e.control) {
                if (was_selected) selection.Remove(selectedIndex);
                else selection.Add(selectedIndex);
            }
            else {
                selection.Clear();
                if (!was_selected) selection.Add(selectedIndex);
            }
            */
            OnSelectionChange();
        }



        void OnSelectionChange() {
            //RecalculateLowestAndHighestSelections();
            RebuildPreviewEditor();        
        }

        void ClearSelectionsAndRebuild(bool resetPage) {
            if (resetPage) page = 0;

            hi_selection = lo_selection = -1;
            //selection.Clear();
            OnSelectionChange();
            RebuildAllElements();
        }

        bool SelectionHasDirectories() {
            if (!hasSelection) return false;
            for (int i = lo_selection; i <= hi_selection; i++) {
            //foreach (var s in selection) {
                if (elements[i].id == -1) return true;
            }
            return false;
        }

        UnityEngine.Object GetObjectRefForElement(int e) {
            return EditorUtils.GetAssetAtPath(objectsDirectory + id2path[elements[e].id], assetType);  
        }
    
        public void RealOnEnable (SerializedObject eventPack) {
        
            AOList = new EditorProp ( eventPack.FindProperty(AssetObjectEventPack.asset_objs_field) );
            hiddenIDsProp = new EditorProp ( eventPack.FindProperty(AssetObjectEventPack.hiddenIDsField) );
            multiEditAO = new EditorProp ( eventPack.FindProperty(AssetObjectEventPack.multi_edit_instance_field) );
             
            hiddenIds = hiddenIds.Generate(hiddenIDsProp.arraySize, i => { return hiddenIDsProp[i].intValue; } );
        }

        public void InitializeWithPack (int packIndex) {
            this.packIndex = packIndex;
            PacksManagerEditor.GetValues(packIndex, out _, out objectsDirectory, out fileExtensions, out assetType);
            PacksManagerEditor.GetErrorsAndWarnings (packIndex, out errorStrings, out warningStrings, out noIDsCount);
            
            paramlabels = new GUIContent[0];
            paramWidths = new GUILayoutOption[0];
                
            if (errorStrings.Length == 0) {

                InitializeAllFilePaths();
                
                int c = AOList.arraySize;
                for (int i = 0; i < c; i++) PacksManagerEditor.AdjustParametersToPack(AOList[i][AssetObject.params_field], packIndex, false);
                
                PacksManagerEditor.AdjustParametersToPack(multiEditAO[AssetObject.params_field], packIndex, true);
                multiEditAO[AssetObject.conditionChecksField].Clear();
                    
                paramlabels = PacksManagerEditor.GUI.GetDefaultParamGUIs(packIndex);
            }
            initializeAfterPackChange = true;
        }


        void InitializeAfterPackChange () {
            if (!initializeAfterPackChange) return;
            initializeAfterPackChange = false;
            int props_count = paramlabels.Length;
            paramWidths = new GUILayoutOption[props_count];
            for (int i = 0; i < props_count; i++) paramWidths[i] = paramlabels[i].CalcWidth(GUIStyles.label);
            
            ClearSelectionsAndRebuild(true);
            //eventPack.ApplyModifiedProperties();
        }

        void RebuildAllElements() {
            HashSet<string> used = new HashSet<string>();            
            int lastDir = 0;
            HashSet<int> idsInSet = isListView ? null : new HashSet<int>().Generate(AOList.arraySize, i => { return AOList[i][AssetObject.id_field].intValue; }); 

            List<ListViewElement> unpaginated = new List<ListViewElement>();

            int l = isListView ? AOList.arraySize : allPaths.Length;
            for (int i = 0; i < l; i++) {
                
                int id;
                string path;
                EditorProp ao = isListView ? AOList[i] : null;
                if (isListView) {
                    id = ao[AssetObject.id_field].intValue;
                    path = id2path[id];
                }
                else {
                    path = allPaths[i];
                    id = AssetObjectsEditor.GetObjectIDFromPath(path);
                }

                if (!isListView && idsInSet.Contains(id)) continue;
                if (!ToolbarGUI.showHidden && hiddenIds.Contains(id)) continue;
                if (!ToolbarGUI.PathPassesSearchFilter(path)) continue;

                if (ToolbarGUI.folderedView) {

                    if (!currentDirectory.IsEmpty() && !path.StartsWith(currentDirectory)) continue;

                    string name = path;
                    if (path.Contains("/")) name = path.Split('/')[foldersHiearchyOffset];    

                    if (name.Contains(".")) { 
                        unpaginated.Add(new ListViewElement(new GUIContent( AssetObjectsEditor.RemoveIDFromPath(name) ), id, i));
                        continue;
                    }
                        
                    if (used.Contains(name)) continue;
                    used.Add(name);
                    unpaginated.Insert(lastDir, new ListViewElement(new GUIContent( name ), -1, i));
                    lastDir++;       
                    continue;
                }
                unpaginated.Add(new ListViewElement(new GUIContent(AssetObjectsEditor.RemoveIDFromPath(path)), id, i));
            }

            l = unpaginated.Count;

            max_pages = (l / elementsPerPage) + Mathf.Min(1, l % elementsPerPage);

            if (page >= max_pages) page = Mathf.Max(0, max_pages - 1);
            
            int min = page * elementsPerPage;
            int max = Mathf.Min(min + elementsPerPage, l - 1);

            curPageGUI = new GUIContent("Page: " + (page + 1) + " / " + max_pages);
        
            elements = unpaginated.Slice(min, max).ToArray();

        }
        int DrawElements (bool forListView){
            if (elements.Length == 0) {
                EditorGUILayout.HelpBox(new string[] {"Add elements through the project view tab", "No elements in the pack directory" }[ToolbarGUI.listProjectView], MessageType.Info);
                return -1;
            }
            GUIUtils.StartBox(0);

            int selectedIndex = -1;

            int l = elements.Length;
            for (int i = 0; i < l; i++) {
                ListViewElement e = elements[i];

                
                bool s = i >= lo_selection && i <= hi_selection;// selection.Contains(i);
                if (e.id == -1) {
                    if (AssetObjectEditor.GUI.AssetObjectDirectoryElement (e.gui, s)) selectedIndex = i;
                }
                else {
                    if (forListView) {
                        if (AssetObjectEditor.GUI.DrawAssetObjectEventView(AOList[e.index], e.gui, s, hiddenIds.Contains(e.id), paramWidths))selectedIndex = i;
                    }
                    else {
                        if (AssetObjectEditor.GUI.DrawAssetObjectProjectView(e.gui, s, hiddenIds.Contains(e.id)))selectedIndex = i;
                    }
                }       
            }
            GUIUtils.EndBox(1);
            return selectedIndex;
        }

        void AddNewAssetObject (int obj_id, UnityEngine.Object obj_ref, bool make_default) {
            EditorProp ao = AOList.AddNew();
            ao[AssetObject.id_field].SetValue ( obj_id );
            ao[AssetObject.obj_ref_field].SetValue ( obj_ref );
            
            //only need to default first one added, the rest will copy the last one 'inserted' into the
            //serialized property array
            if (!make_default) return;
            ao[AssetObject.conditionChecksField].Clear();

            PacksManagerEditor.AdjustParametersToPack(ao[AssetObject.params_field], packIndex, true);                
        }
        
        void InitializeAllFilePaths () {
            allPaths = AssetObjectsEditor.GetAllAssetObjectPaths (objectsDirectory, fileExtensions, false, out id2path);
        }
        
        public bool Draw (){
            InitializeAfterPackChange();

            bool lv = isListView;

            bool generateNewIDs;
            PacksManagerEditor.GUI.DrawErrorsAndWarnings(errorStrings, warningStrings, noIDsCount, out generateNewIDs);
           
            if (errorStrings.Length != 0) return false;
            
            bool folderBack, changedTabView, changedFolderView;
            bool importSettings, toggleHidden,showHiddenToggled, resetHidden, removeOrAdd, searchChanged;
            
            bool validSelection = hasSelection && !SelectionHasDirectories();
            ToolbarGUI.DrawToolbar (validSelection, currentDirectory, foldersHiearchyOffset, out importSettings, out removeOrAdd, out searchChanged, out resetHidden, out toggleHidden, out showHiddenToggled, out changedTabView, out folderBack, out changedFolderView);
        
            bool down, up, left, right, del, ret, h, shift;
            KeyboardInput (out down, out up, out left, out right, out h, out del, out ret, out shift);

            bool multiConditionAdd = false, multiConditionReplace = false, showParamsChanged = false, showConditionsChanged = false;
            int setProp = !lv ? -1 : MultiEditGUI.DrawMultiEditGUI(multiEditAO, paramlabels, paramWidths, out showParamsChanged, out showConditionsChanged, out multiConditionAdd, out multiConditionReplace);
            int selectedElementIndex = DrawElements(lv);
            
            bool prevPage, nextPage;
            PagesGUI.DrawPages (GUIStyles.toolbarButton, out prevPage, out nextPage, curPageGUI);

            HandleSelectionAfterKeyboardInput (down, up, shift);

            if (hasSelection && ret) {
                if (SelectionHasDirectories()) {
                    int c = selectionCount;
                    if (c == 1) selectedElementIndex = 0;// = selection.First();
                    ret = false;
                }
            } 
                
            bool listChanged = false;
            string folderFwdDir = null;

            if (selectedElementIndex != -1) {
                ListViewElement e = elements[selectedElementIndex];
                if (e.id == -1) {
                    folderFwdDir = e.gui.text;
                }
                else {
                    OnObjectSelection(selectedElementIndex//, 

                        //selectedElementIndex >= lo_selection && selectedElementIndex <= hi_selection
                        //selection.Contains(selectedElementIndex)
                    );
                }
            }
            
            
            if (multiConditionAdd || multiConditionReplace) EditMultiConditions(multiConditionAdd);
            if (setProp != -1) SetParameterAll(setProp);
            if (showParamsChanged || showConditionsChanged) ChangeShowParamsOrConditionsMulti(showParamsChanged);
            
            

            //if (setProp != -1) listChanged = true;
            
            if (importSettings) OpenImportSettings();
            

            if (hasSelection) {
                bool remove = lv && del;
                bool add = !lv && ret;
                if (removeOrAdd || add || remove) {
                    if ((removeOrAdd && lv) || remove) DeleteSelectionFromList( );
                    if ((removeOrAdd && !lv) || add) AddSelectionToSet( );
                    listChanged = true;
                }
            }


            bool toggledAnyHidden = (hasSelection && (toggleHidden || h )) && ToggleHiddenSelected();
            
            if (resetHidden) {
                hiddenIDsProp.Clear();
                hiddenIds.Clear();
            }
        
            bool switchedPage = false;     


            if (nextPage || right) {
                if (SwitchPage(1)) switchedPage = true;
            }
            if (prevPage || left) {
                bool wentPrevious = SwitchPage(-1);
                if (wentPrevious) switchedPage = true;
                if (left && !wentPrevious && ToolbarGUI.folderedView) folderBack = true;
            }

            
            bool movedFolder = (folderBack && MoveFolder()) || (folderFwdDir != null && MoveFolder(folderFwdDir));

                
            
            
            if (generateNewIDs) {
                PacksManagerEditor.GenerateIDsForPack(packIndex);
                PacksManagerEditor.GetErrorsAndWarnings (packIndex, out errorStrings, out warningStrings, out noIDsCount);
                InitializeAllFilePaths();
            }
            
            if (movedFolder || changedTabView || changedFolderView || generateNewIDs || toggledAnyHidden || showHiddenToggled || searchChanged || resetHidden || listChanged || switchedPage) {
                ClearSelectionsAndRebuild(changedTabView || changedFolderView || movedFolder);
            }
            return listChanged;
        }   

        void ChangeShowParamsOrConditionsMulti (bool showParamsChanged) {
            string nm = showParamsChanged ? AssetObject.showParamsField : AssetObject.showConditionsField;
            foreach (var el in GetSelectionOrAll()) AOList[elements[el].index][nm].SetValue( multiEditAO[nm].boolValue );
        }
        void SetParameterAll(int index) {
            foreach (var el in GetSelectionOrAll()) CustomParameterEditor.CopyParameter (AOList[elements[el].index][AssetObject.params_field][index], multiEditAO[AssetObject.params_field][index] );      
        }
        void EditMultiConditions(bool multiConditionAdd) {
            EditorProp multiConditions = multiEditAO[AssetObject.conditionChecksField];
            int multiCount = multiConditions.arraySize;


            
            
            
            foreach (var el in GetSelectionOrAll()) {     
                EditorProp conditions = AOList[elements[el].index][AssetObject.conditionChecksField];
                if (!multiConditionAdd) conditions.Clear();
                
                for (int i = 0; i < multiCount; i++) {
                    CustomParameterEditor.CopyParameterList(conditions.AddNew()[AssetObject.paramsToMatchField], multiConditions[i][AssetObject.paramsToMatchField]);
                }
            }
            multiConditions.Clear();
        }

        

        IEnumerable<int> GetSelectionOrAll () {
            return ((selectionCount == 0) ? new HashSet<int>().Generate(elements.Length, i => i ) : new HashSet<int>().Generate(selectionCount, i => i + lo_selection )).Where( i => elements[i].id != -1 );
            //return ((selection.Count == 0) ? new HashSet<int>().Generate(elements.Length, i => i ) : selection).Where( i => elements[i].id != -1 );
        
        
        }

        void AddSelectionToSet () {
            //if (selectionCount == 0) return;
            bool reset_i = true;           

 
            for (int i = lo_selection; i <= hi_selection; i++) {
            //foreach (var i in selection) {
                AddNewAssetObject(elements[i].id, GetObjectRefForElement(i), reset_i);
                reset_i = false;
            }
        }
        void DeleteSelectionFromList () {
            if (selectionCount == 0) return;
            
            //HashSet<int> ids = new HashSet<int>().Generate(selection, o => { return elements[o].id; } );
            HashSet<int> ids = new HashSet<int>().Generate(selectionCount, i => { return elements[i + lo_selection].id; } );
            
            for (int i = AOList.arraySize - 1; i >= 0; i--) {
                if (ids.Contains(AOList[i][AssetObject.id_field].intValue)) AOList.DeleteAt(i);
            }
        }

        bool SwitchPage(int offset) {
            int newVal = page + offset;
            if (newVal < 0 || newVal >= max_pages) return false;
            page += offset;
            return true;
        }
        /*
        void HandleSelectionAfterKeyboardInput (bool down, bool up, bool shift) {
            int e = -1;
            
            int lastIndex = elements.Length - 1;
            int selectionCount = this.selectionCount;// selection.Count;
            
            
            if ((down || up) && selectionCount == 0) {
                hi_selection = lo_selection = down ? 0 : lastIndex;
                e = hi_selection;
                //e = elements[hi_selection];
            }

            if (selectionCount != 0) {
                bool changed = false;
                bool unMulti = selectionCount > 1 && !shift;
                if (down) {
                    if (hi_selection < lastIndex || unMulti) {
                        if (hi_selection < lastIndex) hi_selection++;
                        lo_selection = hi_selection;
                        changed = true;
                    }
                }
                if (up) {
                    if (lo_selection > 0 || unMulti) {
                        if (lo_selection > 0) lo_selection--;
                        hi_selection = lo_selection;
                        changed = true;
                    }
                }
                if ((down || up) && changed) {
                    if (unMulti) selection.Clear();
                    e = hi_selection;

                    //e = elements[hi_selection];
                }
            }

            if (e != -1) OnObjectSelection(e, false);

        }
        */

        void HandleSelectionAfterKeyboardInput (bool down, bool up, bool shift) {
            //int e = -1;
            
            int lastIndex = elements.Length - 1;
            int selectionCount = this.selectionCount;// selection.Count;
            
            
            bool changed = false;
            if ((down || up) && selectionCount == 0) {
                hi_selection = lo_selection = down ? 0 : lastIndex;
                //e = hi_selection;
                //e = elements[hi_selection];
                changed = true;
            }

            if (selectionCount != 0) {
                bool unMulti = selectionCount > 1 && !shift;
                if (down) {

                    
                    if (hi_selection < lastIndex || unMulti) {
                        if (hi_selection < lastIndex) hi_selection++;
                        if (unMulti || !shift) lo_selection = hi_selection;
                        changed = true;
                    }
                }
                if (up) {
                    if (lo_selection > 0 || unMulti) {
                        if (lo_selection > 0) lo_selection--;
                        if (unMulti || !shift) hi_selection = lo_selection;
                        changed = true;
                    }
                }
                if ((down || up) && changed) {
                    //if (unMulti) selection.Clear();
                    //e = hi_selection;

                    //e = elements[hi_selection];
                }
            }

            //if (e != -1) OnObjectSelection(e, false);
            if (changed) OnSelectionChange();


        }


        void KeyboardInput (out bool down, out bool up, out bool left, out bool right, out bool h, out bool del, out bool ret, out bool shift) {
            down = up = left = right = h = del = ret = shift = false;
            Event e = Event.current;
            if (e.type != EventType.KeyDown) return;
            if (GUIUtils.KeyboardOverriden()) return;
            down = e.keyCode == KeyCode.DownArrow;
            up = e.keyCode == KeyCode.UpArrow;
            left = e.keyCode == KeyCode.LeftArrow;
            right = e.keyCode == KeyCode.RightArrow;
            h = e.keyCode == KeyCode.H;
            del = e.keyCode == KeyCode.Backspace || e.keyCode == KeyCode.Delete;
            ret = e.keyCode == KeyCode.Return;
            shift = e.shift;
            if (down || up || left || right || del || ret || h) e.Use();              
        }

        

        bool MoveFolder(string addPath = null) {
            bool back = addPath == null;
            if (back) {
                if (foldersHiearchyOffset <= 0) return false;
                currentDirectory = currentDirectory.Substring(0, currentDirectory.Substring(0, currentDirectory.Length-1).LastIndexOf("/") + 1);
            }
            else currentDirectory += addPath + "/";
            foldersHiearchyOffset+= back ? -1 : 1;
            return true;
        }

        void OpenImportSettings () {
            int c = selectionCount;
            
            Animations.EditImportSettings.CreateWizard(
                //new Object[c].Generate(selection, e => { return AssetDatabase.LoadAssetAtPath(objectsDirectory + id2path[elements[e].id], typeof(Object)); } )
                new Object[c].Generate(i => { return AssetDatabase.LoadAssetAtPath(objectsDirectory + id2path[elements[i + lo_selection].id], typeof(Object)); } )
            
            );
        }

        int selectionCount { get { return hasSelection ? (hi_selection - lo_selection) + 1 : 0; } }
        void RebuildPreviewEditor () {
            if (preview != null) Editor.DestroyImmediate(preview);
            //int c = selection.Count;
            int c = selectionCount;
            if (c == 0) return;
            if (SelectionHasDirectories()) return;
            //preview = Editor.CreateEditor(new UnityEngine.Object[c].Generate(selection, s => { return GetObjectRefForElement(s); } ));
            preview = Editor.CreateEditor(new UnityEngine.Object[c].Generate(i => { return GetObjectRefForElement(lo_selection + i); } ));
            
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