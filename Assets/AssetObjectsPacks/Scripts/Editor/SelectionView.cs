using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
namespace AssetObjectsPacks {
    public abstract class ListViewElement {
        public string full_name, file_path;
        public GUIContent label_gui;
        public int object_id;
        protected void Initialize(string file_path, string full_name, GUIContent label_gui, int object_id) 
        => (this.file_path, this.full_name, this.label_gui, this.object_id) = (file_path, full_name, label_gui, object_id);
    }
    public abstract class SelectionView<L> where L : ListViewElement {
        protected List<L> elements = new List<L>();
        protected HashSet<L> selected_elements = new HashSet<L>();
        protected HashSet<int> ids_in_set = new HashSet<int>();
        protected SerializedProperty ao_list;
        protected SerializedObject eventPack;
        Editor preview;
        AssetObjectPack pack;
        
        protected virtual void OnEnable(SerializedObject eventPack, AssetObjectPack pack){
            this.pack = pack;
            this.eventPack = eventPack;
            this.ao_list = eventPack.FindProperty(AssetObjectEventPack.asset_objs_field);
        }

        void ClearSelections(){
            selected_elements.Clear();
            OnSelectionChange();

        }
        public void ClearSelectionsAndRebuild() {
            ClearSelections();
            RebuildAllElements();
        }

        void InitializeIDsInSet() {
            ids_in_set.Clear();
            int c = ao_list.arraySize;
            for (int i = 0; i < c; i++) {
                ids_in_set.Add(GetObjectIDAtIndex(i));
            }
        }
        public virtual void InitializeView () {
            InitializeIDsInSet ();
            ClearSelectionsAndRebuild();
        }

        protected int GetObjectIDAtIndex(int index) {
            return ao_list.GetArrayElementAtIndex(index).FindPropertyRelative(AssetObject.id_field).intValue;
        }
        protected virtual void OnAddIDsToSet (HashSet<L> elements) {
            bool reset_i = true;
            foreach (L el in elements) {
                ids_in_set.Add(el.object_id);

                Debug.Log(el.file_path);
                ResetInstance(el.object_id, GetObjectRefForElement(el), el.file_path, reset_i);
                reset_i = false;
            }
            ClearSelectionsAndRebuild();
        }

        //only need to default first one added, the rest will copy the last one 'inserted' into the
        //serialized property array
        void ResetInstance (int obj_id, Object obj_ref, string file_path, bool make_default) {
            SerializedProperty inst = ao_list.AddNewElement();
            inst.FindPropertyRelative(AssetObject.id_field).intValue = obj_id;
            inst.FindPropertyRelative(AssetObject.obj_ref_field).objectReferenceValue = obj_ref;
            inst.FindPropertyRelative(AssetObject.obj_file_path_field).stringValue = file_path;
            if (make_default) {
                inst.FindPropertyRelative(AssetObject.tags_field).ClearArray();
                MakeAssetObjectInstanceDefault(inst);
            }
        } 

        protected abstract void RebuildAllElements();


        protected HashSet<int> IDsFromElements(IEnumerable<L> elements) {
            HashSet<int> ids = new HashSet<int>();
            foreach (L el in elements) ids.Add(el.object_id);
            return ids;
        }
        
        protected void OnDeleteIDsFromSet (IEnumerable<L> elements) {
            HashSet<int> ids = IDsFromElements(elements);
            foreach(var id in ids) {
                ids_in_set.Remove(id);
            }
            for (int i = ao_list.arraySize - 1; i >= 0; i--) {
                if (ids.Contains(GetObjectIDAtIndex(i))) ao_list.DeleteArrayElementAtIndex(i);
            }
            ClearSelectionsAndRebuild();
        }

        protected void PaginateElements (List<L> all_shown, int per_page) {
            shown_elements_count = all_shown.Count;
            int min, max;
            pagination.GetIndexRange(out min, out max, per_page, shown_elements_count);
            elements.AddRange(all_shown.Slice(min, max));
        }   
        protected void DrawPaginationGUI (int per_page) {
            if (pagination.ChangePageGUI (shown_elements_count, per_page)) {
                ClearSelectionsAndRebuild();
            }

        }

        protected bool NextPage(int per_page) {
            if (pagination.NextPage(shown_elements_count, per_page)) {
                ClearSelectionsAndRebuild();   
                return true;
            }
            return false;
        }
        protected bool PreviousPage() {
            Debug.Log("previous a");
                
            if (pagination.PreviousPage()) {
                Debug.Log("previous");
                ClearSelectionsAndRebuild();
                return true;
            }
            return false;
                
        }





            
        int shown_elements_count;
        protected GUIUtils.Pagination pagination = new GUIUtils.Pagination();



        protected void KeyboardInput (out bool enter_pressed, out bool delete_pressed, out bool left_p, out bool right_p){//, int min_index, int max_index) {
            enter_pressed = false;
            delete_pressed = false;
            left_p = false;
            right_p = false;

            int c = selected_elements.Count;

            Event e = Event.current;
            if (e.type != EventType.KeyDown) return;

            bool down_p = e.keyCode == KeyCode.DownArrow;
            bool up_p = e.keyCode == KeyCode.UpArrow;
            left_p = e.keyCode == KeyCode.LeftArrow;
            right_p = e.keyCode == KeyCode.RightArrow;
            delete_pressed = e.keyCode == KeyCode.Backspace;
            enter_pressed = e.keyCode == KeyCode.Return;

            if (down_p || up_p || left_p || right_p || delete_pressed || enter_pressed) e.Use();    
            
            L element_to_select = null;

            if ((down_p || up_p) && c == 0) {
                hi_selection = lo_selection = down_p ? 0 : elements.Count - 1;
                element_to_select = elements[hi_selection];
            }
            if (c != 0) {
                bool sel_changed = false;
                if (down_p) {
                    if (hi_selection < elements.Count - 1 || (c > 1 && !e.shift)) {
                        if (hi_selection < elements.Count - 1) {

                            hi_selection++;
                        }
                        lo_selection = hi_selection;
                        sel_changed = true;
                    }
                }
                if (up_p) {
                    if (lo_selection > 0 || (c > 1 && !e.shift)) {
                        if (lo_selection > 0) {

                            lo_selection--;
                        }
                        hi_selection = lo_selection;
                        sel_changed = true;
                    }
                }
                if ((down_p || up_p) && sel_changed) {
                    if (c > 1 && !e.shift) {
                        selected_elements.Clear();
                    }
                    element_to_select = elements[hi_selection];
                }
            }
                
            if (element_to_select != null) OnObjectSelection(element_to_select, false);

        }

        protected void OnObjectSelection (L selection, bool was_selected) {
            Event e = Event.current;
            if (e.shift) {
                selected_elements.Add(selection);
                RecalculateLowestAndHighestSelections();
                selected_elements.Clear();
                for (int i = lo_selection; i <= hi_selection; i++) selected_elements.Add(elements[i]);
            }
            else if (e.command || e.control) {
                if (was_selected) selected_elements.Remove(selection);
                else selected_elements.Add(selection);
            }
            else {
                selected_elements.Clear();
                if (!was_selected) selected_elements.Add(selection);
            }
            OnSelectionChange();
        }
        
        int lo_selection, hi_selection;
        void RecalculateLowestAndHighestSelections () {
            lo_selection = int.MaxValue;
            hi_selection = int.MinValue;
            int s = selected_elements.Count;
            if (s == 0) return;
            
            int c = elements.Count;
            for (int i = 0; i < c; i++) {
                if (selected_elements.Contains(elements[i])) {                    
                    if (i < lo_selection) 
                        lo_selection = i;
                    if (i > hi_selection) 
                        hi_selection = i;
                    if (s == 1) break;
                }
            }
        }
  
        
        protected virtual void OnSelectionChange() {
            RecalculateLowestAndHighestSelections();
            RebuildPreviewEditor();        
        }

        protected void MakeAssetObjectInstanceDefault(SerializedProperty inst) {
            //inst.FindPropertyRelative(AssetObject.base_params[0]).floatValue = -1;
            //inst.FindPropertyRelative(AssetObject.base_params[1]).boolValue = false;


            SerializedProperty params_list = inst.FindPropertyRelative(AssetObject.params_field);
            params_list.ClearArray();
            
            AssetObjectParamDef[] defs = pack.defaultParams;
            //int l = pack.defaultParameters.Length;
            int l = defs.Length;
            
            for (int i = 0; i < l; i++) {                
                //AssetObjectParam def_param = pack.defaultParameters[i].parameter;
                AssetObjectParam def_param = defs[i].parameter;
                
                SerializedProperty new_param = params_list.AddNewElement();
                new_param.FindPropertyRelative(AssetObjectParam.name_field).stringValue = def_param.name;
                new_param.FindPropertyRelative(AssetObjectParam.param_type_field).enumValueIndex = (int)def_param.paramType;
                new_param.FindPropertyRelative(AssetObjectParam.bool_val_field).boolValue = def_param.boolValue;
                new_param.FindPropertyRelative(AssetObjectParam.float_val_field).floatValue = def_param.floatValue;
                new_param.FindPropertyRelative(AssetObjectParam.int_val_field).intValue = def_param.intValue;
            }
        }

        public virtual void OnInteractivePreviewGUI(Rect r, GUIStyle background) {
            if (preview != null) preview.OnInteractivePreviewGUI(r, background); 
        }
        public void OnPreviewSettings() {
            if (preview != null) preview.OnPreviewSettings();
        }
        protected bool SelectionHasDirectories() {
            foreach (var s in selected_elements) {
                if (s.object_id == -1) return true;
            }
            return false;
        }

        Object GetObjectRefForElement(L e) {
            Object[] all_objects = EditorUtils.GetAssetsAtPath(pack.objectsDirectory + e.file_path, pack.assetType);    
            // fbx files have extra "preview" clip that was getting in the awy (mixamo)
            // need to use the last one
            Object o = all_objects[0];// e.file_path.EndsWith(".fbx") ? all_objects.Last() : all_objects[0];

            if (o.name.Contains("__preview__")) {
                Debug.LogError("Getting preview");
            }
            return o;
             
        }
        void RebuildPreviewEditor () {
            if (preview != null) Editor.DestroyImmediate(preview);
            int c = selected_elements.Count;
            if (c == 0) return;
            if (SelectionHasDirectories()) return;


            HashSet<Object> objs = new HashSet<Object>();   
            foreach (L s in selected_elements) {
                objs.Add( GetObjectRefForElement(s) );
            }

            preview = Editor.CreateEditor(objs.ToArray());
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
    }
}



