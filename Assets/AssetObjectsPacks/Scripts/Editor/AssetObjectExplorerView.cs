

using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
namespace AssetObjectsPacks {
    public class ExplorerWindowElement : ListViewElement {
        public bool is_directory { get { return object_id == -1; } }
        public ExplorerWindowElement(string full_name, GUIContent label_gui) : this(full_name, label_gui, -1){}
        public ExplorerWindowElement(string full_name, GUIContent label_gui, int object_id)
        => (this.full_name, this.label_gui, this.object_id) = (full_name, label_gui, object_id);
    }
    public class AssetObjectExplorerView<T> : SelectionView<T, ExplorerWindowElement> where T : Object
    {
        ExplorerWindowElement[] all_asset_objects;
        GUIContent current_path_gui;
        GUILayoutOption current_path_width;
        string current_path = "";
        bool show_hidden;
        int window_offset;
        SerializedProperty hidden_ids_list;
        string[] all_file_paths;

        bool gui_initialized;
       
        GUILayoutOption back_gui_width;
        GUILayoutOption[] explorer_options_widths;
        static readonly GUIContent[] explorer_options_guis = new GUIContent[] {
            new GUIContent("Add All"),
            new GUIContent("Add Selected"),
            new GUIContent("Show Hidden"),
            new GUIContent("Reset Hidden"),
        };
        static readonly GUIContent back_gui = new GUIContent("<< ");
        static readonly GUIContent add_to_cue_gui = new GUIContent("", "Add To Set");
        static readonly GUIContent hide_gui = new GUIContent("", "Hide");
        static readonly GUIContent unhide_gui = new GUIContent("", "Unhide");
        const string hidden_suffix = " [HIDDEN]";
        
       
        public void ReinitializePaths (string[] all_paths, Dictionary<int, string> id2path) {
            base.ReinitializePaths(id2path);
            this.all_file_paths = all_paths;
            BuildAssetObjectReferences();
        }

        public void OnEnable (SerializedObject serializedObject, string pack_name, Dictionary<int, string> id2path, System.Action<SerializedProperty> make_instance_default, string[] all_file_paths) {
            base.OnEnable(serializedObject, pack_name, id2path, make_instance_default);
            this.all_file_paths = all_file_paths;
            hidden_ids_list = serializedObject.FindProperty("hidden_ids");
            BuildAssetObjectReferences();
        }
        void OnExplorerPathChange() {
            ClearSelections();
            RebuildAllElements();
            current_path_gui = new GUIContent(current_path);
            current_path_width = GUILayout.Width(EditorStyles.label.CalcSize(current_path_gui).x);
        }
        
        public override void InitializeView() {
            OnExplorerPathChange();
        }

        protected override void OnAddIDsToSet(List<int> add_ids) {
            for (int i = 0; i < add_ids.Count; i++) {
                int id = add_ids[i];
                hidden_ids_list.Remove(id);
            }
            base.OnAddIDsToSet(add_ids);
        }
        void AddDirectoryContentsToSet(string dir) {
            List<int> ids_in_dir = new List<int>();
            for (int i = 0; i < all_asset_objects.Length; i++) {
                
                string full_path = all_asset_objects[i].full_name;
                if (!full_path.StartsWith(dir))
                    continue;
                
                int object_id = all_asset_objects[i].object_id;
                if (ids_in_set.Contains(object_id)) 
                    continue;
                
                if (hidden_ids_list.Contains(object_id) && !show_hidden) 
                    continue;
                
                ids_in_dir.Add(object_id);
                
            }
            OnAddIDsToSet(ids_in_dir);
        }
        void AddObjectIDToSet (int object_id) {
            OnAddIDsToSet(new List<int>() {object_id});
        }
         void AddSelectionsToSet() {
            if (selected_ids.Count == 0) 
                return;

            Debug.Log("Adding");
            OnAddIDsToSet(selected_ids);
        }
                
        void InitializeExplorerOptionsSizes() {
            back_gui_width = GUILayout.Width(EditorStyles.label.CalcSize(back_gui).x);
            int l = explorer_options_guis.Length;
            explorer_options_widths = new GUILayoutOption[l];
            for (int i = 0; i < l; i++) {
                explorer_options_widths[i] = GUILayout.Width(EditorStyles.label.CalcSize(explorer_options_guis[i]).x);
            }
        }
        void InitializeGUIStuff () {
            if (gui_initialized) return;
            gui_initialized = true;
            InitializeExplorerOptionsSizes();
        }
        public void Draw (int scroll_view_height) {
            InitializeGUIStuff();
            EditorGUILayout.BeginVertical(GUI.skin.box);
            DrawExplorerOptions();
            EditorGUILayout.Space();
            scroll_pos = EditorGUILayout.BeginScrollView(scroll_pos, GUILayout.Height(scroll_view_height));
            DrawExplorerElements();
            EditorGUILayout.EndScrollView();           
            EditorGUILayout.EndVertical();
        }
        void DrawExplorerElements() {
            for (int i = 0; i < all_elements.Count; i++) {
                int object_id = all_elements[i].object_id;
                if (object_id == -1) {
                    if (GUIUtils.ScrollWindowElement (all_elements[i].label_gui, false, false, true)) 
                        ExplorerForward(all_elements[i].full_name);                    
                }
                else {
                    bool is_hidden = hidden_ids_list.Contains(object_id);//window_elements[i].is_hidden;//IsHidden(index_in_all);
                    bool drawing_selected = selected_ids.Contains(object_id);
                    
                    if (is_hidden && !show_hidden) {
                        if (drawing_selected) {
                            selected_ids.Remove(object_id);
                            OnSelectionChange();
                        }
                        continue;
                    }
                    EditorGUILayout.BeginHorizontal();
                    
                    bool little_button_pressed_0 = GUIUtils.LittleButton(EditorColors.green_color, add_to_cue_gui);
                    if (GUIUtils.LittleButton(is_hidden ? EditorColors.selected_color : EditorColors.white_color, is_hidden ? unhide_gui : hide_gui)) {
                        if (is_hidden) 
                            hidden_ids_list.Remove(object_id);
                        else 
                            hidden_ids_list.Add(object_id);
                    }
                    
                    if (GUIUtils.ScrollWindowElement (new GUIContent(all_elements[i].label_gui.text + (is_hidden ? hidden_suffix : StringUtils.empty)), drawing_selected, is_hidden, false)) 
                        OnObjectSelection(object_id, drawing_selected);
                    
                    if (little_button_pressed_0) 
                        AddObjectIDToSet (object_id);
                                                
                    EditorGUILayout.EndHorizontal();
                }       
            }
        }
        
        void ExplorerForward(string dir_name) {
            window_offset += 1;
            current_path += dir_name + "/";
            OnExplorerPathChange();
        }
        void ExplorerBack() {
            window_offset -= 1;
            current_path = current_path.Substring(0, current_path.Substring(0, current_path.LastIndexOf("/") - 1).LastIndexOf("/") + 1);
            OnExplorerPathChange();
        }

        void DrawExplorerOptions () {
            EditorGUILayout.BeginHorizontal();
                
            if (window_offset > 0) {
                if (GUILayout.Button(back_gui, EditorStyles.toolbarButton, back_gui_width)){
                    ExplorerBack();
                }
            }
  
            EditorGUILayout.LabelField(current_path_gui, current_path_width);

            GUILayout.FlexibleSpace();

            int l = explorer_options_guis.Length;
            int pressed_index = -1;
            for (int i = 0; i < l; i++) {
                GUIStyle style; 
                if (i == 0) 
                    style = EditorStyles.miniButtonLeft;
                else if (i == l - 1) 
                    style = EditorStyles.miniButtonRight;
                else 
                    style = EditorStyles.miniButtonMid;

                Color32 orig_bg = GUI.backgroundColor;
                if (i == 2 && show_hidden) 
                    GUI.backgroundColor = EditorColors.selected_color;

                if (GUILayout.Button(explorer_options_guis[i], style, explorer_options_widths[i])) 
                    pressed_index = i;

                if (i == 2 && show_hidden) 
                    GUI.backgroundColor = orig_bg;
            }
            switch(pressed_index) {
                case -1: break;
                case 0: //add all directory
                    AddDirectoryContentsToSet(current_path);
                    break;
                case 1: //add selected
                    AddSelectionsToSet();
                    break;    
                case 2: //show hidden
                    show_hidden = !show_hidden;
                    RebuildAllElements();
                    break;
                case 3: //reset hidden
                    hidden_ids_list.ClearArray();
                    RebuildAllElements();
                    break;       
            }
            EditorGUILayout.EndHorizontal();
        }
        void BuildAssetObjectReferences () {
            int c = all_file_paths.Length;
            all_asset_objects = new ExplorerWindowElement[c];
            for (int i = 0; i < c; i++) {
                string file_path = all_file_paths[i];
                int id = AssetObjectsEditor.GetObjectIDFromPath(file_path);
                string n = AssetObjectsEditor.RemoveIDFromPath(EditorUtils.DirectoryNameSplit(file_path)[1]);
                all_asset_objects[i] = new ExplorerWindowElement(file_path, new GUIContent(n), id);
            }
        }

        protected override void RebuildAllElements() {

            all_elements.Clear();
            List<string> used_window_names = new List<string>();            
            int last_dir_index = 0;
            int c = all_asset_objects.Length;
            for (int i = 0; i < c; i++) {
                ExplorerWindowElement ao = all_asset_objects[i];
                string path = ao.full_name;
                int id = ao.object_id;
                //string disp_name = ao.label_gui.text;

                if (ids_in_set.Contains(ao.object_id)) continue;
                if (!ao.full_name.StartsWith(current_path)) continue;

                string name_display = ao.full_name.Split('/')[window_offset];
                if (used_window_names.Contains(name_display)) continue;
                used_window_names.Add(name_display);
                
                bool is_directory = !name_display.Contains(".");
                if (is_directory) {
                    all_elements.Insert(last_dir_index, new ExplorerWindowElement(name_display, new GUIContent(name_display + " >")));
                    last_dir_index++;
                }
                else {
                    all_elements.Add(new ExplorerWindowElement(name_display, ao.label_gui, id));
                } 
            } 
        }   
    }
}







