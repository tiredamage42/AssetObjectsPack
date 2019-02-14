using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
namespace AssetObjectsPacks {
    public class AssetObjectExplorerView : SelectionView<ListViewElement>
    {
        //GUIContent add2setGUI = new GUIContent("+", "Add To Set");
        //GUIContent add_selected_gui = new GUIContent("   Add   ", "Add Selected");   
        //protected HashSet<int> ids_in_set = new HashSet<int>();
        //GUIContent hide_gui = new GUIContent("H", "Hide");
        //GUIContent unhide_gui = new GUIContent("U", "Unhide");
        //const string hidden_suffix = " [HIDDEN]";

        
        
        //public override void InitializeView (string[] all_paths, HashSet<ListViewElement> selection) {
        //    ids_in_set = ids_in_set.Generate(ao_list.arraySize, i => { return GetObjectIDAtIndex(i); }); 
        //    base.InitializeView(all_paths, selection);
        //}
/*
        void AddElementsToSet (HashSet<ListViewElement> elements_to_add, string[] all_paths, HashSet<ListViewElement> selection) {
            if (elements_to_add.Count == 0) return;
            //hiddenView.UnhideIDs( IDSetFromElements(elements_to_add) );
            bool reset_i = true;
            foreach (ListViewElement e in elements_to_add) {
                ids_in_set.Add(e.object_id);
                AddNewAssetObject(e.object_id, GetObjectRefForElement(e), e.file_path, reset_i);
                reset_i = false;
            }
            ClearSelectionsAndRebuild(all_paths, selection);
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




//explorer draw
        public bool Draw (string[] all_paths, HashSet<ListViewElement> selection) {
            bool changed = false;
            bool enter_pressed, delete_pressed;
            KeyboardInput(all_paths, out enter_pressed, out delete_pressed, selection);

            if (selection.Count != 0) {
                if (enter_pressed) {
                    AddElementsToSet(selection, all_paths, selection);
                    changed = true;
                }
            }

            DrawToolbar(all_paths, selection);
            DrawElements(all_paths, selection);
            DrawPaginationGUI(all_paths, selection);
            return changed;
        }
 */

/*
        protected override void PreSelectButton(int index) {
        }

        protected override void NonFolderSecondTier(string[] all_paths, ListViewElement element, int index) {
                
        }
        protected override void PostSelectButton (ListViewElement element, int index) {

        }

 */

/*
        protected override bool DrawNonFolderElement(ListViewElement element, int index, bool selected, bool hidden, GUILayoutOption element_width) {
        
        
            EditorGUILayout.BeginHorizontal();

            
            bool selected_element = GUIUtils.ScrollWindowElement (element.label_gui, selected, hidden, false, element_width);
            
            
            EditorGUILayout.EndHorizontal();

            return selected_element;

            

        }
         

        protected override void OnPagination()
        {
            max_name_width = GUILayout.ExpandWidth(true);
        }


        protected override void DrawNonFolderElement(string[] all_paths, ListViewElement element, bool selected, bool is_hidden, int index) {
        
            EditorGUILayout.BeginHorizontal();

            
            //bool is_hidden = hiddenView.IsHidden(element.object_id);
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
        protected override void ExtraToolbarButtons(string[] all_paths, bool has_selection, bool selection_has_directories, HashSet<ListViewElement> selection) {
            GUI.enabled = has_selection && !selection_has_directories;            
            
            if (GUIUtils.Button(add_selected_gui, true, EditorStyles.miniButton)) AddElementsToSet(selection, all_paths, selection);
            
            //bool toggled_hidden_selected = hiddenView.ToggleHiddenSelectedButton( IDSetFromElements( selected_elements ) );
            
            GUI.enabled = true;
            
            //bool toggled_show_hidden = hiddenView.ToggleShowHiddenButton();
            
            //bool reset_hidden = hiddenView.ResetHiddenButton();
            
            //if (toggled_hidden_selected || toggled_show_hidden || reset_hidden) ClearSelectionsAndRebuild(all_paths);
        }
 */
/*


        protected override List<ListViewElement> UnpaginatedFoldered(string[] all_paths) {
            HashSet<string> usedNames = new HashSet<string>();            
            List<ListViewElement> unpaginated = new List<ListViewElement>();

            HashSet<int> ids_in_set = new HashSet<int>().Generate(ao_list.arraySize, i => { return GetObjectIDAtIndex(i); }); 
            
            
            int lastDir = 0;
            int c = all_paths.Length;
            for (int i = 0; i < c; i++) {
            
                string file_path = all_paths[i];
                int id = AssetObjectsEditor.GetObjectIDFromPath(file_path);
                
                if (ids_in_set.Contains(id)) continue;

                bool isDirectory;
                GUIContent gui;
                if (ElementPassedFolderedView (id, file_path, ref usedNames, out gui, out isDirectory)) {
                    if (!isDirectory) {
                        unpaginated.Add(new ListViewElement(file_path, gui.text, gui, id, null));
                        continue;
                    }
                    //is directory
                    unpaginated.Insert(lastDir, new ListViewElement(file_path, gui.text, gui, -1, null));
                    lastDir++;   
                }
            } 
            return unpaginated;
        }

        protected override List<ListViewElement> UnpaginatedListed(string[] all_paths) {

            HashSet<int> ids_in_set = new HashSet<int>().Generate(ao_list.arraySize, i => { return GetObjectIDAtIndex(i); }); 
            
            int c = all_paths.Length;
            
            List<ListViewElement> unpaginated = new List<ListViewElement>();
            for (int i = 0; i < c; i++) {
                string file_path = all_paths[i];
                int id = AssetObjectsEditor.GetObjectIDFromPath(file_path);
                
                if (ids_in_set.Contains(id)) continue;

                GUIContent gui;
                if (ElementPassedListedView(id, file_path, out gui)) {

                    unpaginated.Add(new ListViewElement(file_path, file_path, gui, id, null));
                }
            }
            return unpaginated;
        }        
 */
    }
}
