using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
namespace AssetObjectsPacks {
    

    public class ExplorerWindowElement : ListViewElement {
        public ExplorerWindowElement(string file_path, string full_name, GUIContent label_gui) : this(file_path, full_name, label_gui, -1){}
        public ExplorerWindowElement(string file_path, string full_name, GUIContent label_gui, int object_id) => (this.file_path, this.full_name, this.label_gui, this.object_id) = (file_path, full_name, label_gui, object_id);
    }
    public class AssetObjectExplorerView : SelectionView<ExplorerWindowElement>
    {
        const int max_elements_per_page = 25;
        ExplorerWindowElement[] all_asset_objects;
        string current_path = "";
        bool show_hidden, gui_initialized;
        int window_offset;
        SerializedProperty hidden_ids_prop;
        public string[] all_file_paths;
        static readonly GUIContent back_gui = new GUIContent("<< ");
        static readonly GUIContent add_to_cue_gui = new GUIContent("", "Add To Set");
        static readonly GUIContent hide_gui = new GUIContent("", "Hide");
        static readonly GUIContent unhide_gui = new GUIContent("", "Unhide");
        const string hidden_suffix = " [HIDDEN]";

        public void OnEnable (SerializedObject eventPack, AssetObjectPack pack, string[] all_file_paths) {
            base.OnEnable(eventPack, pack);
            hidden_ids_prop = eventPack.FindProperty(AssetObjectEventPack.hidden_ids_field);

            Debug.Log(hidden_ids_prop);
            InitializeHiddenIDs();
            ReinitializePaths(all_file_paths);
        }
        public void OnDisable () {
            SaveHiddenIDs();
        }
        public void ReinitializePaths (string[] all_paths){
            all_file_paths = all_paths;
            BuildAssetObjectReferences();
        }
        protected override void OnAddIDsToSet(HashSet<ExplorerWindowElement> add_ids) {
            UnhideIDs(add_ids);
            base.OnAddIDsToSet(add_ids);
        }
        void AddSelectionsToSet() {
            if (selected_elements.Count == 0) return;                    
            OnAddIDsToSet(selected_elements);
        }

        
                    
        public bool Draw () {
            bool changed = false;
            bool enter_pressed, delete_pressed, right_p, left_p;
            KeyboardInput(out enter_pressed, out delete_pressed, out left_p, out right_p);

            if (right_p) {
                if (NextPage(max_elements_per_page)) {}
            }
            if (left_p) {
                if (PreviousPage()) {}
                else {
                    if (window_offset != 0) {
                        MoveExplorer(current_path.Substring(0, current_path.Substring(0, current_path.LastIndexOf("/") - 1).LastIndexOf("/") + 1), -1);
                    }
                }
            }            
            if (selected_elements.Count != 0) {
                if (delete_pressed) {
                    if (!SelectionHasDirectories()) {
                        ToggleHiddenSelected(selected_elements, false);
                        changed = true;
                    }
                }
                if (enter_pressed) {
                    if (SelectionHasDirectories()) {
                        if (selected_elements.Count == 1) {

                            foreach (var s in selected_elements) {
                                MoveExplorer(current_path + s.full_name + "/", 1);
                                break;
                            }
                        }

                    }
                    else {

                        AddSelectionsToSet();
                    }
                    changed = true;
                }
            }
            
            EditorGUILayout.BeginVertical(GUI.skin.box);
            DrawExplorerOptions();
            EditorGUILayout.Space();
            DrawExplorerElements();

            EditorGUILayout.EndVertical();

            DrawPaginationGUI(max_elements_per_page);

            return changed;
        }

        bool ElementsAreAlltheSameHiddenStatus(HashSet<ExplorerWindowElement> elements, out bool hidden_status) {
            hidden_status = false;
            if (elements.Count == 0 || hidden_ids.Count == 0){
                return true;
            }

            bool was_checked = false;
            foreach (var e in elements) {
                bool is_hidden = hidden_ids.Contains(e.object_id);
                if (!was_checked) {
                    hidden_status = is_hidden;
                    was_checked = true;
                }
                else {
                    if (is_hidden != hidden_status) return false;
                }
            }
            return true;
        }

        void HideIDs(HashSet<ExplorerWindowElement> elements) {
            foreach (var e in elements) hidden_ids.Add(e.object_id);
        }
        void UnhideIDs(HashSet<ExplorerWindowElement> elements) {    
            foreach (var e in elements) hidden_ids.Remove(e.object_id);
        }

        void DrawExplorerElements() {

            for (int i = 0; i < elements.Count; i++) {

                ExplorerWindowElement element = elements[i];
                bool drawing_selected = selected_elements.Contains(element);
                int object_id = elements[i].object_id;
                    
                if (object_id == -1) {
                    if (GUIUtils.ScrollWindowElement (elements[i].label_gui, drawing_selected, false, true)) MoveExplorer(current_path + elements[i].full_name + "/", 1);
                }
                else {
                    bool is_hidden = false; 
                    if (show_hidden) is_hidden = hidden_ids.Contains(object_id);

                    EditorGUILayout.BeginHorizontal();
                    
                    bool little_button_pressed_0 = GUIUtils.LittleButton(EditorColors.green_color, add_to_cue_gui);
                    if (GUIUtils.LittleButton(is_hidden ? EditorColors.selected_color : EditorColors.white_color, is_hidden ? unhide_gui : hide_gui)) ToggleHiddenSelected(new HashSet<ExplorerWindowElement>() {element}, is_hidden, false);
                    
                    if (GUIUtils.ScrollWindowElement (new GUIContent(elements[i].label_gui.text + (is_hidden ? hidden_suffix : StringUtils.empty)), drawing_selected, is_hidden, false)) OnObjectSelection(element, drawing_selected);
                    
                    if (little_button_pressed_0) OnAddIDsToSet(new HashSet<ExplorerWindowElement>() {element});
                        
                    EditorGUILayout.EndHorizontal();
                }       
            }
        }

        void MoveExplorer(string new_cur_path, int window_offset_add) {
            pagination.cur_page = 0;
            window_offset += window_offset_add;
            current_path = new_cur_path;
            ClearSelectionsAndRebuild();
        }
        
        void ToggleHiddenSelected (HashSet<ExplorerWindowElement> elements, bool hidden_status, bool check_hidden_status = true) {
            bool contin = true;
            if (check_hidden_status) contin = ElementsAreAlltheSameHiddenStatus(elements, out hidden_status);
            if (!contin) return;

            if (hidden_status) UnhideIDs(elements);
            else HideIDs(elements);

            if (!show_hidden) ClearSelectionsAndRebuild();
        }

        void DrawExplorerOptions () {
            EditorGUILayout.BeginHorizontal();
            if (window_offset > 0) {
                if (GUILayout.Button(back_gui, EditorStyles.toolbarButton, back_gui.CalcWidth())) {
                    MoveExplorer(current_path.Substring(0, current_path.Substring(0, current_path.LastIndexOf("/") - 1).LastIndexOf("/") + 1), -1);
                }
                GUIContent current_path_gui = new GUIContent(current_path);
                EditorGUILayout.LabelField(current_path_gui, current_path_gui.CalcWidth());
            }

            GUILayout.FlexibleSpace();
            GUIStyle s = EditorStyles.miniButton;
            if (selected_elements.Count != 0) {

                if (!SelectionHasDirectories()) {
                    if (GUILayout.Button(add_selected_gui, s, add_selected_gui.CalcWidth())) {
                        AddSelectionsToSet();
                    }
                    bool hidden_status;
                    if (ElementsAreAlltheSameHiddenStatus(selected_elements, out hidden_status)) {
                        GUIContent hide_content = new GUIContent( hidden_status ? "Unhide Selected" : "Hide Selected");
                        if (GUILayout.Button(hide_content,  EditorStyles.miniButtonMid, hide_content.CalcWidth())){
                            ToggleHiddenSelected(selected_elements, hidden_status, false);
                        }
                    }
                }

            }

            if (hidden_ids.Count != 0) {
                Color32 orig_bg = GUI.backgroundColor;
                if (show_hidden) GUI.backgroundColor = EditorColors.selected_color;
                if (GUILayout.Button(show_hidden_gui, s, show_hidden_gui.CalcWidth())){
                    show_hidden = !show_hidden;
                    ClearSelectionsAndRebuild();
                }
                GUI.backgroundColor = orig_bg;
                
                if (GUILayout.Button(reset_hidden_gui, s, reset_hidden_gui.CalcWidth())){
                    hidden_ids.Clear();
                    ClearSelectionsAndRebuild();                
                }
            }
            
            EditorGUILayout.EndHorizontal();
        }
        GUIContent add_selected_gui = new GUIContent("Add Selected");
        GUIContent show_hidden_gui = new GUIContent("Show Hidden");
        GUIContent reset_hidden_gui = new GUIContent("Reset Hidden");
        
        void BuildAssetObjectReferences () {

            int c = all_file_paths.Length;
            all_asset_objects = new ExplorerWindowElement[c];
            
            for (int i = 0; i < c; i++) {
                string file_path = all_file_paths[i];
                int id = AssetObjectsEditor.GetObjectIDFromPath(file_path);
                string n = AssetObjectsEditor.RemoveIDFromPath(EditorUtils.RemoveDirectory(file_path));
                all_asset_objects[i] = new ExplorerWindowElement(file_path, file_path, new GUIContent(n), id);
            }
        }

        HashSet<int> hidden_ids = new HashSet<int>();
        void InitializeHiddenIDs() {
            int c = hidden_ids_prop.arraySize;
            for (int i = 0; i < c; i++) hidden_ids.Add(hidden_ids_prop.GetArrayElementAtIndex(i).intValue);
        }
        void SaveHiddenIDs () {
            hidden_ids_prop.ClearArray();
            foreach (int id in hidden_ids) hidden_ids_prop.AddNewElement().intValue = id;
        }

        protected override void RebuildAllElements() {

            elements.Clear();
            HashSet<string> used_dir_names = new HashSet<string>();            
            int last_dir_index = 0;
            int c = all_asset_objects.Length;

            EditorUtils.StartTimer();

            List<ExplorerWindowElement> first_l = new List<ExplorerWindowElement>();
            
            for (int i = 0; i < c; i++) {
            
                ExplorerWindowElement ao = all_asset_objects[i];

                if (!current_path.IsEmpty()) {
                    if (!ao.full_name.StartsWith(current_path)) continue;
                }

                if (ids_in_set.Contains(ao.object_id)) continue;

                if (!show_hidden) {
                    if (hidden_ids.Contains(ao.object_id)) continue;
                }
                
                string name_display = ao.full_name;
                bool has_directory = name_display.Contains("/");
                if (has_directory) {
                    name_display = ao.full_name.Split('/')[window_offset];
                    if (used_dir_names.Contains(name_display)) continue;
                    used_dir_names.Add(name_display);
                }                
                bool is_directory = !name_display.Contains(".");
                if (is_directory) {
                    first_l.Insert(last_dir_index, new ExplorerWindowElement(ao.file_path, name_display, new GUIContent(name_display)));
                    last_dir_index++;
                }
                else {
                    first_l.Add(new ExplorerWindowElement(ao.file_path, name_display, ao.label_gui, ao.object_id));
                } 
            } 
            EditorUtils.PrintTimer("end build");

            PaginateElements(first_l, max_elements_per_page);

        }

        
    }
}







