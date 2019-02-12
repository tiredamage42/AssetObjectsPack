using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
namespace AssetObjectsPacks {
    public abstract class ListViewElement {
        public string fullName, file_path;
        public GUIContent label_gui;
        public int object_id;
        protected void Initialize(string file_path, string fullName, GUIContent label_gui, int object_id) 
        => (this.file_path, this.fullName, this.label_gui, this.object_id) = (file_path, fullName, label_gui, object_id);
    }
    public abstract class SelectionView<L> where L : ListViewElement {
        protected List<L> elements = new List<L>();
        protected HashSet<L> selected_elements = new HashSet<L>();
        protected SerializedProperty ao_list;
        protected SerializedObject eventPack;
        Editor preview;
        protected AssetObjectPack pack;
        
        public virtual void RealOnEnable(SerializedObject eventPack){
            this.eventPack = eventPack;
            this.ao_list = eventPack.FindProperty(AssetObjectEventPack.asset_objs_field);
        }
        public virtual void InitializeWithPack(AssetObjectPack pack) {
            this.pack = pack;
        }
        






        void ClearSelections(){
            selected_elements.Clear();
            OnSelectionChange();
        }
        public void ClearSelectionsAndRebuild() {
            ClearSelections();
            RebuildAllElements();
        }

        public virtual void InitializeView () {
            ClearSelectionsAndRebuild();
        }

        protected int GetObjectIDAtIndex(int index) {
            return ao_list.GetArrayElementAtIndex(index).FindPropertyRelative(AssetObject.id_field).intValue;
        }
        

        protected abstract void RebuildAllElements();


        protected HashSet<int> IDsFromElements(IEnumerable<L> elements) {
            HashSet<int> ids = new HashSet<int>();
            foreach (L el in elements) ids.Add(el.object_id);
            return ids;
        }
        
        protected void OnDeleteIDsFromSet (IEnumerable<L> elements) {
            HashSet<int> ids = IDsFromElements(elements);
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
                
            if (pagination.PreviousPage()) {
                ClearSelectionsAndRebuild();
                return true;
            }
            return false;
                
        }





            
        int shown_elements_count;
        protected GUIUtils.Pagination pagination = new GUIUtils.Pagination();



        protected void KeyboardInput (out bool enter_pressed, out bool delete_pressed, out bool left_p, out bool right_p){//, int min_index, int max_index) {
            int c = selected_elements.Count;
            Event e = Event.current;
            left_p = e.keyCode == KeyCode.LeftArrow;
            right_p = e.keyCode == KeyCode.RightArrow;
            delete_pressed = e.keyCode == KeyCode.Backspace;
            enter_pressed = e.keyCode == KeyCode.Return;

            if (e.type != EventType.KeyDown) return;
            bool down_p = e.keyCode == KeyCode.DownArrow;
            bool up_p = e.keyCode == KeyCode.UpArrow;

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
                    if (c > 1 && !e.shift) selected_elements.Clear();
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

        


                


        

        

        protected static readonly GUIContent import_settings_gui = new GUIContent("Import Settings");
        


        protected void OpenImportSettings () {
            List<Object> objs = new List<Object>(selected_elements.Count);
            foreach (var el in selected_elements) {
                objs.Add(AssetDatabase.LoadAssetAtPath(pack.objectsDirectory + el.file_path, typeof(Object)));
            }
            Animations.EditImportSettings.CreateWizard(objs.ToArray());
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
        protected bool SelectionHasDirectories() {
            foreach (var s in selected_elements) {
                if (s.object_id == -1) return true;
            }
            return false;
        }

            
        
        protected Object GetObjectRefForElement(L e) {
            return EditorUtils.GetAssetAtPath(pack.objectsDirectory + e.file_path, pack.assetType);  
        }

        void RebuildPreviewEditor () {
            if (preview != null) Editor.DestroyImmediate(preview);
            int c = selected_elements.Count;
            if (c == 0) return;
            if (SelectionHasDirectories()) return;

            HashSet<Object> objs = new HashSet<Object>();   
            foreach (L s in selected_elements) objs.Add( GetObjectRefForElement(s) );
            
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



