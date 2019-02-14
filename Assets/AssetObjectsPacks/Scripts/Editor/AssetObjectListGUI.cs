using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;

namespace AssetObjectsPacks {
    public class AssetObjectListGUI {
        int selection_view;
        string[] no_ids;
        string[] all_file_paths;
        GUIContent[] tab_guis = new GUIContent[] { new GUIContent("List"), new GUIContent("Explorer") };
        AssetObjectListView list_view = new AssetObjectListView();
        AssetObjectExplorerView explorer_view = new AssetObjectExplorerView();
        AssetObjectPack pack;

        GUIContent showConditionsGUI = new GUIContent("C", "Show Conditions");

        protected SerializedProperty ao_list;
        


        

        //delegate bool DrawMethod(string[] all_paths, HashSet<ListViewElement> selection);
        //delegate bool DrawNonFolderElement(ListViewElement element, int index, bool selected, bool hidden, GUILayoutOption selection_width);
        
        //DrawNonFolderElement[] drawNonFolderElement;
        //delegate List<ListViewElement> GetUnpaginated (string[] all_paths);
        
        //DrawMethod[] draw_methods;
        //Action<Rect, GUIStyle>[] on_interactive_previews;
        //Action<string[], HashSet<ListViewElement>>[] initialize_views;
        //GetUnpaginated[] unpaginatedListeds, unpaginatedFoldereds;
        
        //Action[] on_preview_settings;


        HashSet<ListViewElement> selection = new HashSet<ListViewElement>();
        List<ListViewElement> elements = new List<ListViewElement>();


        GUIContent foldersViewGUI = new GUIContent("Folders", "Enable/Disable Folders View");
        GUIContent importSettingsGUI = new GUIContent("Import Settings");

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


        const string searchControlName = "SearchField";

        GUIContent searchGUI = new GUIContent("Search:");

        string search_string;

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


        

        


 int lo_selection, hi_selection;
        void RecalculateLowestAndHighestSelections () {
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
                if (s.object_id == -1) return true;
            }
            return false;
        }
        Editor preview;

        UnityEngine.Object GetObjectRefForElement(ListViewElement e) {
            return EditorUtils.GetAssetAtPath(pack.objectsDirectory + e.file_path, pack.assetType);  
        }
        
        
        

        void RebuildPreviewEditor () {
            if (preview != null) Editor.DestroyImmediate(preview);
            int c = selection.Count;
            if (c == 0) return;
            if (SelectionHasDirectories()) return;


            UnityEngine.Object[] objs = new UnityEngine.Object[selection.Count].Generate(selection, s => { return GetObjectRefForElement(s); } );

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

















        public void RealOnEnable (SerializedObject eventPack) {
            this.eventPack = eventPack;
            //draw_methods = new DrawMethod[] { list_view.Draw, explorer_view.Draw };
            //on_interactive_previews = new Action<Rect, GUIStyle>[] { list_view.OnInteractivePreviewGUI, explorer_view.OnInteractivePreviewGUI };
            //on_preview_settings = new Action[] { list_view.OnPreviewSettings, explorer_view.OnPreviewSettings };
            //initialize_views = new Action<string[], HashSet<ListViewElement>>[] { list_view.InitializeView, explorer_view.InitializeView };
            
            //unpaginatedFoldereds = new GetUnpaginated[] { list_view.UnpaginatedFoldered, explorer_view.UnpaginatedFoldered };
            //unpaginatedListeds = new GetUnpaginated[] { list_view.UnpaginatedListed, explorer_view.UnpaginatedListed };
            //drawNonFolderElement = new DrawNonFolderElement[] { list_view.DrawNonFolderElement, explorer_view.DrawNonFolderElement };

            this.ao_list = eventPack.FindProperty(AssetObjectEventPack.asset_objs_field);
            hiddenView.OnEnable(eventPack, AssetObjectEventPack.hiddenIDsField);
            multi_edit_instance = eventPack.FindProperty(AssetObjectEventPack.multi_edit_instance_field);
        

            //explorer_view.RealOnEnable(eventPack);
            //list_view.RealOnEnable(eventPack);
        }

        public void InitializeWithPack (AssetObjectPack pack) {
            this.pack = pack;
            if (pack == null) {
                current_pack_explorer_valid = false;
                pack_help_string = "Please Choose an Asset Object Pack";
                return;
            }
            current_pack_explorer_valid = pack.assetType.IsValidTypeString() && pack.objectsDirectory.IsValidDirectory();
            if (!current_pack_explorer_valid) {
                if (!pack.assetType.IsValidTypeString()) pack_help_string = pack.name + " pack doesnt have a valid asset type to target!";
                else if (!pack.objectsDirectory.IsValidDirectory()) pack_help_string = pack.name + " pack doesnt have a valid object directory!";
                return;
            }
            pack_help_string = "";
            
            UpdateListParametersToReflectPack(eventPack.FindProperty(AssetObjectEventPack.asset_objs_field), pack);
            
            //Dictionary<int, string> id2path;
            all_file_paths = GetAllFilePaths();//out id2path);
            
            InitializeIDsPrompt(pack);


            AssetObjectParamDef[] defs = pack.defaultParams;

            //clear conditionals as well
            multi_edit_instance.FindPropertyRelative(AssetObject.conditionChecksField).ClearArray();
            ReInitializeAssetObjectParameters(multi_edit_instance, defs);



            this.dummy_list_element = new DummyListElement(multi_edit_instance, defs);





            //explorer_view.InitializeWithPack(pack);
            //list_view.InitializeWithPack(pack, id2path);


            InitializeView();
            //initialize_views[selection_view](all_file_paths, selection);

            eventPack.ApplyModifiedProperties();
        }

        GUILayoutOption max_name_width;

        const int max_elements_per_page = 25;
        bool foldered;




        void RebuildAllElements() {
            elements.Clear();

            List<ListViewElement> unpaginated;
            if (foldered) unpaginated = UnpaginatedFoldered();// unpaginatedFoldereds[selection_view](all_file_paths);
            else unpaginated = UnpaginatedListed();// unpaginatedListeds[selection_view](all_file_paths);
            
            int min, max;
            paginateView.Paginate(unpaginated.Count, max_elements_per_page, out min, out max);
            elements.AddRange(unpaginated.Slice(min, max));
            
            int l = elements.Count;
            float max_width = 0;
            
            for (int i = 0; i < l; i++) {

                float w = EditorStyles.toolbarButton.CalcSize(elements[i].label_gui).x;
                if (w > max_width) max_width = w;
            
            }
                
            max_name_width = selection_view == 1 ? GUILayout.ExpandWidth(true) : GUILayout.Width( Mathf.Max(max_width, min_element_width ) );

            //OnPagination();
        }

        const float min_element_width = 256;


        void DrawElements () {

            GUIUtils.StartBox();

            for (int i = 0; i < elements.Count; i++) {

                ListViewElement element = elements[i];

                bool selected = selection.Contains(element);

                    
                if (element.object_id == -1) {
                    if (GUIUtils.ScrollWindowElement (element.label_gui, selected, false, true, GUILayout.ExpandWidth(true))) {
                        MoveForwardFolder(element.fullName);
                    }
                }
                else {



                    EditorGUILayout.BeginHorizontal();

            bool newShow = false;
            if (isListView) {
                SerializedProperty showProp = element.ao.FindPropertyRelative(AssetObject.showConditionsField);
                bool origShow = showProp.boolValue;
                newShow = GUIUtils.SmallToggleButton(showConditionsGUI, origShow);
                if (newShow != origShow) {
                    showProp.boolValue = newShow;
                }
            }


                        bool return_val = GUIUtils.ScrollWindowElement (element.label_gui, selected, hiddenView.IsHidden(element.object_id), false, max_name_width);

                        if (return_val) {

                                                    OnObjectSelection(element, selected);


                        }
                        if (isListView) {

                    
            DrawParameterFields(element.ao);
                        }

                    
            EditorGUILayout.EndHorizontal();

             if (isListView) {



            if (newShow) DrawConditions(element.ao);
             }

             /*

                    if (drawNonFolderElement[selection_view]( element, i, selected, hiddenView.IsHidden(element.object_id), max_name_width )) {

                    
                        OnObjectSelection(element, selected);

                    }
              */
                }       
            }
            GUIUtils.EndBox();

        }


        void AddElementsToSet (HashSet<ListViewElement> elements_to_add) {
            if (elements_to_add.Count == 0) return;
            //hiddenView.UnhideIDs( IDSetFromElements(elements_to_add) );
            bool reset_i = true;
            foreach (ListViewElement e in elements_to_add) {
                //ids_in_set.Add(e.object_id);
                AddNewAssetObject(e.object_id, GetObjectRefForElement(e), e.file_path, reset_i);
                reset_i = false;
            }
            ClearSelectionsAndRebuild();
        }
        void AddNewAssetObject (int obj_id, UnityEngine.Object obj_ref, string file_path, bool make_default) {
            SerializedProperty ao = ao_list.AddNewElement();
            ao.FindPropertyRelative(AssetObject.id_field).intValue = obj_id;
            ao.FindPropertyRelative(AssetObject.obj_ref_field).objectReferenceValue = obj_ref;
            if (!make_default) return;
            //only need to default first one added, the rest will copy the last one 'inserted' into the
            //serialized property array
            ao.FindPropertyRelative(AssetObject.conditionChecksField).ClearArray();
            ao.FindPropertyRelative(AssetObject.tags_field).ClearArray();
            ReInitializeAssetObjectParameters(ao, pack.defaultParams);
        } 
        



        

        SerializedObject eventPack;

        void UpdateListParametersToReflectPack (SerializedProperty ao_list, AssetObjectPack pack) {
            int c = ao_list.arraySize;
            AssetObjectParamDef[] defs = pack.defaultParams;
            for (int i = 0; i < c; i++) SerializedPropertyUtils.Parameters.UpdateParametersToReflectDefaults(ao_list.GetArrayElementAtIndex(i).FindPropertyRelative(AssetObject.params_field), defs);
        }
     

        public bool HasPreviewGUI() { return current_pack_explorer_valid; }

        public void OnInteractivePreviewGUI(Rect r, GUIStyle background) { 
            if (!current_pack_explorer_valid) return;
            //on_interactive_previews[selection_view](r, background); 
            if (preview != null) preview.OnInteractivePreviewGUI(r, background); 
        
        }
        public void OnPreviewSettings() { 
            if (!current_pack_explorer_valid) return;
            //on_preview_settings[selection_view] (); 
            if (preview != null) preview.OnPreviewSettings();

        }


        bool ids_prompt_dismissed;
        string ids_help_msg, sure_msg;

        void InitializeIDsPrompt (AssetObjectPack pack) {
            int l = no_ids.Length;
            if (l == 0) {
                ids_help_msg = sure_msg = "";
                return;
            }
            ids_help_msg = "\n"+ l + " [" + pack.fileExtensions + "] file(s) without proper IDs in the pack directory.\n\n\n" + pack.objectsDirectory + "\n";
            sure_msg = "Generating IDs will rename the following assets:\n";
            for (int i = 0; i < l; i++) sure_msg += "\n" + no_ids[i] + "\n";
        }




        const string sGenerateIDs = "Generate IDs", sCancel = "Cancel";
        GUIContent generate_ids_gui = new GUIContent(sGenerateIDs);
        GUIContent dismiss_gui = new GUIContent("Dismiss");
        Dictionary<int, string> id2path;
                    
            

        void DrawObjectsWithoutIDsPrompt (int buffer_space) {
            if (ids_prompt_dismissed) return;

            GUIStyle b_style = GUI.skin.button;
            
            GUIUtils.Space(buffer_space);
            EditorGUILayout.HelpBox(ids_help_msg, MessageType.Warning);
            
            EditorGUILayout.BeginHorizontal();
            if (GUIUtils.Button(generate_ids_gui, false, b_style, EditorColors.green_color, EditorColors.black_color)) {
                if (EditorUtility.DisplayDialog(sGenerateIDs, sure_msg, sGenerateIDs, sCancel)) {

                    AssetObjectsEditor.GenerateNewIDs(AssetObjectsEditor.GetAllAssetObjectPaths (pack.objectsDirectory, pack.fileExtensions, false), no_ids);
                    //Dictionary<int, string> id2path;
                    all_file_paths = GetAllFilePaths();//out id2path);
                    //explorer_view.ReinitializeAssetObjectReferences(all_file_paths);
                    //list_view.id2path = id2path;

                    InitializeView();
                    //initialize_views[selection_view](all_file_paths, selection);
                }
            }
            if (GUIUtils.Button(dismiss_gui, false, b_style)) ids_prompt_dismissed = true;
            EditorGUILayout.EndHorizontal();
            
            GUIUtils.Space(buffer_space);       
        }
                
        string[] GetAllFilePaths (){//out Dictionary<int, string> id2path){
            return AssetObjectsEditor.GetAllAssetObjectPaths (pack.objectsDirectory, pack.fileExtensions, false, out no_ids, out id2path);
        }

        bool current_pack_explorer_valid;
        string pack_help_string = "";


        void OnChangeView () {
            elements.Clear();
            selection.Clear();
            InitializeView();
            //initialize_views[selection_view](all_file_paths, selection);

        }

        
        
        
        public bool Draw (){
            if (!current_pack_explorer_valid) {
                EditorGUILayout.HelpBox(pack_help_string, MessageType.Error);
                return false;
            }
                    
            if (no_ids.Length != 0) DrawObjectsWithoutIDsPrompt(2);

            bool changed;
            selection_view = GUIUtils.Tabs(tab_guis, selection_view, out changed);
            if (changed) OnChangeView();










            changed = false;
            bool enter_pressed, delete_pressed;
            KeyboardInput(out enter_pressed, out delete_pressed);


            if (selection.Count != 0) {
                //list
                if (delete_pressed) {
                    OnDeleteIDsFromSet(IDSetFromElements(selection));
                    changed = true;
                }
                //explorer
                if (enter_pressed) {
                    AddElementsToSet(selection);
                    changed = true;
                }
            
            }

            DrawToolbar();

            if (selection_view == 0) {
                //list
                DrawMultipleEditWindow();
            }




            if (elements.Count == 0) {
                
                EditorGUILayout.HelpBox(selection_view == 1 ? no_elements_expl_help_string : no_elements_list_help_string, MessageType.Info);
            }
            else {
                DrawElements();
            }
            DrawPaginationGUI();
            return changed;
            

            //return draw_methods[selection_view](all_file_paths, selection);
        }   


        


        const string no_elements_list_help_string = "No elements, add some through the explorer view tab";
        const string no_elements_expl_help_string = "No elements in pack directory";
        


        void DrawPaginationGUI () {
            if (paginateView.ChangePageGUI ()) ClearSelectionsAndRebuild();
        }

        bool NextPage() {
            if (paginateView.NextPage()) {
                ClearSelectionsAndRebuild();   
                return true;
            }
            return false;
        }
        bool PreviousPage() {
            if (paginateView.PreviousPage()) {
                ClearSelectionsAndRebuild();
                return true;
            }
            return false;
                
        }

        void OpenImportSettings () {
            Animations.EditImportSettings.CreateWizard(new UnityEngine.Object[selection.Count].Generate(selection, e => { return AssetDatabase.LoadAssetAtPath(pack.objectsDirectory + e.file_path, typeof(UnityEngine.Object)); } ));
        }

        void ReInitializeAssetObjectParameters(SerializedProperty ao, AssetObjectParamDef[] defs) {
            SerializedProperty params_list = ao.FindPropertyRelative(AssetObject.params_field);
            params_list.ClearArray();
            int l = defs.Length;            
            for (int i = 0; i < l; i++) {                    
                SerializedPropertyUtils.Parameters.CopyParameter(params_list.AddNewElement(), defs[i].parameter);
            }
        }
        

        void KeyboardInput (out bool enter_pressed, out bool delete_pressed){
            
            
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
            
            ListViewElement element_to_select = null;

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
            if (element_to_select != null) OnObjectSelection(element_to_select, false);


            if (right_p) {
                if (NextPage()) {}
            }
            if (left_p) {
                if (PreviousPage()) {}
                else {
                    if (foldered) {
                        if (folderView.MoveBackward()) {
                            OnFolderViewChange();
                        }
                    }
                }
            }      

            if (selection.Count != 0) {
                
                if (enter_pressed) {
                    if (SelectionHasDirectories()) {
                        if (selection.Count == 1) {
                            foreach (var s in selection) MoveForwardFolder(s.fullName);
                        }
                        enter_pressed = false;
                    }
                }
                if (h_pressed) {
                    if (!SelectionHasDirectories()) {
                        if (hiddenView.ToggleHidden(IDSetFromElements( selection ), false, true)) {
                            ClearSelectionsAndRebuild();
                        }
                    }
                }
            } 

            
        }



        void DrawToolbar () {

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
            if (has_selection) { selection_has_directories = SelectionHasDirectories(); }
            if (foldered && folderView.DrawBackButton()) OnFolderViewChange();

            bool lastview = foldered;
            foldered = GUIUtils.ToggleButton(foldersViewGUI, true, foldered, toolbar_style);
            if (foldered != lastview) {
                //initialize_views[selection_view](all_file_paths, selection);
                InitializeView();
            }
            
            GUI.enabled = has_selection && !selection_has_directories;
            if (GUIUtils.Button(importSettingsGUI, true, toolbar_style)) OpenImportSettings();
            GUI.enabled = true;

            GUI.enabled = has_selection && !selection_has_directories;            
            
            bool toggled_hidden_selected = hiddenView.ToggleHiddenButton( IDSetFromElements( selection ) );
            
            GUI.enabled = true;
            
            bool toggled_show_hidden = hiddenView.ToggleShowHiddenButton();
            
            bool reset_hidden = hiddenView.ResetHiddenButton();
            
            if (toggled_hidden_selected || toggled_show_hidden || reset_hidden) ClearSelectionsAndRebuild();
        
            if (selection_view == 0) {
                GUI.enabled = has_selection && !selection_has_directories;            
                    
                if (GUIUtils.Button(remove_selected_gui, true, EditorStyles.miniButton)) OnDeleteIDsFromSet( IDSetFromElements(selection));
                
                
                GUI.enabled = true;
            
        
            }
            else {

                GUI.enabled = has_selection && !selection_has_directories;            
            
                if (GUIUtils.Button(add_selected_gui, true, EditorStyles.miniButton)) AddElementsToSet(selection);
                
                
                GUI.enabled = true;
            

            }



            //ExtraToolbarButtons(all_paths, has_selection, selection_has_directories, selection);

            EditorGUILayout.EndHorizontal();

            GUIUtils.Space();
            DrawSearchBar();
            GUIUtils.Space();

            GUIUtils.EndBox();
        }


         void DrawMultipleEditWindow () {
            //if (elements.Count == 0) return;
            
            
            GUIUtils.StartBox();

            EditorGUILayout.Space();

            //EditorGUILayout.BeginHorizontal();

            //search_keywords.DrawTagSearch();
            //GUILayout.FlexibleSpace();








            //EditorGUILayout.EndHorizontal();






            EditorGUILayout.Space();

            DrawObjectFieldsTop();

            bool editing_all_shown = selection.Count == 0;
            
            EditorGUILayout.BeginHorizontal();
            //if (GUIUtils.SmallButton(editing_all_shown ? remove_shown_gui : remove_selected_gui, EditorColors.red_color, EditorColors.white_color)) {
            //    OnDeleteIDsFromSet( editing_all_shown ? IDSetFromElements(elements) : IDSetFromElements(selected_elements), all_paths);
                //if (editing_all_shown) 
                //else OnDeleteIDsFromSet(IDsFromElements(selected_elements), all_paths);
            //}

            show_multi_conditions = GUIUtils.SmallToggleButton(showConditionsGUI, show_multi_conditions);
            
            GUIUtils.ScrollWindowElement (editing_all_shown ? editing_shown_gui : editing_selected_gui, false, false, false, max_name_width);
            
            
            DrawParameterFields(multi_edit_instance);
            //MultipleAnimEditFields();
            EditorGUILayout.EndHorizontal();

            if (show_multi_conditions) {
                DrawConditions(multi_edit_instance);
            }

            
            EditorGUILayout.Space();

            GUIUtils.EndBox();
        }


         void SetPropertyAll(int prop_index) {
            
            SerializedProperty multi_parameters = multi_edit_instance.FindPropertyRelative(AssetObject.params_field);
            SerializedProperty copy_prop = SerializedPropertyUtils.Parameters.GetParamProperty(multi_parameters.GetArrayElementAtIndex(prop_index));
            
            IEnumerable<ListViewElement> l = elements;
            if (selection.Count != 0) l = selection;

            foreach (ListViewElement el in l) {

                SerializedProperty parameters = el.ao.FindPropertyRelative(AssetObject.params_field);
                SerializedProperty prop = SerializedPropertyUtils.Parameters.GetParamProperty(parameters.GetArrayElementAtIndex(prop_index));
            
                prop.CopyProperty( copy_prop );
            
          
            

                //el.relevant_props[prop_index].CopyProperty(dummy_list_element.multi_edit_props[prop_index]);                      
            }
        }







        void DrawConditions (SerializedProperty ao) {
            SerializedProperty conditions = ao.FindPropertyRelative(AssetObject.conditionChecksField);

            GUIUtils.BeginIndent();
            GUIUtils.StartBox (EditorColors.dark_color);
            EditorGUILayout.BeginHorizontal();

            if (GUIUtils.Button(addConditionGUI, true, EditorStyles.miniButton)) {
                SerializedProperty newCondition = conditions.AddNewElement();
                newCondition.FindPropertyRelative(AssetObject.paramsToMatchField).ClearArray();
                DefaultifyConditionParam(newCondition.FindPropertyRelative(AssetObject.paramsToMatchField).AddNewElement());                    
            }
            
            EditorGUILayout.EndHorizontal();

            List<int> delete_indicies = new List<int>();
            int l = conditions.arraySize;
            for (int i = 0; i < l; i++) {
                bool delete;
                DrawConditional(conditions.GetArrayElementAtIndex(i), i, out delete);
                if(delete) delete_indicies.Add(i);
            }
            for (int i = delete_indicies.Count-1; i >= 0; i--) conditions.DeleteArrayElementAtIndex(delete_indicies[i]);

            EditorGUILayout.Space();

            GUIUtils.EndBox ();

            GUIUtils.EndIndent();            
        }


        void DefaultifyConditionParam (SerializedProperty conditionParam) {
            conditionParam.FindPropertyRelative(AssetObjectParam.name_field).stringValue = "Parameter Name";
        }


        void DrawConditional (SerializedProperty conditional, int i, out bool delete) {
            
            SerializedProperty conditionParameters = conditional.FindPropertyRelative(AssetObject.paramsToMatchField);

            GUIUtils.StartBox ();

            EditorGUILayout.BeginHorizontal();
            delete = GUIUtils.SmallButton(deleteConditionalGUI, EditorColors.red_color, EditorColors.white_color);

            if (GUIUtils.Button(addParameterToConditionalGUI, true, EditorStyles.miniButton)) {
                DefaultifyConditionParam(conditionParameters.AddNewElement());
            }
            
            EditorGUILayout.EndHorizontal();

            GUIUtils.BeginIndent();
                        
            List<int> delete_indicies = new List<int>();
            int l = conditionParameters.arraySize;
            for (int x = 0; x < l; x++) {
                bool d;
                DrawConditionParameter(conditionParameters.GetArrayElementAtIndex(x), out d);
                if(d) delete_indicies.Add(x);
            }
            for (int x = delete_indicies.Count-1; x >= 0; x--) conditionParameters.DeleteArrayElementAtIndex(delete_indicies[x]);
            
            GUIUtils.EndIndent();

            GUIUtils.EndBox();

        }

        GUIContent deleteConditionParameterGUI = new GUIContent("D", "Delete Parameter");


        void DrawConditionParameter(SerializedProperty parameter, out bool delete) {
            EditorGUILayout.BeginHorizontal();
        
            delete = GUIUtils.SmallButton(deleteConditionParameterGUI, EditorColors.red_color, EditorColors.white_color);
            
            
            
            // Set the internal name of the textfield
            GUI.SetNextControlName(searchControlName);
            EditorGUILayout.PropertyField(parameter.FindPropertyRelative(AssetObjectParam.name_field), GUIContent.none, condition_param_width );
            
            bool searchClicked;
            if (ClickHappened(out searchClicked)) {
                if (!searchClicked) EditorGUI.FocusTextInControl("");
            }

            
            
            
            EditorGUILayout.PropertyField(parameter.FindPropertyRelative(AssetObjectParam.param_type_field), GUIContent.none, condition_param_width ) ;
            EditorGUILayout.PropertyField(SerializedPropertyUtils.Parameters.GetParamProperty(parameter), GUIContent.none, condition_param_width );
            
            EditorGUILayout.EndHorizontal();
        }  

        protected int GetObjectIDAtIndex(int index) {
            return ao_list.GetArrayElementAtIndex(index).FindPropertyRelative(AssetObject.id_field).intValue;
        }
        

        void OnDeleteIDsFromSet (HashSet<int> ids) {
            for (int i = ao_list.arraySize - 1; i >= 0; i--) {
                if (ids.Contains(GetObjectIDAtIndex(i))) ao_list.DeleteArrayElementAtIndex(i);
            }
            ClearSelectionsAndRebuild();
        }


        void DrawListInstancePropertyLabels () {
            for (int i = 0; i < dummy_list_element.props_count; i++) {
                EditorGUILayout.LabelField(dummy_list_element.labels[i], dummy_list_element.widths[i]);
                if (GUIUtils.SmallButton(set_values_gui, EditorColors.selected_color, EditorColors.selected_text_color)) {
                    SetPropertyAll(i);
                }
                
            }
        }

        

        
        void DrawObjectFieldsTop () {
            EditorGUILayout.BeginHorizontal();
            GUIUtils.SmallButtonClear();
            
            GUIUtils.ScrollWindowElement (GUIContent.none, false, false, false, max_name_width);
            DrawListInstancePropertyLabels();
            EditorGUILayout.EndHorizontal();
            
            //GUILayout.Button(" mass set condition checks here");

        }

        DummyListElement dummy_list_element;
        static readonly GUIContent set_values_gui = new GUIContent("S","Set Values");
        static readonly GUIContent editing_shown_gui = new GUIContent("Editing Shown");
        static readonly GUIContent editing_selected_gui = new GUIContent("Editing Selected");
        bool show_multi_conditions;

        bool[] show_conditions;
        
        GUIContent addParameterToConditionalGUI = new GUIContent("Add Parameter +");
        GUIContent deleteConditionalGUI = new GUIContent("D", "Delete Condition");
        GUIContent addConditionGUI = new GUIContent("Add Condition +");
        
        GUILayoutOption condition_param_width = GUILayout.Width(128);
        
        SerializedProperty multi_edit_instance;
        //public Dictionary<int, string> id2path;



        public class DummyListElement {
            public GUIContent[] labels;
            public GUILayoutOption[] widths;
            //public SerializedProperty[] multi_edit_props;
            public int props_count;
            public DummyListElement (SerializedProperty multi_edit_instance, AssetObjectParamDef[] default_params){
                props_count = default_params.Length;


                labels = new GUIContent[props_count];
                widths = new GUILayoutOption[props_count];
                for (int i = 0; i < props_count; i++) {
                    labels[i] = new GUIContent(default_params[i].parameter.name, default_params[i].hint);
                    widths[i] = GUILayout.Width(EditorStyles.label.CalcSize(labels[i]).x);
                }

                //multi_edit_props = multi_edit_instance.GetRelevantParamsFromAssetObjectInstance();
            }
        }

        protected HashSet<int> IDSetFromElements (IEnumerable<ListViewElement> elements) {
            return new HashSet<int>().Generate(elements, o => { return o.object_id; } );
        }









        
        void DrawParameterFields (SerializedProperty ao) {
            int l = pack.defaultParams.Length;

            SerializedProperty parameters = ao.FindPropertyRelative(AssetObject.params_field);
            //return new SerializedProperty[parameters.arraySize].Generate( i => { return SerializedPropertyUtils.Parameters.GetParamProperty(parameters.GetArrayElementAtIndex(i)); } );
        

            for (int i = 0; i < l; i++) {

                EditorGUILayout.PropertyField(
                    SerializedPropertyUtils.Parameters.GetParamProperty(parameters.GetArrayElementAtIndex(i)), 
                    GUIUtils.blank_content, 
                    dummy_list_element.widths[i]
                );

                GUIUtils.SmallButtonClear();


                
            }


        }




        bool isListView { get { return selection_view == 0; } }
        

/*

        //LISTVIEW
        bool DrawNonFolderElement(ListViewElement element, int index, bool selected, bool hidden, GUILayoutOption element_width) {
            EditorGUILayout.BeginHorizontal();

            bool newShow = false;
            if (isListView) {
                SerializedProperty showProp = element.ao.FindPropertyRelative(AssetObject.showConditionsField);
                bool origShow = showProp.boolValue;
                newShow = GUIUtils.SmallToggleButton(showConditionsGUI, origShow);
                if (newShow != origShow) {
                    showProp.boolValue = newShow;
                }
            }


                        bool return_val = GUIUtils.ScrollWindowElement (element.label_gui, selected, hidden, false, element_width);
                        if (isListView) {

                    
            DrawParameterFields(element.ao);
                        }

                    
            EditorGUILayout.EndHorizontal();

             if (isListView) {



            if (newShow) DrawConditions(element.ao);
             }

             return return_val;
        

        }


 */




        GUIContent add_selected_gui = new GUIContent("   Add   ", "Add Selected");   
        static readonly GUIContent remove_selected_gui = new GUIContent("Remove", "Remove Selected");
        
        
        void InitializeView () {
            paginateView.ResetPage();
            ClearSelectionsAndRebuild();
        }


        void DrawSearchBar () {
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

            if (search_string != last_search) ClearSelectionsAndRebuild();
            
            EditorGUILayout.EndHorizontal();
        }



        void ClearSelections(){
            selection.Clear();
            OnSelectionChange();
        }
        void ClearSelectionsAndRebuild() {
            ClearSelections();
            RebuildAllElements();
        }



        void OnFolderViewChange () {
            paginateView.ResetPage();
            ClearSelectionsAndRebuild();
        }

        void MoveForwardFolder (string addPath) {
            folderView.MoveForward(addPath);
            OnFolderViewChange();
        }

        FoldersView folderView = new FoldersView();
        PaginateView paginateView = new PaginateView();
        HiddenView hiddenView = new HiddenView();


        GUILayoutOption search_options = GUILayout.MaxWidth(345);

        bool ClickHappened (out bool lastElementClicked) {
            Event e = Event.current;
            bool click_happened = e.type == EventType.MouseDown && e.button == 0;            
            lastElementClicked = false;
            if (click_happened) lastElementClicked = GUILayoutUtility.GetLastRect().Contains(e.mousePosition);
            return click_happened;
        }


        protected bool ElementPassedListedView (int id, string file_path, out GUIContent gui) {
            gui = null;


            bool is_hidden = hiddenView.IsHidden(id);
            if (!hiddenView.showHidden && is_hidden) return false;

            if (!search_string.IsEmpty() && !file_path.ToLower().Contains(search_string.ToLower())) return false;
            
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



        List<ListViewElement> UnpaginatedListed() {

            HashSet<int> ids_in_set = null;
            if (!isListView) ids_in_set = new HashSet<int>().Generate(ao_list.arraySize, i => { return GetObjectIDAtIndex(i); }); 
                        
            int l = isListView ? ao_list.arraySize : all_file_paths.Length;
            
            List<ListViewElement> unpaginated = new List<ListViewElement>();
            for (int i = 0; i < l; i++) {


                int id;
                string filePath;
                SerializedProperty ao;
                GetListElementInfo (i, out id, out filePath, out ao);

                if (!isListView && ids_in_set.Contains(id)) continue;


                GUIContent gui;
                if (ElementPassedListedView(id, filePath, out gui)) {

                    unpaginated.Add(new ListViewElement(filePath, filePath, gui, id, ao));
                }
            }
            return unpaginated;
        }    


        void GetListElementInfo (int i, out int id, out string filePath, out SerializedProperty ao) {
            if (isListView) {
                ao = ao_list.GetArrayElementAtIndex(i);
                id = ao.FindPropertyRelative(AssetObject.id_field).intValue;
                filePath = id2path[id];
            }
            else {
                ao = null;
                filePath = all_file_paths[i];
                id = AssetObjectsEditor.GetObjectIDFromPath(filePath);
            }
        }

        
        List<ListViewElement> UnpaginatedFoldered() {
            HashSet<string> usedNames = new HashSet<string>();            
            List<ListViewElement> unpaginated = new List<ListViewElement>();
            int lastDir = 0;

            HashSet<int> ids_in_set = null;
            if (!isListView) ids_in_set = new HashSet<int>().Generate(ao_list.arraySize, i => { return GetObjectIDAtIndex(i); }); 
            
            int l = isListView ? ao_list.arraySize : all_file_paths.Length;
            
            
            for (int i = 0; i < l; i++) {


                int id;
                string filePath;
                SerializedProperty ao;
                GetListElementInfo (i, out id, out filePath, out ao);

                if (!isListView && ids_in_set.Contains(id)) continue;


                GUIContent gui;
                bool isDirectory;
                if (ElementPassedFolderedView (id, filePath, ref usedNames, out gui, out isDirectory)) {
                    if (!isDirectory) { 
                        unpaginated.Add(new ListViewElement(filePath, gui.text, gui, id, ao));
                        continue;
                    }
                    unpaginated.Insert(lastDir, new ListViewElement(filePath, gui.text, gui, -1, null));
                    lastDir++;   
                }            
            }
            return unpaginated;
        }

        

        











    }
}