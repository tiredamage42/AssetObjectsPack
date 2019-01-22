

//using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
namespace AssetObjectsPacks {
    public abstract class ListViewElement {
        public string full_name;
        public GUIContent label_gui;
        public int object_id;
        protected void Initialize(string full_name, GUIContent label_gui, int object_id) 
        => (this.full_name, this.label_gui, this.object_id) = (full_name, label_gui, object_id);
    }
    public abstract class SelectionView<T, L> where T : Object where L : ListViewElement {
        protected List<L> all_elements = new List<L>();
        protected Vector2 scroll_pos;
        protected List<int> selected_ids = new List<int>();
        protected List<int> ids_in_set = new List<int>();
        protected Dictionary<int, string> id2path;
        protected SerializedProperty targeted_list_prop;
        System.Action<SerializedProperty> make_instance_default;
        Editor preview_editor;
        string pack_name;
        protected const string sTags = "tags";
        protected const string sID = "id";
        const string sObjRef = "object_reference";
        
        public bool HasPreviewGUI() { 
            return selected_ids.Count != 0;
        }
        protected virtual void OnEnable(SerializedObject serializedObject, string pack_name, Dictionary<int, string> id2path, System.Action<SerializedProperty> make_instance_default) {
            this.id2path = id2path;
            this.pack_name = pack_name;
            this.make_instance_default = make_instance_default;
            this.targeted_list_prop = serializedObject.FindProperty("runtimeAssetObjects");
        }

        public virtual void ReinitializePaths (Dictionary<int, string> id2path) {
            this.id2path = id2path;
        }

        void CheckIDsForSelections (List<int> ids) {
            if (ids.Count == 1) {
                if (selected_ids.Contains(ids[0])) {
                    selected_ids.Remove(ids[0]);
                    OnSelectionChange();
                }
            }
            else {
                ClearSelections();
            }
        }

        protected virtual void OnAddIDsToSet (List<int> ids) {
            string asset_load_file_prefix = AssetObjectsEditor.GetAssetObjectsDirectory(pack_name); 
            int c = ids.Count;
            for (int i = 0; i < c; i++) {
                int id = ids[i];
                ids_in_set.Add(id);
                string file_path = asset_load_file_prefix + id2path[id];
                int l = targeted_list_prop.arraySize;
                targeted_list_prop.InsertArrayElementAtIndex(l);
                ResetInstance(targeted_list_prop.GetArrayElementAtIndex(l), id, EditorUtils.GetAssetAtPath<T>(file_path));
                Debug.Log("Adding : " + id);
            }
            CheckIDsForSelections(ids);
            RebuildAllElements();
        }

        protected abstract void RebuildAllElements();

        void InitializeIDsInSet () {
            ids_in_set.Clear();
            int l = targeted_list_prop.arraySize;
            for (int i = 0; i< l; i++) {
                ids_in_set.Add(targeted_list_prop.GetArrayElementAtIndex(i).FindPropertyRelative(sID).intValue);
            }
        }
        protected void OnDeleteIDsFromSet (List<int> ids) {
            CheckIDsForSelections(ids);
            for (int i = targeted_list_prop.arraySize - 1; i >= 0; i--) {
                int id = targeted_list_prop.GetArrayElementAtIndex(i).FindPropertyRelative(sID).intValue;
                if (ids.Contains(id)) {
                    targeted_list_prop.DeleteArrayElementAtIndex(i);
                    ids_in_set.Remove(id);
                }
            }
            RebuildAllElements();
        }

        public virtual void InitializeView () {
            ClearSelections();
            InitializeIDsInSet();
            RebuildAllElements();
        }

        protected void OnObjectSelection (int selection_id, bool drawing_selected) {
            Event e = Event.current;
            if (!e.shift)
                selected_ids.Clear();
            if (drawing_selected) {
                selected_ids.Remove(selection_id);
            }
            else {
                selected_ids.Add(selection_id);
            }
            OnSelectionChange();
        }

        protected virtual void OnSelectionChange() {
            RebuildPreviewEditor();        
        }

        protected void ClearSelections() {
            selected_ids.Clear();
            OnSelectionChange();
        }

        void ResetInstance (SerializedProperty inst, int obj_id, Object obj_ref) {
            inst.FindPropertyRelative(sTags).ClearArray();
            inst.FindPropertyRelative(sID).intValue = obj_id;
            inst.FindPropertyRelative(sObjRef).objectReferenceValue = obj_ref;
            make_instance_default(inst);
        } 
        
        public virtual void OnInteractivePreviewGUI(Rect r, GUIStyle background) {
            if (preview_editor != null) { 
                preview_editor.OnInteractivePreviewGUI(r, background); 
            }
        }
        public void OnPreviewSettings() {
            if (preview_editor != null) 
                preview_editor.OnPreviewSettings();
        }
    
        void RebuildPreviewEditor () {
            if (preview_editor != null) 
                Editor.DestroyImmediate(preview_editor);
            int c = selected_ids.Count;
            if (c == 0) return;
            T[] clips = new T[c];   

            string asset_load_file_prefix = AssetObjectsEditor.GetAssetObjectsDirectory(pack_name); 

            for (int i = 0; i < c; i++) {
                string p = asset_load_file_prefix + id2path[selected_ids[i]];
                clips[i] = EditorUtils.GetAssetAtPath<T>(p);
            }
            preview_editor = Editor.CreateEditor(clips);
            preview_editor.HasPreviewGUI();
            preview_editor.OnInspectorGUI();
        }
    }
}



