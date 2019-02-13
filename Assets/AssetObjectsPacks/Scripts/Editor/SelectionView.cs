using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
namespace AssetObjectsPacks {
    public class ListViewElement {
        public string fullName, file_path;
        public GUIContent label_gui;
        public int object_id;
        public ListViewElement(string file_path, string fullName, GUIContent label_gui, int object_id) => (this.file_path, this.fullName, this.label_gui, this.object_id) = (file_path, fullName, label_gui, object_id);
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

        
        protected FoldersView folderView = new FoldersView();
        protected PaginateView paginateView = new PaginateView();


        protected int max_elements_per_page = 25;
        protected bool foldered;


        GUIContent foldersViewGUI = new GUIContent("Folders View");
        GUIContent importSettingsGUI = new GUIContent("Import Settings");
        
        
        public virtual void RealOnEnable(SerializedObject eventPack){
            this.eventPack = eventPack;
            this.ao_list = eventPack.FindProperty(AssetObjectEventPack.asset_objs_field);
        }
        public virtual void InitializeWithPack(AssetObjectPack pack) {
            this.pack = pack;
        }
        protected HashSet<int> IDSetFromElements (IEnumerable<L> elements) {
            return new HashSet<int>().Generate(elements, o => { return o.object_id; } );
        }
        
        

        
        protected abstract void DrawNonFolderElement (string[] all_paths, L element, bool selected, int index);
        protected void DrawElements (string[] all_paths) {

            GUIUtils.StartBox();
            for (int i = 0; i < elements.Count; i++) {

                L element = elements[i];
                bool drawing_selected = selected_elements.Contains(element);

                int object_id = elements[i].object_id;
                    
                if (object_id == -1) {
                    if (GUIUtils.ScrollWindowElement (elements[i].label_gui, drawing_selected, false, true)) {
                        MoveForwardFolder(elements[i].fullName, all_paths);
                    }
                }
                else {
                    DrawNonFolderElement(all_paths, element, drawing_selected, i);
                }       
            }
            GUIUtils.EndBox();

        }

        protected virtual void OnPagination () {

        }
        protected void OnFolderViewChange (string[] all_paths) {
            paginateView.ResetPage();
            ClearSelectionsAndRebuild(all_paths);
        }

        protected void MoveForwardFolder (string addPath, string[] all_paths) {
            folderView.MoveForward(addPath);
            OnFolderViewChange(all_paths);
        }

        






        void ClearSelections(){
            selected_elements.Clear();
            OnSelectionChange();
        }
        public void ClearSelectionsAndRebuild(string[] all_paths) {
            ClearSelections();
            RebuildAllElements(all_paths);
        }

        public virtual void InitializeView (string[] all_paths) {
            paginateView.ResetPage();
            ClearSelectionsAndRebuild(all_paths);
        }

        protected int GetObjectIDAtIndex(int index) {
            return ao_list.GetArrayElementAtIndex(index).FindPropertyRelative(AssetObject.id_field).intValue;
        }
        
        protected abstract List<L> UnpaginatedFoldered(string[] all_paths);
        protected abstract List<L> UnpaginatedListed(string[] all_paths);
        
        protected void RebuildAllElements(string[] all_paths) {
            elements.Clear();
            List<L> unpaginated = null;
            if (foldered) unpaginated = UnpaginatedFoldered(all_paths);
            else unpaginated = UnpaginatedListed(all_paths);
            PaginateElements(unpaginated);
            OnPagination();
        }
        
        void PaginateElements (List<L> all_shown) {
            int min, max;
            paginateView.Paginate(all_shown.Count, max_elements_per_page, out min, out max);
            elements.AddRange(all_shown.Slice(min, max));
        }   
        
        protected virtual void ExtraToolbarButtons(string[] all_paths, bool has_selection, bool selection_has_directories) {}
        protected void DrawToolbar (string[] all_paths) {

            GUIUtils.StartBox();
            EditorGUILayout.BeginHorizontal();
            bool has_selection = selected_elements.Count != 0;
            bool selection_has_directories = false;
            if (has_selection) { selection_has_directories = SelectionHasDirectories(); }
            if (foldered && folderView.DrawBackButton()) OnFolderViewChange(all_paths);

            bool lastview = foldered;
            foldered = GUIUtils.ToggleButton(foldersViewGUI, true, foldered);
            if (foldered != lastview) InitializeView(all_paths);

            GUI.enabled = has_selection && !selection_has_directories;
            if (GUIUtils.Button(importSettingsGUI, true)) OpenImportSettings();
            GUI.enabled = true;
            
            ExtraToolbarButtons(all_paths, has_selection, selection_has_directories);
            
            EditorGUILayout.EndHorizontal();

            GUIUtils.EndBox();


        }


        
        
        protected void DrawPaginationGUI (string[] all_paths) {
            if (paginateView.ChangePageGUI ()) ClearSelectionsAndRebuild(all_paths);
        }

        bool NextPage(string[] all_paths) {
            if (paginateView.NextPage()) {
                ClearSelectionsAndRebuild(all_paths);   
                return true;
            }
            return false;
        }
        bool PreviousPage(string[] all_paths) {
            if (paginateView.PreviousPage()) {
                ClearSelectionsAndRebuild(all_paths);
                return true;
            }
            return false;
                
        }




        protected void KeyboardInput (string[] all_paths, out bool enter_pressed, out bool delete_pressed){
            delete_pressed = enter_pressed = false;
            Event e = Event.current;
            if (e.type != EventType.KeyDown) return;
            
            bool down_p = e.keyCode == KeyCode.DownArrow;
            bool up_p = e.keyCode == KeyCode.UpArrow;
            bool left_p = e.keyCode == KeyCode.LeftArrow;
            bool right_p = e.keyCode == KeyCode.RightArrow;
            delete_pressed = e.keyCode == KeyCode.Backspace;
            enter_pressed = e.keyCode == KeyCode.Return;

            if (down_p || up_p || left_p || right_p || delete_pressed || enter_pressed) e.Use();    
            
            L element_to_select = null;

            int c = selected_elements.Count;
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


            if (right_p) {
                if (NextPage(all_paths)) {}
            }
            if (left_p) {
                if (PreviousPage(all_paths)) {}
                else {
                    if (foldered) {
                        if (folderView.MoveBackward()) {
                            OnFolderViewChange(all_paths);
                        }
                    }
                }
            }      

            if (selected_elements.Count != 0) {
                
                if (enter_pressed) {
                    if (SelectionHasDirectories()) {
                        if (selected_elements.Count == 1) {
                            foreach (var s in selected_elements) MoveForwardFolder(s.fullName, all_paths);
                        }
                        enter_pressed = false;
                    }
                }
            } 
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
                if (!was_selected) {
                    selected_elements.Add(selection);
                }
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

        
        void OpenImportSettings () {
            Animations.EditImportSettings.CreateWizard(new Object[selected_elements.Count].Generate(selected_elements, e => { return AssetDatabase.LoadAssetAtPath(pack.objectsDirectory + e.file_path, typeof(Object)); } ));
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


            Object[] objs = new Object[selected_elements.Count].Generate(selected_elements, s => { return GetObjectRefForElement(s); } );

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
    }
}



