using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
namespace AssetObjectsPacks {
    public class ListViewElement {
        public string fullName, file_path;
        public GUIContent label_gui;
        public int object_id;
        public SerializedProperty ao;
        
        public ListViewElement(string file_path, string fullName, GUIContent label_gui, int object_id, SerializedProperty ao) => (this.file_path, this.fullName, this.label_gui, this.object_id, this.ao) = (file_path, fullName, label_gui, object_id, ao);
        //protected void Initialize(string file_path, string fullName, GUIContent label_gui, int object_id) 
        //=> (this.file_path, this.fullName, this.label_gui, this.object_id) = (file_path, fullName, label_gui, object_id);
    }

    public abstract class SelectionView<L> where L : ListViewElement {
        //protected List<L> elements = new List<L>();
        //protected HashSet<L> selected_elements = new HashSet<L>();
        //protected SerializedProperty ao_list;
        //protected SerializedObject eventPack;
        //Editor preview;
        //protected AssetObjectPack pack;

        //FoldersView folderView = new FoldersView();
        //PaginateView paginateView = new PaginateView();
        //HiddenView hiddenView = new HiddenView();

        //protected GUILayoutOption max_name_width;
        //protected int max_elements_per_page = 25;
        //protected bool foldered;

        //GUIContent foldersViewGUI = new GUIContent("Folders", "Enable/Disable Folders View");
        //GUIContent importSettingsGUI = new GUIContent("Import Settings");
        
/*
        public virtual void RealOnEnable(SerializedObject eventPack){
            this.eventPack = eventPack;
            //this.ao_list = eventPack.FindProperty(AssetObjectEventPack.asset_objs_field);
            //hiddenView.OnEnable(eventPack, AssetObjectEventPack.hiddenIDsField);

        }
 */
        //public virtual void InitializeWithPack(AssetObjectPack pack) {
        //    this.pack = pack;
        //}
        /*
        protected HashSet<int> IDSetFromElements (IEnumerable<ListViewElement> elements) {
            return new HashSet<int>().Generate(elements, o => { return o.object_id; } );
        }

        protected abstract bool DrawNonFolderElement(ListViewElement element, int index, bool selected, bool hidden, GUILayoutOption element_width);
        
        protected void DrawElements (string[] all_paths, HashSet<ListViewElement> selection) {

            GUIUtils.StartBox();

            for (int i = 0; i < elements.Count; i++) {

                L element = elements[i];

                bool selected = selection.Contains(element);

                    
                if (elements[i].object_id == -1) {
                    if (GUIUtils.ScrollWindowElement (elements[i].label_gui, selected, false, true, GUILayout.ExpandWidth(true))) {
                        MoveForwardFolder(elements[i].fullName, all_paths, selection);
                    }
                }
                else {
                    EditorGUILayout.BeginHorizontal();

                    PreSelectButton(i);
                    
                    if (GUIUtils.ScrollWindowElement (element.label_gui, selected, hiddenView.IsHidden(element.object_id), false, max_name_width)) OnObjectSelection(element, selected, selection);
                    
                    PostSelectButton(element, i);

                    EditorGUILayout.EndHorizontal();

                    NonFolderSecondTier(all_paths, element, i);
                }       
            }
            GUIUtils.EndBox();

        }
         */
        //protected abstract void PreSelectButton (int index);

        //protected abstract void PostSelectButton (L element, int index);

        //protected abstract void NonFolderSecondTier(string[] all_paths, L element, int index);

        //protected virtual void OnPagination () {

        //}
        /*
        protected void OnFolderViewChange (string[] all_paths, HashSet<ListViewElement> selection) {
            paginateView.ResetPage();
            ClearSelectionsAndRebuild(all_paths, selection);
        }

        protected void MoveForwardFolder (string addPath, string[] all_paths, HashSet<ListViewElement> selection) {
            folderView.MoveForward(addPath);
            OnFolderViewChange(all_paths, selection);
        }
         */
        /*
        protected bool ElementPassedListedView (int id, string file_path, out GUIContent gui) {
            gui = null;


            bool is_hidden = hiddenView.IsHidden(id);
            if (!hiddenView.showHidden && is_hidden) return false;

            if (!search_string.IsEmpty() && !file_path.Contains(search_string)) return false;
            
            string n = AssetObjectsEditor.RemoveIDFromPath(file_path);
            gui = new GUIContent(n);
            return true;
        }
        
        protected bool ElementPassedFolderedView (int id, string file_path, ref HashSet<string> usedNames, out GUIContent gui, out bool isDirectory) {
            gui = null;
            isDirectory = false;

            bool is_hidden = hiddenView.IsHidden(id);
            if (!hiddenView.showHidden && is_hidden) return false;
                
            if (!folderView.DisplaysPath(file_path)) return false;

            if (!search_string.IsEmpty() && !file_path.Contains(search_string)) return false;

            string name_display = folderView.DisplayNameFromPath(file_path);
            if (usedNames.Contains(name_display)) return false;
            usedNames.Add(name_display);

            isDirectory = !name_display.Contains(".");
            if (!isDirectory) { 
                gui = new GUIContent( AssetObjectsEditor.RemoveIDFromPath(EditorUtils.RemoveDirectory(file_path)) );                
            }
            else {
                gui = new GUIContent( name_display );
            }
            
            return true;
        }
       
        void ClearSelections(HashSet<ListViewElement> selection){
            selection.Clear();
            OnSelectionChange(selection);
        }
        public void ClearSelectionsAndRebuild(string[] all_paths, HashSet<ListViewElement> selection) {
            ClearSelections(selection);
            RebuildAllElements(all_paths);
        }

        public virtual void InitializeView (string[] all_paths, HashSet<ListViewElement> selection) {
            paginateView.ResetPage();
            ClearSelectionsAndRebuild(all_paths, selection);
        }

        protected int GetObjectIDAtIndex(int index) {
            return ao_list.GetArrayElementAtIndex(index).FindPropertyRelative(AssetObject.id_field).intValue;
        }
        
        protected abstract List<L> UnpaginatedFoldered(string[] all_paths);
        protected abstract List<L> UnpaginatedListed(string[] all_paths);
         */
        /*
        void RebuildAllElements(string[] all_paths) {
            elements.Clear();

            List<L> unpaginated;
            if (foldered) unpaginated = UnpaginatedFoldered(all_paths);
            else unpaginated = UnpaginatedListed(all_paths);
            
            int min, max;
            paginateView.Paginate(unpaginated.Count, max_elements_per_page, out min, out max);
            elements.AddRange(unpaginated.Slice(min, max));
            
            int l = elements.Count;
            float max_width = 0;
            
            for (int i = 0; i < l; i++) {

                float w = EditorStyles.toolbarButton.CalcSize(elements[i].label_gui).x;
                if (w > max_width) max_width = w;
            
            }
                
            max_name_width = GUILayout.Width( Mathf.Max(max_width, min_element_width ) );

            //OnPagination();
        }

        const float min_element_width = 256;
        
        bool show_help;
        int help_tab;

        GUIContent[] helpTabsGUI = new GUIContent[] {
            new GUIContent("Help: Controls"),
            new GUIContent("Help: Conditions"),
            new GUIContent("Help: Multi-Editing"),
        };

        string[] helpTexts = new string[] {
            controls_help, conditions_help, multi_edit_help
        };

        GUIContent helpGUI = new GUIContent("[?]", "Show Help");
         */
        //protected abstract void ExtraToolbarButtons(string[] all_paths, bool has_selection, bool selection_has_directories, HashSet<ListViewElement> selection);
       /*
        protected void DrawToolbar (string[] all_paths, HashSet<ListViewElement> selection) {

            GUIStyle toolbar_style = EditorStyles.miniButton;

            GUIUtils.StartBox();

            if (show_help) {

                GUIStyle myStyle = new GUIStyle(EditorStyles.helpBox);
                myStyle.richText = true;
                
                GUIUtils.Space(2);
                bool changed;
                EditorGUILayout.TextArea(helpTexts[help_tab], myStyle);
                help_tab = GUIUtils.Tabs(helpTabsGUI, help_tab, out changed, true);
                GUIUtils.Space(2);
                
            }

            GUIUtils.Space();

            EditorGUILayout.BeginHorizontal();
            show_help = GUIUtils.ToggleButton(helpGUI, true, show_help, toolbar_style);
            
            bool has_selection = selection.Count != 0;
            bool selection_has_directories = false;
            if (has_selection) { selection_has_directories = SelectionHasDirectories(selection); }
            if (foldered && folderView.DrawBackButton()) OnFolderViewChange(all_paths, selection);

            bool lastview = foldered;
            foldered = GUIUtils.ToggleButton(foldersViewGUI, true, foldered, toolbar_style);
            if (foldered != lastview) InitializeView(all_paths, selection);

            GUI.enabled = has_selection && !selection_has_directories;
            if (GUIUtils.Button(importSettingsGUI, true, toolbar_style)) OpenImportSettings(selection);
            GUI.enabled = true;

            GUI.enabled = has_selection && !selection_has_directories;            
            
            bool toggled_hidden_selected = hiddenView.ToggleHiddenButton( IDSetFromElements( selection ) );
            
            GUI.enabled = true;
            
            bool toggled_show_hidden = hiddenView.ToggleShowHiddenButton();
            
            bool reset_hidden = hiddenView.ResetHiddenButton();
            
            if (toggled_hidden_selected || toggled_show_hidden || reset_hidden) ClearSelectionsAndRebuild(all_paths, selection);
        
            ExtraToolbarButtons(all_paths, has_selection, selection_has_directories, selection);

            EditorGUILayout.EndHorizontal();

            GUIUtils.Space();
            DrawSearchBar(all_paths, selection);
            GUIUtils.Space();

            GUIUtils.EndBox();
        }

        void DrawSearchBar (string[] all_paths, HashSet<ListViewElement> selection) {
            EditorGUILayout.BeginHorizontal();

            GUIUtils.Label(searchGUI, true);

            string last_search = search_string;
            // Set the internal name of the textfield
            GUI.SetNextControlName(searchControlName);
            search_string = EditorGUILayout.DelayedTextField(GUIContent.none, search_string, search_options);
            
            bool searchClicked;
            if (ClickHappened(out searchClicked)) {
                if (!searchClicked) EditorGUI.FocusTextInControl("");
            }

            if (search_string != last_search) ClearSelectionsAndRebuild(all_paths, selection);
            
            EditorGUILayout.EndHorizontal();
        }


        */


        /*

        GUILayoutOption search_options = GUILayout.MaxWidth(345);

        bool ClickHappened (out bool lastElementClicked) {
            Event e = Event.current;
            bool click_happened = e.type == EventType.MouseDown && e.button == 0;            
            lastElementClicked = false;
            if (click_happened) lastElementClicked = GUILayoutUtility.GetLastRect().Contains(e.mousePosition);
            return click_happened;
        }



        const string searchControlName = "SearchField";

        GUIContent searchGUI = new GUIContent("Search:");

        string search_string;


        
        protected void DrawPaginationGUI (string[] all_paths, HashSet<ListViewElement> selection) {
            if (paginateView.ChangePageGUI ()) ClearSelectionsAndRebuild(all_paths, selection);
        }

        bool NextPage(string[] all_paths, HashSet<ListViewElement> selection) {
            if (paginateView.NextPage()) {
                ClearSelectionsAndRebuild(all_paths, selection);   
                return true;
            }
            return false;
        }
        bool PreviousPage(string[] all_paths, HashSet<ListViewElement> selection) {
            if (paginateView.PreviousPage()) {
                ClearSelectionsAndRebuild(all_paths, selection);
                return true;
            }
            return false;
                
        }
         */
/* 

const string controls_help = @"
    <b>CONTROLS:</b>

    Click on the name to select an element.

    <b>[ Shift ] / [ Ctrl ] / [ Cmd ]</b> Multiple selections
    <b>[ Del ] / [ Backspace ]</b> Delete selection from list (In List View).
    <b>[ Enter ] / [ Return ]</b> Add selection to list (In Explorer View).
    <b>[ H ]</b> Hide / Unhide selection.
    
    <b>Arrows:</b>
    <b>[ Left ]</b> Page Back ( Folder View Back when page 0 ).
    <b>[ Right ]</b> Page Fwd.
    <b>[ Up ] / [ Down ]</b> Scroll selection
";
const string conditions_help = @"
    <b>CONDITIONS:</b>

    When an Event Player plays an event, 
    each Asset Object will be available for random selection when:

        1.  it has no conditions
        2.  if at least one of its conditions are met.

    A condition is considered met when all of the condition's parameters match 
    the corresponding named parameter on the player

    conditions are 'or's, parameters in each conditon are 'and's.
        
";
const string multi_edit_help = @"
    <b>MULTI-EDITING (LIST VIEW):</b>

    To multi edit a parameter, change it in the multi edit box (at the top).
    then click the blue button to the right of the parameter name

    if no elements are selected, changes will be made to all shown elements.
    
    <b>When multi editing conditions:</b>

    The <b>'Add'</b> button adds the changed conditions list to the each asset objects' conditions list.
    The <b>'Replace'</b> button replaces each asset object's conditions with the changed conditions list.


";

*/


/*

        protected void KeyboardInput (string[] all_paths, out bool enter_pressed, out bool delete_pressed, HashSet<ListViewElement> selection){
            
            
            delete_pressed = enter_pressed = false;
            
            if (GUI.GetNameOfFocusedControl() == searchControlName) return;
            Event e = Event.current;
            if (e.type != EventType.KeyDown) return;
            

            bool down_p = e.keyCode == KeyCode.DownArrow;
            bool up_p = e.keyCode == KeyCode.UpArrow;
            bool left_p = e.keyCode == KeyCode.LeftArrow;
            bool right_p = e.keyCode == KeyCode.RightArrow;
            bool h_pressed = e.keyCode == KeyCode.H;
            delete_pressed = e.keyCode == KeyCode.Backspace || e.keyCode == KeyCode.Delete;
            enter_pressed = e.keyCode == KeyCode.Return;

            if (down_p || up_p || left_p || right_p || delete_pressed || enter_pressed || h_pressed) e.Use();    
            
            L element_to_select = null;

            int c = selection.Count;
            if ((down_p || up_p) && c == 0) {
                hi_selection = lo_selection = down_p ? 0 : elements.Count - 1;
                element_to_select = elements[hi_selection];
            }
            if (c != 0) {
                bool sel_changed = false;
                if (down_p) {
                    if (hi_selection < elements.Count - 1 || (c > 1 && !e.shift)) {
                        if (hi_selection < elements.Count - 1) hi_selection++;
                        lo_selection = hi_selection;
                        sel_changed = true;
                    }
                }
                if (up_p) {
                    if (lo_selection > 0 || (c > 1 && !e.shift)) {
                        if (lo_selection > 0) lo_selection--;
                        hi_selection = lo_selection;
                        sel_changed = true;
                    }
                }
                if ((down_p || up_p) && sel_changed) {
                    if (c > 1 && !e.shift) selection.Clear();
                    element_to_select = elements[hi_selection];
                }
            }
            if (element_to_select != null) OnObjectSelection(element_to_select, false, selection);


            if (right_p) {
                if (NextPage(all_paths, selection)) {}
            }
            if (left_p) {
                if (PreviousPage(all_paths, selection)) {}
                else {
                    if (foldered) {
                        if (folderView.MoveBackward()) {
                            OnFolderViewChange(all_paths, selection);
                        }
                    }
                }
            }      

            if (selection.Count != 0) {
                
                if (enter_pressed) {
                    if (SelectionHasDirectories(selection)) {
                        if (selection.Count == 1) {
                            foreach (var s in selection) MoveForwardFolder(s.fullName, all_paths, selection);
                        }
                        enter_pressed = false;
                    }
                }
                if (h_pressed) {
                    if (!SelectionHasDirectories(selection)) {
                        if (hiddenView.ToggleHidden(IDSetFromElements( selection ), false, true)) {
                            ClearSelectionsAndRebuild(all_paths, selection);
                        }
                    }
                }
            } 

            
        }


        void OnObjectSelection (L selected, bool was_selected, HashSet<ListViewElement> selection) {
            Event e = Event.current;
            if (e.shift) {
                selection.Add(selected);
                RecalculateLowestAndHighestSelections(selection);
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
            OnSelectionChange(selection);
        }
        
        int lo_selection, hi_selection;
        void RecalculateLowestAndHighestSelections (HashSet<ListViewElement> selection) {
            lo_selection = int.MaxValue;
            hi_selection = int.MinValue;
            int s = selection.Count;
            if (s == 0) return;
            
            int c = elements.Count;
            for (int i = 0; i < c; i++) {
                if (selection.Contains(elements[i])) {                    
                    if (i < lo_selection) 
                        lo_selection = i;
                    if (i > hi_selection) 
                        hi_selection = i;
                    if (s == 1) break;
                }
            }
        }
  
        
        void OnSelectionChange(HashSet<ListViewElement> selection) {
            RecalculateLowestAndHighestSelections(selection);
            RebuildPreviewEditor(selection);        
        }

 */
        /*
        
        void OpenImportSettings (HashSet<ListViewElement> selection) {
            Animations.EditImportSettings.CreateWizard(new Object[selection.Count].Generate(selection, e => { return AssetDatabase.LoadAssetAtPath(pack.objectsDirectory + e.file_path, typeof(Object)); } ));
        }

        protected void ReInitializeAssetObjectParameters(SerializedProperty ao, AssetObjectParamDef[] defs) {
            SerializedProperty params_list = ao.FindPropertyRelative(AssetObject.params_field);
            params_list.ClearArray();
            int l = defs.Length;            
            for (int i = 0; i < l; i++) {                    
                SerializedPropertyUtils.Parameters.CopyParameter(params_list.AddNewElement(), defs[i].parameter);
            }
        }

        public virtual void OnInteractivePreviewGUI(Rect r, GUIStyle background) {
            if (preview != null) preview.OnInteractivePreviewGUI(r, background); 
        }
        public void OnPreviewSettings() {
            if (preview != null) preview.OnPreviewSettings();
        }
        protected bool SelectionHasDirectories(HashSet<ListViewElement> selection) {
            foreach (var s in selection) {
                if (s.object_id == -1) return true;
            }
            return false;
        }
        protected Object GetObjectRefForElement(ListViewElement e) {
            return EditorUtils.GetAssetAtPath(pack.objectsDirectory + e.file_path, pack.assetType);  
        }
         */
        /*
        void RebuildPreviewEditor (HashSet<ListViewElement> selection) {
            if (preview != null) Editor.DestroyImmediate(preview);
            int c = selection.Count;
            if (c == 0) return;
            if (SelectionHasDirectories(selection)) return;


            Object[] objs = new Object[selection.Count].Generate(selection, s => { return GetObjectRefForElement(s); } );

            //HashSet<Object> objs = new HashSet<Object>();   
            //foreach (L s in selected_elements) objs.Add( GetObjectRefForElement(s) );
            
            //preview = Editor.CreateEditor(objs.ToArray());
            preview = Editor.CreateEditor(objs);
            
            preview.HasPreviewGUI();
            preview.OnInspectorGUI();
            preview.OnPreviewSettings();

            //auto play single selection for animations
            //preview_editor.m_AvatarPreview.timeControl.playing = true
            if (c == 1) {
                if (pack.assetType == "UnityEngine.AnimationClip") {     
                    var flags = System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
                    var avatarPreview = preview.GetType().GetField("m_AvatarPreview", flags).GetValue(preview);
                    var timeControl = avatarPreview.GetType().GetField("timeControl", flags).GetValue(avatarPreview);
                    var setter = timeControl.GetType().GetProperty("playing", flags).GetSetMethod(true);
                    setter.Invoke(timeControl, new object[] { true });
                }
            }
        }
         */
    }
}



