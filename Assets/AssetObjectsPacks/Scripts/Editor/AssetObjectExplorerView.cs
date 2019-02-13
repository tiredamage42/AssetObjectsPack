using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
namespace AssetObjectsPacks {
    public class AssetObjectExplorerView : SelectionView<ListViewElement>
    {
        HiddenView hiddenView = new HiddenView();
        GUIContent add2setGUI = new GUIContent("+", "Add To Set");
        GUIContent hide_gui = new GUIContent("H", "Hide");
        GUIContent unhide_gui = new GUIContent("U", "Unhide");
        GUIContent add_selected_gui = new GUIContent("Add Selected");   
        protected HashSet<int> ids_in_set = new HashSet<int>();
        const string hidden_suffix = " [HIDDEN]";
        
        public override void InitializeView (string[] all_paths) {
            ids_in_set = ids_in_set.Generate(ao_list.arraySize, i => { return GetObjectIDAtIndex(i); }); 
            base.InitializeView(all_paths);
        }

        void OnAddIDsToSet (HashSet<ListViewElement> elements, string[] all_paths) {
            if (elements.Count == 0) return;
            hiddenView.UnhideIDs( IDSetFromElements(elements) );
            bool reset_i = true;
            foreach (ListViewElement e in elements) {
                ids_in_set.Add(e.object_id);
                AddNewAssetObject(e.object_id, GetObjectRefForElement(e), e.file_path, reset_i);
                reset_i = false;
            }
            ClearSelectionsAndRebuild(all_paths);
        }

        void AddNewAssetObject (int obj_id, Object obj_ref, string file_path, bool make_default) {
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

        public bool Draw (string[] all_paths) {
            bool changed = false;
            bool enter_pressed, delete_pressed;
            KeyboardInput(all_paths, out enter_pressed, out delete_pressed);

            if (selected_elements.Count != 0) {
                if (delete_pressed) {
                    if (!SelectionHasDirectories()) {
                        if (hiddenView.ToggleHiddenSelected(IDSetFromElements( selected_elements ), false, true)) {
                            ClearSelectionsAndRebuild(all_paths);
                            changed = true;
                        }
                    }
                }
                if (enter_pressed) {
                    OnAddIDsToSet(selected_elements, all_paths);
                    changed = true;
                }
            }

            DrawToolbar(all_paths);
            DrawElements(all_paths);
            DrawPaginationGUI(all_paths);
            return changed;
        }

        protected override void DrawNonFolderElement(string[] all_paths, ListViewElement element, bool selected, int index) {
        
            EditorGUILayout.BeginHorizontal();

            bool is_hidden = hiddenView.IsHidden(element.object_id);
            
            bool add_button_pressed = GUIUtils.SmallButton(add2setGUI, EditorColors.green_color, EditorColors.black_color);
            bool hide_button_pressed = GUIUtils.SmallToggleButton(is_hidden ? unhide_gui : hide_gui, is_hidden) != is_hidden;
            bool obj_selected = GUIUtils.ScrollWindowElement (element.label_gui, selected, is_hidden, false);
            
            if (obj_selected) {
                OnObjectSelection(element, selected);
            }
            if (add_button_pressed) {
                OnAddIDsToSet(new HashSet<ListViewElement>() {element}, all_paths);
            }
            if (hide_button_pressed) {
                if (hiddenView.ToggleHiddenSelected(IDSetFromElements(new HashSet<ListViewElement>() {element} ), is_hidden, false)) {
                    ClearSelectionsAndRebuild(all_paths);
                }
            }
            
            EditorGUILayout.EndHorizontal();
        }

        protected override void ExtraToolbarButtons(string[] all_paths, bool has_selection, bool selection_has_directories) {
            GUI.enabled = has_selection && !selection_has_directories;            
            
            if (GUIUtils.Button(add_selected_gui, true)) OnAddIDsToSet(selected_elements, all_paths);
            
            bool toggled_hidden_selected = hiddenView.ToggleHiddenSelectedButton( IDSetFromElements( selected_elements ) );
            
            GUI.enabled = true;
            
            bool toggled_show_hidden = hiddenView.ToggleShowHiddenButton();
            
            bool reset_hidden = hiddenView.ResetHiddenButton();
            
            if (toggled_hidden_selected || toggled_show_hidden || reset_hidden) ClearSelectionsAndRebuild(all_paths);
        }

        protected override List<ListViewElement> UnpaginatedFoldered(string[] all_paths) {
            HashSet<string> usedNames = new HashSet<string>();            
            List<ListViewElement> unpaginated = new List<ListViewElement>();
            
            int lastDir = 0;
            int c = all_paths.Length;
            for (int i = 0; i < c; i++) {
            
                string file_path = all_paths[i];
                int id = AssetObjectsEditor.GetObjectIDFromPath(file_path);
                
                if (!folderView.DisplaysPath(file_path)) continue;
                if (ids_in_set.Contains(id)) continue;

                bool is_hidden = hiddenView.IsHidden(id);

                if (!hiddenView.showHidden && is_hidden) continue;
                
                string name_display = folderView.DisplayNameFromPath(file_path);
                if (usedNames.Contains(name_display)) continue;
                usedNames.Add(name_display);

                if (name_display.Contains(".")) { 
                    string name_string = AssetObjectsEditor.RemoveIDFromPath(EditorUtils.RemoveDirectory(file_path));

                    if (is_hidden) name_string += hidden_suffix;

                    GUIContent label_gui = new GUIContent( name_string );// ao.label_gui;
                    unpaginated.Add(new ListViewElement(file_path, name_display, label_gui, id));
                    continue;
                }

                //is directory
                unpaginated.Insert(lastDir, new ListViewElement(file_path, name_display, new GUIContent(name_display), -1));
                lastDir++;   
            } 
            return unpaginated;
        }

        protected override List<ListViewElement> UnpaginatedListed(string[] all_paths) {
            int c = all_paths.Length;
            
            List<ListViewElement> unpaginated = new List<ListViewElement>();
            for (int i = 0; i < c; i++) {
                string file_path = all_paths[i];
                int id = AssetObjectsEditor.GetObjectIDFromPath(file_path);
                
                if (ids_in_set.Contains(id)) continue;
                if (!hiddenView.showHidden && hiddenView.IsHidden(id)) continue;
                
                string n = AssetObjectsEditor.RemoveIDFromPath(file_path);
                unpaginated.Add(new ListViewElement(file_path, file_path, new GUIContent(n), id));
            }
            return unpaginated;
        }        
    }
}
