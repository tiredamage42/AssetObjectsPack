using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
namespace AssetObjectsPacks {
    public static class PropertyExtensions {
        public static SerializedProperty[] GetRelevantParamsFromAssetObjectInstance (this SerializedProperty ao) {
            SerializedProperty parameters = ao.FindPropertyRelative(AssetObject.params_field);
            return new SerializedProperty[parameters.arraySize].Generate( i => { return SerializedPropertyUtils.Parameters.GetParamProperty(parameters.GetArrayElementAtIndex(i)); } );
        }
    }

    public class ListElement : ListViewElement {
        //public SerializedProperty[] relevant_props;
        public SerializedProperty tags_prop;
        public SerializedProperty ao;
        public ListElement (string file_name, string full_name, GUIContent label_gui, int object_id, SerializedProperty ao, SerializedProperty tags_prop) 
            : base(file_name, full_name, label_gui, object_id) {
            Initialize(file_name, full_name, label_gui, object_id);    
            this.tags_prop = tags_prop;
            this.ao = ao;
            //relevant_props = ao.GetRelevantParamsFromAssetObjectInstance();
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

        DummyListElement dummy_list_element;
        static readonly GUIContent remove_gui = new GUIContent("D", "Remove");
        static readonly GUIContent set_values_gui = new GUIContent("S","Set Values");
        static readonly GUIContent remove_shown_gui = new GUIContent("D", "Remove Shown");
        static readonly GUIContent remove_selected_gui = new GUIContent("D", "Remove Selected");
        static readonly GUIContent editing_shown_gui = new GUIContent("Editing Shown");
        static readonly GUIContent editing_selected_gui = new GUIContent("Editing Selected");
        GUILayoutOption max_name_width;
         bool show_multi_conditions;

        bool[] show_conditions;
        
        GUIContent addParameterToConditionalGUI = new GUIContent("Add Parameter +");
        GUIContent deleteConditionalGUI = new GUIContent("D", "Delete Condition");
        GUIContent addConditionGUI = new GUIContent("Add Condition +");
        
        GUILayoutOption condition_param_width = GUILayout.Width(128);
        




        const string allTagsField = "allTags";
        const string packsField = "packs";
        
        SerializedProperty multi_edit_instance;

        public Dictionary<int, string> id2path;
        


        
        bool HasSearchTags (SerializedProperty tags_prop, int keywords_count) {
            for (int i = 0; i < keywords_count; i++) {
                if (tags_prop.Contains(search_keywords.keywords[i])) {
                    return true;
                }
            }
            return false;
        }
        
        protected override List<ListElement> UnpaginatedListed(string[] all_paths) {
            
            int l = ao_list.arraySize;
            
            float max_width = 0;

            //int keywords_count = search_keywords.keywords.Count;

            List<ListElement> unpaginated = new List<ListElement>();
            for (int i = 0; i < l; i++) {
                SerializedProperty ao = ao_list.GetArrayElementAtIndex(i);
                SerializedProperty tags_prop = ao.FindPropertyRelative(AssetObject.tags_field);
                //if (keywords_count != 0 && !HasSearchTags(tags_prop, keywords_count)) continue;
                
                int id = ao.FindPropertyRelative(AssetObject.id_field).intValue;
                string file_path = id2path[id];
                

                                
                string n = AssetObjectsEditor.RemoveIDFromPath(file_path);
                GUIContent gui_n = new GUIContent(n);


                float w = EditorStyles.toolbarButton.CalcSize(gui_n).x;
                if (w > max_width) max_width = w;
                
                unpaginated.Add(new ListElement(file_path, file_path, gui_n, id, ao, tags_prop));
            }

            max_name_width = GUILayout.Width( Mathf.Max(max_width, EditorStyles.toolbarButton.CalcSize(editing_selected_gui).x) );

            return unpaginated;
        }

        protected override List<ListElement> UnpaginatedFoldered(string[] all_paths) {
            HashSet<string> usedNames = new HashSet<string>();            
            List<ListElement> unpaginated = new List<ListElement>();
            int lastDir = 0;
            



            
            int l = ao_list.arraySize;
            
            float max_width = 0;

            //int keywords_count = search_keywords.keywords.Count;

            for (int i = 0; i < l; i++) {
                SerializedProperty ao = ao_list.GetArrayElementAtIndex(i);
                SerializedProperty tags_prop = ao.FindPropertyRelative(AssetObject.tags_field);
                
                //if (keywords_count != 0 && !HasSearchTags(tags_prop, keywords_count)) continue;
                

                int id = ao.FindPropertyRelative(AssetObject.id_field).intValue;
                string file_path = id2path[id];

                if (!folderView.DisplaysPath(file_path)) continue;


                string name_display = folderView.DisplayNameFromPath(file_path);
                if (usedNames.Contains(name_display)) continue;
                usedNames.Add(name_display);

                if (name_display.Contains(".")) { 
                    string name_string = AssetObjectsEditor.RemoveIDFromPath(EditorUtils.RemoveDirectory(file_path));
                    GUIContent label_gui = new GUIContent( name_string );
                    
                    float w = EditorStyles.toolbarButton.CalcSize(label_gui).x;
                    if (w > max_width) max_width = w;
                    
                    unpaginated.Add(new ListElement(file_path, name_display, label_gui, id, ao, tags_prop));
                    continue;
                }
                //is directory
                unpaginated.Insert(lastDir, new ListElement(file_path, name_display, new GUIContent(name_display), -1, ao, tags_prop));
                lastDir++;   
            
            }

            max_name_width = GUILayout.Width( Mathf.Max(max_width, EditorStyles.toolbarButton.CalcSize(editing_selected_gui).x) );

       
            return unpaginated;
        }

        protected override void OnPagination () {
            show_conditions = new bool[elements.Count];
        }
/*
        void DrawListInstancePropertyFields(SerializedProperty[] props) {
            int l = props.Length;
            for (int i = 0; i < l; i++) {
                EditorGUILayout.PropertyField(props[i], GUIUtils.blank_content, dummy_list_element.widths[i]);
                GUIUtils.SmallButtonClear();
            }
        }

 */
        

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
        //void MultipleAnimEditFields () {
        //    for (int i = 0; i < dummy_list_element.props_count; i++) {
        //        EditorGUILayout.PropertyField(dummy_list_element.multi_edit_props[i], GUIUtils.blank_content, dummy_list_element.widths[i]);
        //        GUIUtils.SmallButtonClear();
        //    }        
        //}

        void DrawMultipleEditWindow (string[] all_paths) {
            //if (elements.Count == 0) return;
            
            DrawToolbar(all_paths);

            GUIUtils.StartBox();

            EditorGUILayout.Space();

            //EditorGUILayout.BeginHorizontal();

            //search_keywords.DrawTagSearch();
            //GUILayout.FlexibleSpace();








            //EditorGUILayout.EndHorizontal();






            EditorGUILayout.Space();

            DrawObjectFieldsTop();

            bool editing_all_shown = selected_elements.Count == 0;
            
            EditorGUILayout.BeginHorizontal();
            if (GUIUtils.SmallButton(editing_all_shown ? remove_shown_gui : remove_selected_gui, EditorColors.red_color, EditorColors.white_color)) {
                OnDeleteIDsFromSet( editing_all_shown ? IDSetFromElements(elements) : IDSetFromElements(selected_elements), all_paths);
                //if (editing_all_shown) 
                //else OnDeleteIDsFromSet(IDsFromElements(selected_elements), all_paths);
            }

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
        void OnDeleteIDsFromSet (HashSet<int> ids, string[] all_paths) {
            //HashSet<int> ids = IDsFromElements(elements);
            for (int i = ao_list.arraySize - 1; i >= 0; i--) {
                if (ids.Contains(GetObjectIDAtIndex(i))) ao_list.DeleteArrayElementAtIndex(i);
            }
            ClearSelectionsAndRebuild(all_paths);
        }


        void DrawListInstancePropertyLabels () {
            for (int i = 0; i < dummy_list_element.props_count; i++) {
                EditorGUILayout.LabelField(dummy_list_element.labels[i], dummy_list_element.widths[i]);
                if (GUIUtils.SmallButton(set_values_gui, EditorColors.selected_color, EditorColors.selected_text_color)) {
                    SetPropertyAll(i);
                }
                
            }
        }
        
        void SetPropertyAll(int prop_index) {
            
            SerializedProperty multi_parameters = multi_edit_instance.FindPropertyRelative(AssetObject.params_field);
            SerializedProperty copy_prop = SerializedPropertyUtils.Parameters.GetParamProperty(multi_parameters.GetArrayElementAtIndex(prop_index));
            
            IEnumerable<ListElement> l = elements;
            if (selected_elements.Count != 0) l = selected_elements;






            foreach (ListElement el in l) {

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

            if (GUIUtils.Button(addConditionGUI, true)) {
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

            if (GUIUtils.Button(addParameterToConditionalGUI, true)) {
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
            
            EditorGUILayout.PropertyField(parameter.FindPropertyRelative(AssetObjectParam.name_field), GUIContent.none, condition_param_width );
            EditorGUILayout.PropertyField(parameter.FindPropertyRelative(AssetObjectParam.param_type_field), GUIContent.none, condition_param_width ) ;
            EditorGUILayout.PropertyField(SerializedPropertyUtils.Parameters.GetParamProperty(parameter), GUIContent.none, condition_param_width );
            
            EditorGUILayout.EndHorizontal();
        }  

        
        
        





        void DrawObjectFieldsTop () {
            EditorGUILayout.BeginHorizontal();
            GUIUtils.SmallButtonClear();
            
            GUIUtils.ScrollWindowElement (GUIContent.none, false, false, false, max_name_width);
            DrawListInstancePropertyLabels();
            EditorGUILayout.EndHorizontal();
            
            //GUILayout.Button(" mass set condition checks here");

        }



        public bool Draw(string[] all_paths) {
            bool changed = false;
            bool enter_pressed, delete_pressed;
            KeyboardInput(all_paths, out enter_pressed, out delete_pressed);


            if (selected_elements.Count != 0) {
                if (delete_pressed) {
                    OnDeleteIDsFromSet(IDSetFromElements(selected_elements), all_paths);
                    changed = true;
                }
            }

            DrawMultipleEditWindow(all_paths);
            if (elements.Count == 0) {
                EditorGUILayout.HelpBox("No elements, add some through the explorer view tab", MessageType.Info);
            }
            else {
                DrawElements(all_paths);        
            }
            DrawPaginationGUI(all_paths);
            return changed;
        }

        GUIContent showConditionsGUI = new GUIContent("C", "Show Conditions");

        protected override void DrawNonFolderElement(string[] all_paths, ListElement element, bool selected, int index) {
            EditorGUILayout.BeginHorizontal();
            bool little_button_pressed = GUIUtils.SmallButton(remove_gui, EditorColors.red_color, EditorColors.white_color);
            
            show_conditions[index] = GUIUtils.SmallToggleButton(showConditionsGUI, show_conditions[index]);
            
            
            bool big_button_pressed = GUIUtils.ScrollWindowElement (element.label_gui, selected, false, false, max_name_width);
            
            DrawParameterFields(element.ao);
            //DrawListInstancePropertyFields(element.relevant_props);
            EditorGUILayout.EndHorizontal();

            if (show_conditions[index]) {
                DrawConditions(element.ao);
            }


            if (big_button_pressed) OnObjectSelection(element, selected);
            if (little_button_pressed) OnDeleteIDsFromSet(new HashSet<int>() {element.object_id}, all_paths);

        }


        
        public override void RealOnEnable(SerializedObject eventPack){
            base.RealOnEnable(eventPack);
            multi_edit_instance = eventPack.FindProperty(AssetObjectEventPack.multi_edit_instance_field);
            //multi_edit_instance.FindPropertyRelative(AssetObject.tags_field).ClearArray();
            
        }
        public void InitializeWithPack(AssetObjectPack pack, Dictionary<int, string> id2path) {
            base.InitializeWithPack(pack);
            this.id2path = id2path;

            AssetObjectParamDef[] defs = pack.defaultParams;

            //clear conditionals as well
            multi_edit_instance.FindPropertyRelative(AssetObject.conditionChecksField).ClearArray();

            ReInitializeAssetObjectParameters(multi_edit_instance, defs);



            this.dummy_list_element = new DummyListElement(multi_edit_instance, defs);

            var all_packs = AssetObjectsManager.instance.packs;
            packs_so = new SerializedObject(all_packs);

            int l = all_packs.packs.Length;
            for (int i = 0; i < l; i++) {
                if (all_packs.packs[i] == pack) {
                    all_tags = packs_so.FindProperty(packsField).GetArrayElementAtIndex(i).FindPropertyRelative(allTagsField);
                    break;
                }
            }
            //search_keywords.OnEnable(OnSearchKeywordsChange, all_tags);
            tags_gui.OnEnable(OnTagsChanged);          


        }
        
        

        
        void OnSearchKeywordsChange (string[] all_paths) {
            paginateView.ResetPage();
            
            ClearSelectionsAndRebuild(all_paths);
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



