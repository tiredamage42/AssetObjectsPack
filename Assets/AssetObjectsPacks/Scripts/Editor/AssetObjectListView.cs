

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
namespace AssetObjectsPacks {

    public static class PropertyExtensions {
        public static SerializedProperty[] GetRelevantParamsFromAssetObjectInstance (this SerializedProperty ao) {
            SerializedProperty params_list = ao.FindPropertyRelative("parameters");
            int l = params_list.arraySize;

            SerializedProperty[] return_props = new SerializedProperty[l];
            for (int i = 0; i < l; i++) {
                SerializedProperty param_prop = params_list.GetArrayElementAtIndex(i);
                SerializedProperty paramType = param_prop.FindPropertyRelative("paramType");
                AssetObjectParam.ParamType actual_type = (AssetObjectParam.ParamType)paramType.enumValueIndex;
                SerializedProperty param_to_draw = null;
                switch(actual_type) {
                    case AssetObjectParam.ParamType.Bool:
                    Debug.Log("Drawing bool");
                        param_to_draw = param_prop.FindPropertyRelative("boolValue");
                        break;
                    case AssetObjectParam.ParamType.Float:
                    Debug.Log("Drawing float");
                    
                        param_to_draw = param_prop.FindPropertyRelative("floatValue");
                        break;
                    case AssetObjectParam.ParamType.Int:
                    Debug.Log("Drawing int");
                    
                        param_to_draw = param_prop.FindPropertyRelative("intValue");                    
                        break;
                }
                return_props[i] = param_to_draw;
            }


            return return_props;
        }
    }

    public class ListElement : ListViewElement {
        public SerializedProperty[] relevant_props;
        public ListElement (
            string full_name, 
            GUIContent label_gui, 
            int object_id, 
            //string[] field_names, 
            SerializedProperty instance
        ) {
            Initialize(full_name, label_gui, object_id);    
            relevant_props = instance.GetRelevantParamsFromAssetObjectInstance();

            
            //props = props.GenerateArray(i => { return instance.FindPropertyRelative(field_names[i]); }, field_names.Length);
        }
    }
    public class AssetObjectListView : SelectionView<ListElement>
    {
        protected override void OnSelectionChange() {
            base.OnSelectionChange();
            tag_objects_system.OnSelectionChanged(selected_ids);
        }
        public override void OnInteractivePreviewGUI(Rect r, GUIStyle background) {
            base.OnInteractivePreviewGUI(r, background);
            tag_objects_system.OnInteractivePreviewGUI(r, background);
        }
            
        public class DummyListElement {
            public GUIContent[] labels;
            public GUILayoutOption[] widths;
            //public string[] names;
            public SerializedProperty[] multi_edit_props;
            public int props_count;
            public DummyListElement (
                SerializedProperty multi_edit_instance, 
                AssetObjectParamDef[] default_params//,
                //string[] names, 
                //GUIContent[] labels
            ){
                props_count = default_params.Length;
                //this.names = names;
                //this.labels = labels;

                labels = new GUIContent[props_count];
                widths = new GUILayoutOption[props_count];
                for (int i = 0; i < props_count; i++) {

                    labels[i] = new GUIContent(default_params[i].parameter.name, default_params[i].hint);
                    widths[i] = GUILayout.Width(EditorStyles.label.CalcSize(labels[i]).x);
                }


                multi_edit_props = multi_edit_instance.GetRelevantParamsFromAssetObjectInstance();

                for (int i = 0; i < props_count; i++) {
                    Debug.Log(multi_edit_props[i]);
                }

                
                
                //multi_edit_props = multi_edit_props.GenerateArray(i => { return multi_edit_instance.FindPropertyRelative(names[i]); }, names.Length);
            }

        }

        TagObjectSystem tag_objects_system = new TagObjectSystem();
        DummyListElement dummy_list_element;
        static readonly GUIContent remove_gui = new GUIContent("", "Remove");
        static readonly GUIContent set_values_gui = new GUIContent("","Set Values");
        static readonly GUIContent remove_shown_gui = new GUIContent("", "Remove Shown");
        static readonly GUIContent remove_selected_gui = new GUIContent("", "Remove Selected");
        static readonly GUIContent editing_shown_gui = new GUIContent("Editing Shown");
        static readonly GUIContent editing_selected_gui = new GUIContent("Editing Selected");
        static readonly GUIContent name_gui = new GUIContent("Name");
        
        
        protected override void RebuildAllElements() {
            all_elements.Clear();
            int l = targeted_list_prop.arraySize;
            for (int i = 0; i < l; i++) {
                SerializedProperty list_instance = targeted_list_prop.GetArrayElementAtIndex(i);
                int object_id = list_instance.FindPropertyRelative(sID).intValue;
                string file_path = id2path[object_id];
                string n = AssetObjectsEditor.RemoveIDFromPath(file_path);
                all_elements.Add(new ListElement(file_path, new GUIContent(n), object_id,
                 
                //dummy_list_element.names, 
                list_instance));
            }
            tag_objects_system.RebuildListTrackers(selected_ids);
        }

        List<TagObjectSystem.TagsListTracker> RebuildTagsProperties() {
            List<TagObjectSystem.TagsListTracker> ret = new List<TagObjectSystem.TagsListTracker>();
            int l = targeted_list_prop.arraySize;
            for (int i = 0; i < l; i++) {
                SerializedProperty anim = targeted_list_prop.GetArrayElementAtIndex(i);
                SerializedProperty anim_tags_list_prop = anim.FindPropertyRelative(sTags);
                int anim_id = anim.FindPropertyRelative(sID).intValue;
                ret.Add(new TagObjectSystem.TagsListTracker(anim_tags_list_prop, anim_id));
            }
            return ret;
        }
        void DrawListInstancePropertyFields(SerializedProperty[] props) {
            int l = props.Length;
            for (int i = 0; i < l; i++) {
                EditorGUILayout.PropertyField(props[i], GUIUtils.blank_content, dummy_list_element.widths[i]);
                GUIUtils.LittleButton(EditorColors.clear_color);
            }
        }
        void DrawAssetObjectParam (SerializedProperty param_prop, GUILayoutOption width) {
            SerializedProperty paramType = param_prop.FindPropertyRelative("paramType");
            AssetObjectParam.ParamType actual_type = (AssetObjectParam.ParamType)paramType.enumValueIndex;
            SerializedProperty param_to_draw = null;
            switch(actual_type) {
                case AssetObjectParam.ParamType.Bool:
                    param_to_draw = param_prop.FindPropertyRelative("boolValue");
                    break;
                case AssetObjectParam.ParamType.Float:
                    param_to_draw = param_prop.FindPropertyRelative("floatValue");
                    break;
                case AssetObjectParam.ParamType.Int:
                    param_to_draw = param_prop.FindPropertyRelative("intValue");                    
                    break;
            }
            EditorGUILayout.PropertyField(param_to_draw, GUIUtils.blank_content, width);

        }

        

        


















        void RemoveShownFromAnimSet () {
            List<int> ids2delete = new List<int>();
            for (int i = 0; i < all_elements.Count; i++) {
                if (tag_objects_system.keywords_filter[i]) {
                    ids2delete.Add(all_elements[i].object_id);
                }
            }
            OnDeleteIDsFromSet(ids2delete);
        }
        void SetPropertyAll(int prop_index) {
            for (int i = 0; i < all_elements.Count; i++) {
                if (selected_ids.Count == 0 || selected_ids.Contains(all_elements[i].object_id)) {

                    all_elements[i].relevant_props[prop_index].CopyProperty(dummy_list_element.multi_edit_props[prop_index]);
                    //all_elements[i].props[prop_index].CopyProperty(dummy_list_element.multi_edit_props[prop_index]);
                
                
                }
            }
        }
        
        void MultipleAnimEditFields () {
            for (int i = 0; i < dummy_list_element.props_count; i++) {
                EditorGUILayout.PropertyField(dummy_list_element.multi_edit_props[i], GUIUtils.blank_content, dummy_list_element.widths[i]);
                if (GUIUtils.LittleButton(EditorColors.white_color, set_values_gui)) {
                    SetPropertyAll(i);
                }
            }        
        }

        void DrawMultipleEditWindow () {
            EditorGUILayout.BeginVertical(GUI.skin.box);
            EditorGUILayout.BeginScrollView(Vector2.zero, GUILayout.Height(60));
            EditorGUILayout.BeginHorizontal();
            bool editing_all_shown = selected_ids.Count == 0;
            if (GUIUtils.LittleButton(EditorColors.red_color, editing_all_shown ? remove_shown_gui : remove_selected_gui)) {
                if (editing_all_shown) 
                    RemoveShownFromAnimSet();
                else 
                    OnDeleteIDsFromSet(selected_ids);
            }
            GUIUtils.ScrollWindowElement (editing_all_shown ? editing_shown_gui : editing_selected_gui, false, false, false);
            MultipleAnimEditFields();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
            tag_objects_system.DrawTagsSearch();
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }
        void DrawListInstancePropertyLabels () {
            for (int i = 0; i < dummy_list_element.props_count; i++) {
                EditorGUILayout.LabelField(dummy_list_element.labels[i], dummy_list_element.widths[i]);
                GUIUtils.LittleButton(EditorColors.clear_color);
            }
        }
        void DrawObjectFieldsTop () {
            EditorGUILayout.BeginHorizontal();
            GUIUtils.LittleButton(EditorColors.clear_color);
            GUIUtils.ScrollWindowElement (name_gui, false, false, false);
            DrawListInstancePropertyLabels();
            EditorGUILayout.EndHorizontal();
        }
        public void Draw(int scroll_view_height) {
            DrawMultipleEditWindow();
            EditorGUILayout.BeginVertical(GUI.skin.box);
            scroll_pos = EditorGUILayout.BeginScrollView(scroll_pos, GUILayout.Height(scroll_view_height));
            DrawObjectFieldsTop();
            for (int i = 0; i < all_elements.Count; i++) {
                int object_id = all_elements[i].object_id;
                bool drawing_selected = selected_ids.Contains(object_id);
                if (!tag_objects_system.keywords_filter[i]) {
                    if (drawing_selected) {
                        selected_ids.Remove(object_id);
                        OnSelectionChange();
                    }
                    continue;
                }
                EditorGUILayout.BeginHorizontal();
                bool little_button_pressed = GUIUtils.LittleButton(EditorColors.red_color, remove_gui);
                bool big_button_pressed = GUIUtils.ScrollWindowElement (all_elements[i].label_gui, drawing_selected, false, false);
                //DrawListInstancePropertyFields(all_elements[i].props);
                DrawListInstancePropertyFields(all_elements[i].relevant_props);
                
                EditorGUILayout.EndHorizontal();
                if (big_button_pressed)
                    OnObjectSelection(object_id, drawing_selected);
                if (little_button_pressed)
                    OnDeleteIDsFromSet(new List<int>() {object_id});
            }
            EditorGUILayout.EndScrollView();        
            EditorGUILayout.EndVertical();
        }
        public void OnEnable (
            SerializedObject so, SerializedObject editor_as_so, string asset_object_unity_asset_type, string pack_name, 
            Dictionary<int, string> id2path, 
            //System.Action<SerializedProperty> make_instance_default, 
            AssetObjectParamDef[] default_params
            //string[] instance_field_names, GUIContent[] instance_field_labels
        ) {
            base.OnEnable(so, asset_object_unity_asset_type, pack_name, id2path, default_params);//, make_instance_default);

            
            Debug.Log("1");
            SerializedProperty multi_edit_instance = so.FindProperty("multi_edit_instance");
            //if (multi_edit_l.arraySize == 0) {
            //    Debug.Log("1a");
            //    multi_edit_l.InsertArrayElementAtIndex(0);
            //}
            
            Debug.Log("2");
            //SerializedProperty multi_edit_instance = multi_edit_l.GetArrayElementAtIndex(0);
            Debug.Log(multi_edit_instance.propertyType);
            multi_edit_instance.FindPropertyRelative(sTags).ClearArray();
            MakeAssetObjectInstanceDefault(multi_edit_instance);
            //make_instance_default(multi_edit_instance);
            Debug.Log("3");
            editor_as_so.ApplyModifiedProperties();

                
            this.dummy_list_element = new DummyListElement(multi_edit_instance, default_params);//, instance_field_names, instance_field_labels);
            tag_objects_system.OnEnable(pack_name, RebuildTagsProperties, so);                
        }
    }
}



