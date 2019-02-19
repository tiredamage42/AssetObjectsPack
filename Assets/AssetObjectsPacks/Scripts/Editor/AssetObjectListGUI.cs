using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AssetObjectsPacks {

    public class ListViewElement {
        public string fullName, path;
        public GUIContent gui;
        public int id;
        public EditorProp ao;
        public ListViewElement(string path, string fullName, GUIContent gui, int id, EditorProp ao) => (this.path, this.fullName, this.gui, this.id, this.ao) = (path, fullName, gui, id, ao);
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
        const float minElementWidth = 256;
        static readonly string[] noElementsMsgs = new string[] {
            "No elements, add some through the project view tab",
            "No elements in the pack directory",
        };
        
        bool isListView { get { return ViewToolbarGUI.listProjectView == 0; } }
        bool hasSelection { get { return selection.Count != 0; } }        

        int page, max_pages, foldersHiearchyOffset, lo_selection, hi_selection;
        string currentDirectory = "";
        string[] pathsWithoutIDs, all_file_paths, errorStrings, warningStrings;
        
        HashSet<ListViewElement> selection = new HashSet<ListViewElement>();
        HashSet<int> hiddenIds = new HashSet<int>();
        AssetObjectPack pack;
        ListViewElement[] elements;
        Editor preview;
        SerializedObject eventPack;
        Dictionary<int, string> id2path;
        EditorProp multiEditAO, AOList, hiddenIDsProp, packsProp, packProp, defParamsProp;
        GUIContent[] paramlabels;
        GUIContent folderBackGUI, curPageGUI;
        GUILayoutOption elementWidth, folderWidth = GUILayout.ExpandWidth(true);
        GUILayoutOption[] paramWidths;
        
        void ToggleHiddenSelected () {
            bool toggledAny = false;
            foreach (var e in selection) {
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
                eventPack.ApplyModifiedProperties();
                if (!ToolbarGUI.showHidden) ClearSelectionsAndRebuild();
            }

        }

        void RecalculateLowestAndHighestSelections () {
            lo_selection = int.MaxValue;
            hi_selection = int.MinValue;
            int s = selection.Count;
            if (s == 0) return;
            int c = elements.Length;
            for (int i = 0; i < c; i++) {
                if (selection.Contains(elements[i])) {                    
                    if (i < lo_selection) lo_selection = i;
                    if (i > hi_selection) hi_selection = i;
                    if (s == 1) break;
                }
            }
        }
  
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
        void OnSelectionChange() {
            RecalculateLowestAndHighestSelections();
            RebuildPreviewEditor();        
        }

        bool SelectionHasDirectories() {
            foreach (var s in selection) {
                if (s.id == -1) return true;
            }
            return false;
        }

        UnityEngine.Object GetObjectRefForElement(ListViewElement e) {
            return EditorUtils.GetAssetAtPath(pack.objectsDirectory + e.path, pack.assetType);  
        }
    
        public void RealOnEnable (EditorProp packsProp, SerializedObject eventPack) {
            this.eventPack = eventPack;
            this.packsProp = packsProp;

            AOList = new EditorProp ( eventPack.FindProperty(AssetObjectEventPack.asset_objs_field) );
            hiddenIDsProp = new EditorProp ( eventPack.FindProperty(AssetObjectEventPack.hiddenIDsField) );
            multiEditAO = new EditorProp ( eventPack.FindProperty(AssetObjectEventPack.multi_edit_instance_field) );
        
            hiddenIds = hiddenIds.Generate(hiddenIDsProp.arraySize, i => { return hiddenIDsProp[i].intValue; } );
        }

        public void InitializeWithPack (AssetObjectPack pack, EditorProp packProp) {
            this.pack = pack;
            this.packProp = packProp;

            errorStrings = HelpGUI.GetErrorStrings(packsProp, packProp);
            System.Array.Resize(ref pathsWithoutIDs, 0);
            if (errorStrings.Length == 0) {
                InitializeAllFilePaths();
            }
            warningStrings = HelpGUI.GetWarningStrings(packsProp, packProp, pathsWithoutIDs);                
            

            defParamsProp = null;
            paramlabels = new GUIContent[0];
            paramWidths = new GUILayoutOption[0];
                
            //dummy_list_element = null;

            if (errorStrings.Length == 0) {
                defParamsProp = packProp[AssetObjectPack.defaultParametersField];
                int c = AOList.arraySize;
                for (int i = 0; i < c; i++) AOParameters.UpdateParametersToReflectDefaults(AOList[i][AssetObject.params_field], defParamsProp);
            
                //clear conditionals as well
                multiEditAO[AssetObject.conditionChecksField].Clear();
                AOParameters.ClearAndRebuildParameters(multiEditAO, defParamsProp);//, defs);
                
                
                int props_count = pack.defaultParameters.Length;
                paramlabels = new GUIContent[props_count];
                paramWidths = new GUILayoutOption[props_count];
                for (int i = 0; i < props_count; i++) {
                    paramlabels[i] = new GUIContent(pack.defaultParameters[i].name, pack.defaultParameters[i].hint);
                    paramWidths[i] = GUILayout.Width(EditorStyles.label.CalcSize(paramlabels[i]).x);
                }
                
                //dummy_list_element = new DummyListElement(pack.defaultParameters);
            }
        
            InitializeView();
            eventPack.ApplyModifiedProperties();
        }

        void RebuildAllElements() {
            
            HashSet<string> used = new HashSet<string>();            
            int lastDir = 0;

            HashSet<int> idsInSet = isListView ? null : new HashSet<int>().Generate(AOList.arraySize, i => { return GetObjectIDAtIndex(i); }); 
        
            List<ListViewElement> unpaginated = new List<ListViewElement>();

            int l = isListView ? AOList.arraySize : all_file_paths.Length;
            for (int i = 0; i < l; i++) {
                
                int id;
                string path;
                EditorProp ao = isListView ? AOList[i] : null;
                if (isListView) {
                    id = ao[AssetObject.id_field].intValue;
                    path = id2path[id];
                }
                else {
                    path = all_file_paths[i];
                    id = AssetObjectsEditor.GetObjectIDFromPath(path);
                }

                if (!isListView && idsInSet.Contains(id)) continue;
                if (!ToolbarGUI.showHidden && hiddenIds.Contains(id)) continue;
                if (!ToolbarGUI.PathPassesSearchFilter(path)) continue;

                if (ViewToolbarGUI.folderedView) {

                    if (!currentDirectory.IsEmpty() && !path.StartsWith(currentDirectory)) continue;

                    string name = path;
                    if (path.Contains("/")) name = path.Split('/')[foldersHiearchyOffset];    

                    if (name.Contains(".")) { 
                        name = AssetObjectsEditor.RemoveIDFromPath(name);                        
                        unpaginated.Add(new ListViewElement(path, name, new GUIContent( name ), id, ao));
                        continue;
                    }
                        
                    if (used.Contains(name)) continue;
                    used.Add(name);
                    unpaginated.Insert(lastDir, new ListViewElement(path, name, new GUIContent( name ), -1, null));
                    lastDir++;       
                    continue;
                }
                unpaginated.Add(new ListViewElement(path, path, new GUIContent(AssetObjectsEditor.RemoveIDFromPath(path)), id, ao));
            }

            l = unpaginated.Count;

            max_pages = (l / elementsPerPage) + Mathf.Min(1, l % elementsPerPage);
            int min = page * elementsPerPage;
            int max = Mathf.Min(min + elementsPerPage, l - 1);

            curPageGUI = new GUIContent("Page: " + (page + 1) + " / " + max_pages);
        
            elements = unpaginated.Slice(min, max).ToArray();

            l = elements.Length;
            float maxWidth = 0;
            for (int i = 0; i < l; i++) {
                float w = EditorStyles.toolbarButton.CalcSize(elements[i].gui).x;
                if (w > maxWidth) maxWidth = w;
            }
            elementWidth = !isListView ? GUILayout.ExpandWidth(true) : GUILayout.Width( Mathf.Max(maxWidth, minElementWidth ) );
        }
        ListViewElement DrawElements (){
            if (elements.Length == 0) {
                EditorGUILayout.HelpBox(noElementsMsgs[ViewToolbarGUI.listProjectView], MessageType.Info);
                return null;
            }
            GUIUtils.StartBox(0);
            ListViewElement selectedElement = null;
            int l = elements.Length;
            for (int i = 0; i < l; i++) {
                ListViewElement e = elements[i];
                bool s = selection.Contains(e);
                if (e.id == -1) {
                    if (GUIUtils.ScrollWindowElement (e.gui, s, false, true, folderWidth)) selectedElement = e;
                }
                else {
                    if (AssetObjectGUI.DrawAssetObject(e.ao, isListView, e.gui, s, hiddenIds.Contains(e.id), elementWidth, paramWidths)) selectedElement = e;
                }       
            }
            GUIUtils.EndBox(0);
            return selectedElement;
        }

        void AddElementsToSet (HashSet<ListViewElement> elements_to_add) {
            if (elements_to_add.Count == 0) return;
            bool reset_i = true;
            foreach (ListViewElement e in elements_to_add) {
                AddNewAssetObject(e.id, GetObjectRefForElement(e), reset_i);
                reset_i = false;
            }
            ClearSelectionsAndRebuild();
        }
        void AddNewAssetObject (int obj_id, UnityEngine.Object obj_ref, bool make_default) {
            EditorProp ao = AOList.AddNew();
            ao[AssetObject.id_field].SetValue ( obj_id );
            ao[AssetObject.obj_ref_field].SetValue ( obj_ref );
            
            //only need to default first one added, the rest will copy the last one 'inserted' into the
            //serialized property array
            if (!make_default) return;
            ao[AssetObject.conditionChecksField].Clear();
            AOParameters.ClearAndRebuildParameters(ao[AssetObject.params_field], defParamsProp);
        }
        
        void InitializeAllFilePaths () {
            all_file_paths = AssetObjectsEditor.GetAllAssetObjectPaths (pack.objectsDirectory, pack.fileExtensions, false, out pathsWithoutIDs, out id2path);
        }
        
        public bool Draw (){

            bool generateNewIDs;
            HelpGUI.DrawErrorsAndWarnings(errorStrings, warningStrings, pathsWithoutIDs, out generateNewIDs);
           
            if (errorStrings.Length != 0) return false;
            

            bool folderBack, changedTabView, changedFolderView;
            ViewToolbarGUI.DrawToolbar(out changedTabView, out folderBack, out changedFolderView, folderBackGUI, EditorStyles.miniButton, foldersHiearchyOffset);
            
            bool importSettings, toggleHidden,showHiddenToggled, resetHidden, removeOrAdd, searchChanged;
            ToolbarGUI.DrawToolbar (isListView, EditorStyles.miniButton, hasSelection && !SelectionHasDirectories(), out importSettings, out toggleHidden, out showHiddenToggled, out resetHidden, out removeOrAdd, out searchChanged);

            bool down, up, left, right, del, ret, h, shift;
            KeyboardInput (out down, out up, out left, out right, out h, out del, out ret, out shift);
          
            int setProp = !isListView ? -1 : MultiEditGUI.DrawMultiEditGUI(multiEditAO, paramlabels, elementWidth, paramWidths);
            
            ListViewElement selectedElement = DrawElements();

            bool prevPage, nextPage;
            PagesGUI.DrawPages (EditorStyles.toolbarButton, out prevPage, out nextPage, curPageGUI);


            HandleSelectionAfterKeyboardInput (down, up, shift);

            if (hasSelection && ret) {
                if (SelectionHasDirectories()) {
                    if (selection.Count == 1) MoveForwardFolder(selection.First().fullName);
                    ret = false;
                }
            } 
                
            bool listChanged = false;
            
            if (selectedElement != null) {
                if (selectedElement.id == -1) {
                    MoveForwardFolder(selectedElement.fullName);
                }
                else {
                    OnObjectSelection(selectedElement, selection.Contains(selectedElement));
                }
            }

            SetParameterAll(setProp);

            if (setProp != -1) {
                listChanged = true;
            }
            if (changedTabView || changedFolderView) {
                InitializeView();
            }
            if (importSettings) {
                OpenImportSettings();
            }
            if (resetHidden) {
                hiddenIDsProp.Clear();
                hiddenIds.Clear();
                eventPack.ApplyModifiedProperties();
                ClearSelectionsAndRebuild();
            }

            if (hasSelection) {

                bool remove = isListView && del;
                bool add = !isListView && ret;

                if (removeOrAdd || add || remove) {
                    if (isListView || add) {
                        DeleteSelectionFromList( );
                    }
                    else {
                        AddElementsToSet( selection );
                    }
                    listChanged = true;
                }
            }

            if (hasSelection && (toggleHidden || h )) ToggleHiddenSelected();

            if (showHiddenToggled || searchChanged) {
                ClearSelectionsAndRebuild();
            }

            if (folderBack) {
                MoveBackward();
            } 
            if (nextPage || right) {
                NextPage();
            }
            if (prevPage || left) {
                bool wentPrevious = PreviousPage();
                if (left && !wentPrevious && ViewToolbarGUI.folderedView) {
                    MoveBackward();
                }
            }
            
            if (generateNewIDs) {
                AssetObjectsEditor.GenerateNewIDs(all_file_paths, pathsWithoutIDs);
                InitializeAllFilePaths();
                warningStrings = HelpGUI.GetWarningStrings(packsProp, packProp, pathsWithoutIDs);                
                InitializeView();
            }
            return listChanged;
        }   

        void DeleteSelectionFromList () {
            HashSet<int> ids = new HashSet<int>().Generate(selection, o => { return o.id; } );
            for (int i = AOList.arraySize - 1; i >= 0; i--) {
                if (ids.Contains(GetObjectIDAtIndex(i))) AOList.DeleteAt(i);
            }
            ClearSelectionsAndRebuild();
        }

        void NextPage() {
            if (page+1 >= max_pages) return;
            page++;
            ClearSelectionsAndRebuild();   
        }
        bool PreviousPage() {
            if (page-1 < 0) return false;
            page--;
            ClearSelectionsAndRebuild();
            return true;
        }

        void HandleSelectionAfterKeyboardInput (bool down, bool up, bool shift) {
            ListViewElement e = null;
            int l = elements.Length - 1;
            int c = selection.Count;
            if ((down || up) && c == 0) {
                hi_selection = lo_selection = down ? 0 : l;
                e = elements[hi_selection];
            }
            if (c != 0) {
                bool changed = false;
                bool unMulti = c > 1 && !shift;
                if (down) {
                    if (hi_selection < l || unMulti) {
                        if (hi_selection < l) hi_selection++;
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
                    e = elements[hi_selection];
                }
            }
            if (e != null) OnObjectSelection(e, false);
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

        void SetParameterAll(int index) {
            if (index == -1) return;
            EditorProp copy_prop = multiEditAO[AssetObject.params_field][index];
            
            IEnumerable<ListViewElement> l = elements;
            if (selection.Count != 0) l = selection;
            foreach (ListViewElement el in l) {     
                AOParameters.CopyParameter (el.ao[AssetObject.params_field][index], copy_prop );      
            }
        }

        int GetObjectIDAtIndex(int index) {
            return AOList[index][AssetObject.id_field].intValue;
        }

        void InitializeView () {
            page = 0;
            ClearSelectionsAndRebuild();
        }
        
        void ClearSelectionsAndRebuild() {
            selection.Clear();
            OnSelectionChange();
            RebuildAllElements();
        }

        void OnFolderViewChange () {
            folderBackGUI = new GUIContent( "  <<  " + currentDirectory);
            InitializeView();
        }

        void MoveBackward () {
            if (foldersHiearchyOffset <= 0) return;
            foldersHiearchyOffset--;
            string noSlash = currentDirectory.Substring(0, currentDirectory.Length-1);
            int cutoff = noSlash.LastIndexOf("/") + 1;
            currentDirectory = currentDirectory.Substring(0, cutoff);
            OnFolderViewChange();                
        }
        
        void MoveForwardFolder (string addPath) {
            foldersHiearchyOffset++;
            currentDirectory += addPath + "/";
            OnFolderViewChange();
        }

        void OpenImportSettings () {
            Animations.EditImportSettings.CreateWizard(new UnityEngine.Object[selection.Count].Generate(selection, e => { return AssetDatabase.LoadAssetAtPath(pack.objectsDirectory + e.path, typeof(UnityEngine.Object)); } ));
        }
        void RebuildPreviewEditor () {
            if (preview != null) Editor.DestroyImmediate(preview);
            int c = selection.Count;
            if (c == 0) return;
            if (SelectionHasDirectories()) return;
            preview = Editor.CreateEditor(new UnityEngine.Object[c].Generate(selection, s => { return GetObjectRefForElement(s); } ));
            preview.HasPreviewGUI();
            preview.OnInspectorGUI();
            preview.OnPreviewSettings();

            //auto play single selection for animations
            if (c == 1) {
                if (pack.assetType == "UnityEngine.AnimationClip") {     
                    
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