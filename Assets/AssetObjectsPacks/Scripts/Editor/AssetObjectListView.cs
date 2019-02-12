using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
namespace AssetObjectsPacks {
    public static class PropertyExtensions {
        public static SerializedProperty[] GetRelevantParamsFromAssetObjectInstance (this SerializedProperty ao) {
            SerializedProperty params_l = ao.FindPropertyRelative(AssetObject.params_field);
            return new SerializedProperty[0].GenerateArray( i => { return params_l.GetArrayElementAtIndex(i).GetRelevantParamProperty(); } , params_l.arraySize );
        }
    }

    public class ListElement : ListViewElement {
        public SerializedProperty[] relevant_props;
        public SerializedProperty tags_prop;
        public ListElement (string file_name, string full_name, GUIContent label_gui, int object_id, SerializedProperty instance, SerializedProperty tags_prop) {
            Initialize(file_name, full_name, label_gui, object_id);    
            this.tags_prop = tags_prop;
            relevant_props = instance.GetRelevantParamsFromAssetObjectInstance();
        }
    }

    public class AssetObjectListView : SelectionView<ListElement>
    {
        protected override void OnSelectionChange() {
            base.OnSelectionChange();
            tags_gui.selection_changed = true;
            RebuildSelectedTagsProps();
        }
        void RebuildSelectedTagsProps () {
            int s = selected_elements.Count;
            selected_tags_props = new SerializedProperty[s];
            
            if (s == 0) return;
            int u = 0;

            foreach (var e in selected_elements) {
                selected_tags_props[u] = e.tags_prop;
                u++;
            }

        }
       
        public override void OnInteractivePreviewGUI(Rect r, GUIStyle background) {
            base.OnInteractivePreviewGUI(r, background);
            if (selected_tags_props.Length != 0) tags_gui.OnInteractivePreviewGUI(selected_tags_props, all_tags);
        }
        public class DummyListElement {
            public GUIContent[] labels;
            public GUILayoutOption[] widths;
            public SerializedProperty[] multi_edit_props;
            public int props_count;
            public DummyListElement (SerializedProperty multi_edit_instance, AssetObjectParamDef[] default_params){
                props_count = default_params.Length;

                labels = new GUIContent[props_count];
                widths = new GUILayoutOption[props_count];

                for (int i = 0; i < default_params.Length; i++) {
                    labels[i] = new GUIContent(default_params[i].parameter.name, default_params[i].hint);
                    widths[i] = GUILayout.Width(EditorStyles.label.CalcSize(labels[i]).x);
                }

                multi_edit_props = multi_edit_instance.GetRelevantParamsFromAssetObjectInstance();
            }
        }

        DummyListElement dummy_list_element;
        static readonly GUIContent remove_gui = new GUIContent("", "Remove");
        static readonly GUIContent set_values_gui = new GUIContent("","Set Values");
        static readonly GUIContent remove_shown_gui = new GUIContent("", "Remove Shown");
        static readonly GUIContent remove_selected_gui = new GUIContent("", "Remove Selected");
        static readonly GUIContent editing_shown_gui = new GUIContent("Editing Shown");
        static readonly GUIContent editing_selected_gui = new GUIContent("Editing Selected");
        
        GUILayoutOption max_name_width;

        const int max_elements_per_page = 25;


        
        bool HasSearchTags (SerializedProperty tags_prop, int keywords_count) {
            for (int i = 0; i < keywords_count; i++) {
                if (tags_prop.Contains(search_keywords.keywords[i])) {
                    return true;
                }
            }
            return false;
        }
        
        protected override void RebuildAllElements() {
            elements.Clear();
            
            int l = ao_list.arraySize;
            
            float max_width = 0;

            int keywords_count = search_keywords.keywords.Count;

            List<ListElement> first_l = new List<ListElement>();
            for (int i = 0; i < l; i++) {
                SerializedProperty ao = ao_list.GetArrayElementAtIndex(i);
                SerializedProperty tags_prop = ao.FindPropertyRelative(AssetObject.tags_field);
                
                if (keywords_count != 0 && !HasSearchTags(tags_prop, keywords_count)) continue;
                

                int object_id = ao.FindPropertyRelative(AssetObject.id_field).intValue;
                string file_path = id2path[object_id];// ao.FindPropertyRelative(AssetObject.obj_file_path_field).stringValue;
                                
                string n = AssetObjectsEditor.RemoveIDFromPath(file_path);
                GUIContent gui_n = new GUIContent(n);


                float w = EditorStyles.toolbarButton.CalcSize(gui_n).x;
                if (w > max_width) max_width = w;
                
                first_l.Add(new ListElement(file_path, file_path, gui_n, object_id, ao, tags_prop));
            }

            max_name_width = GUILayout.Width( Mathf.Max(max_width, EditorStyles.toolbarButton.CalcSize(editing_selected_gui).x) );


            PaginateElements(first_l, max_elements_per_page);
            tags_gui.selection_changed = true;
            RebuildSelectedTagsProps();            
        }

    
        void DrawListInstancePropertyFields(SerializedProperty[] props) {
            int l = props.Length;
            for (int i = 0; i < l; i++) {
                EditorGUILayout.PropertyField(props[i], GUIUtils.blank_content, dummy_list_element.widths[i]);
                GUIUtils.LittleButton(EditorColors.clear_color);
            }
        }
  
        
        void SetPropertyAll(int prop_index) {
            IEnumerable<ListElement> l = elements;
            if (selected_elements.Count != 0) l = selected_elements;
            foreach (ListElement el in l) el.relevant_props[prop_index].CopyProperty(dummy_list_element.multi_edit_props[prop_index]);                      
        }
        void MultipleAnimEditFields () {
            for (int i = 0; i < dummy_list_element.props_count; i++) {
                EditorGUILayout.PropertyField(dummy_list_element.multi_edit_props[i], GUIUtils.blank_content, dummy_list_element.widths[i]);
                GUIUtils.LittleButton(EditorColors.clear_color);
            }        
        }
        void DrawMultipleEditWindow () {
            //if (elements.Count == 0) return;
            
            EditorGUILayout.BeginVertical(GUI.skin.box);
            EditorGUILayout.Space();


            EditorGUILayout.BeginHorizontal();

            search_keywords.DrawTagSearch();
            GUILayout.FlexibleSpace();

            GUI.enabled = selected_elements.Count != 0;
            if (GUILayout.Button(import_settings_gui, EditorStyles.miniButton, import_settings_gui.CalcWidth())) {
                OpenImportSettings();
            }
            GUI.enabled = true;



            EditorGUILayout.EndHorizontal();



            EditorGUILayout.Space();

            DrawObjectFieldsTop();

            bool editing_all_shown = selected_elements.Count == 0;
            
            EditorGUILayout.BeginHorizontal();
            if (GUIUtils.LittleButton(EditorColors.red_color, editing_all_shown ? remove_shown_gui : remove_selected_gui)) {
                if (editing_all_shown) OnDeleteIDsFromSet(elements);
                else OnDeleteIDsFromSet(selected_elements);
            }




            
            GUIContent c = editing_all_shown ? editing_shown_gui : editing_selected_gui;
            GUIUtils.ScrollWindowElement (c, false, false, false, max_name_width);
            MultipleAnimEditFields();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
            EditorGUILayout.EndVertical();
        }
        void DrawListInstancePropertyLabels () {
            for (int i = 0; i < dummy_list_element.props_count; i++) {
                EditorGUILayout.LabelField(dummy_list_element.labels[i], dummy_list_element.widths[i]);
                if (GUIUtils.LittleButton(EditorColors.selected_color, set_values_gui)) {
                    SetPropertyAll(i);
                }
                
            }
        }
        void DrawObjectFieldsTop () {
            EditorGUILayout.BeginHorizontal();
            GUIUtils.LittleButton(EditorColors.clear_color);
            GUIUtils.ScrollWindowElement (GUIContent.none, false, false, false, max_name_width);
            
            DrawListInstancePropertyLabels();
            EditorGUILayout.EndHorizontal();
        }
        public bool Draw() {
            bool changed = false;
            bool enter_pressed, delete_pressed, right_p, left_p;
            KeyboardInput(out enter_pressed, out delete_pressed, out left_p, out right_p);
            if (selected_elements.Count != 0) {
                if (delete_pressed) {
                    OnDeleteIDsFromSet(selected_elements);
                    changed = true;
                }
            }
            if (right_p) {
                if (NextPage(max_elements_per_page)) {}
            }
            if (left_p) {
                if (PreviousPage()) {}
                else {}
            }

            if (elements.Count == 0) {
                EditorGUILayout.HelpBox("No elements, add some through the explorer view tab", MessageType.Info);
            }
            DrawMultipleEditWindow();
            DrawListElements();
            DrawPaginationGUI(max_elements_per_page);
            return changed;
        }

        void DrawListElements () {
            EditorGUILayout.BeginVertical(GUI.skin.box);
            for (int i = 0; i < elements.Count; i++) {
                bool drawing_selected = selected_elements.Contains(elements[i]);
                
                EditorGUILayout.BeginHorizontal();
                bool little_button_pressed = GUIUtils.LittleButton(EditorColors.red_color, remove_gui);
                bool big_button_pressed = GUIUtils.ScrollWindowElement (elements[i].label_gui, drawing_selected, false, false, max_name_width);
                DrawListInstancePropertyFields(elements[i].relevant_props);
                EditorGUILayout.EndHorizontal();

                if (big_button_pressed) OnObjectSelection(elements[i], drawing_selected);
                if (little_button_pressed) OnDeleteIDsFromSet(new HashSet<ListElement>() {elements[i]});
            }   

            EditorGUILayout.Space();
            EditorGUILayout.EndVertical();
        }
        const string allTagsField = "allTags";
        const string packsField = "packs";
        AssetObjectPack pack;

        public Dictionary<int, string> id2path;
        public void OnEnable (SerializedObject eventPack, AssetObjectPack pack, Dictionary<int, string> id2path){
            base.OnEnable(eventPack, pack);
            this.pack = pack;
            this.id2path = id2path;
            
            SerializedProperty multi_edit_instance = eventPack.FindProperty(AssetObjectEventPack.multi_edit_instance_field);
            
            multi_edit_instance.FindPropertyRelative(AssetObject.tags_field).ClearArray();
            ReInitializeAssetObjectParameters(multi_edit_instance);
            
            this.dummy_list_element = new DummyListElement(multi_edit_instance, pack.defaultParams);


            var all_packs = AssetObjectsManager.instance.packs;

            packs_so = new SerializedObject(AssetObjectsManager.instance.packs);

            int l = all_packs.packs.Length;

            for (int i = 0; i < l; i++) {
                if (all_packs.packs[i] == pack) {
                    all_tags = packs_so.FindProperty(packsField).GetArrayElementAtIndex(i).FindPropertyRelative(allTagsField);
                    break;
                }
            }

        
            search_keywords.OnEnable(OnSearchKeywordsChange, all_tags);
            tags_gui.OnEnable(OnTagsChanged);          
        
        }

        
        void OnSearchKeywordsChange () {
            pagination.cur_page = 0;
            ClearSelectionsAndRebuild();
            search_keywords.RepopulatePopupList(all_tags);
        }
        void OnTagsChanged (string changed_tag) {
            if (!all_tags.Contains(changed_tag)) all_tags.Add(changed_tag);
            eventPack.ApplyModifiedProperties();
            packs_so.ApplyModifiedProperties();
        }
        
        SearchKeywordsGUI search_keywords = new SearchKeywordsGUI();
        AssetObjectTagsGUI tags_gui = new AssetObjectTagsGUI();
        SerializedProperty[] selected_tags_props;
        SerializedProperty all_tags;
        SerializedObject packs_so;

        
        
    }
}



