
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace AssetObjectsPacks {
    public class AssetObjectListGUI {

        int selection_view;
        string[] paths_without_ids, all_valid_paths;
        string file_extension, pack_name;
        GUIContent[] tab_guis;
        public AssetObjectListView list_view = new AssetObjectListView();
        AssetObjectExplorerView explorer_view = new AssetObjectExplorerView();

        public void OnEnable(
            string pack_name, string asset_object_unity_asset_type, string file_extension, 
            //System.Action<SerializedProperty> make_instance_default, 
            AssetObjectParamDef[] default_params,
            //string[] instance_field_names, GUIContent[] instance_field_labels,
            SerializedObject serializedObject, SerializedObject editor_so
        ) {
            this.file_extension = file_extension;
            this.pack_name = pack_name;

            Dictionary<int, string> id2path;
            string[] all_file_paths = GetAllFilePaths(out id2path);
            list_view.OnEnable(serializedObject, editor_so, asset_object_unity_asset_type, pack_name, id2path, default_params);//, make_instance_default, default_params);//, instance_field_names, instance_field_labels);
            explorer_view.OnEnable(serializedObject, asset_object_unity_asset_type, pack_name, id2path, default_params, all_file_paths);//, make_instance_default, all_file_paths);

            tab_guis = new GUIContent[] { new GUIContent("List: " + pack_name), new GUIContent("Explorer") };
            list_view.InitializeView();
        }
         public void OnInteractivePreviewGUI(Rect r, GUIStyle background) {
             if (selection_view == 0) {
                 list_view.OnInteractivePreviewGUI(r, background);
             }
             else {
                 explorer_view.OnInteractivePreviewGUI(r, background);

             }
         }
          public void OnPreviewSettings() {
            if (selection_view == 0) {
                 list_view.OnPreviewSettings();
             }
             else {
                 explorer_view.OnPreviewSettings();

             }
        }
        public  bool HasPreviewGUI() { 
            if (selection_view == 0) {
                 return list_view.HasPreviewGUI();
             }
             else {
                 return explorer_view.HasPreviewGUI();

             }
        }

        
        void DrawObjectsWithoutIDsPrompt () {
            int l = paths_without_ids.Length;
            if (l == 0) {
                return;
            }
            EditorGUILayout.HelpBox("There are " + l + " " + file_extension + " files without proper IDs.", MessageType.Warning);
            if (GUILayout.Button("Generate IDs")) {
                AssetObjectsEditor.GenerateNewIDs(all_valid_paths, paths_without_ids);
                all_valid_paths = new string[0];
                paths_without_ids = new string[0];


                Dictionary<int, string> id2path;
                string[] all_file_paths = GetAllFilePaths(out id2path);
                list_view.ReinitializePaths(id2path);
                explorer_view.ReinitializePaths(all_file_paths, id2path);

            }
        }

        string[] GetAllFilePaths (out Dictionary<int, string> id2path) {
            string[] all_paths = AssetObjectsEditor.GetAllAssetObjectPaths (pack_name, file_extension, false, out paths_without_ids, out id2path);
            if (paths_without_ids.Length != 0) 
                all_valid_paths = all_paths;
            return all_paths;
        }
        
        public void Draw (int scroll_view_height){
            DrawObjectsWithoutIDsPrompt();

            int last_selection_view = selection_view;
            selection_view = GUIUtils.Tabs(tab_guis, selection_view);
            if (selection_view != last_selection_view) {
                if (selection_view == 0) 
                    list_view.InitializeView();
                else 
                    explorer_view.InitializeView();
            }
            if (selection_view == 0) {
                list_view.Draw(scroll_view_height);
            }
            else {
                explorer_view.Draw(scroll_view_height);
            }
        }
    }
}


        
