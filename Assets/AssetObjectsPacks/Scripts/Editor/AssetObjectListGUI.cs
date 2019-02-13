using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;

namespace AssetObjectsPacks {
    public class AssetObjectListGUI {
        int selection_view;
        string[] no_ids;
        string[] all_file_paths;
        GUIContent[] tab_guis = new GUIContent[] { new GUIContent("List"), new GUIContent("Explorer") };
        AssetObjectListView list_view = new AssetObjectListView();
        AssetObjectExplorerView explorer_view = new AssetObjectExplorerView();
        AssetObjectPack pack;

        delegate bool DrawMethod(string[] all_paths);
        DrawMethod[] draw_methods;
        Action<Rect, GUIStyle>[] on_interactive_previews;
        Action<string[]>[] initialize_views;
        Action[] on_preview_settings;



        public void RealOnEnable (SerializedObject eventPack) {
            this.eventPack = eventPack;
            draw_methods = new DrawMethod[] { list_view.Draw, explorer_view.Draw };
            on_interactive_previews = new Action<Rect, GUIStyle>[] { list_view.OnInteractivePreviewGUI, explorer_view.OnInteractivePreviewGUI };
            on_preview_settings = new Action[] { list_view.OnPreviewSettings, explorer_view.OnPreviewSettings };
            initialize_views = new Action<string[]>[] { list_view.InitializeView, explorer_view.InitializeView };
            explorer_view.RealOnEnable(eventPack);
            list_view.RealOnEnable(eventPack);
        }

        public void InitializeWithPack (AssetObjectPack pack) {
            this.pack = pack;
            if (pack == null) {
                current_pack_explorer_valid = false;
                pack_help_string = "Please Choose an Asset Object Pack";
                return;
            }
            current_pack_explorer_valid = pack.assetType.IsValidTypeString() && pack.objectsDirectory.IsValidDirectory();
            if (!current_pack_explorer_valid) {
                if (!pack.assetType.IsValidTypeString()) pack_help_string = pack.name + " pack doesnt have a valid asset type to target!";
                else if (!pack.objectsDirectory.IsValidDirectory()) pack_help_string = pack.name + " pack doesnt have a valid object directory!";
                return;
            }
            pack_help_string = "";
            
            UpdateListParametersToReflectPack(eventPack.FindProperty(AssetObjectEventPack.asset_objs_field), pack);
            
            Dictionary<int, string> id2path;
            all_file_paths = GetAllFilePaths(out id2path);
            
            InitializeIDsPrompt(pack);
            explorer_view.InitializeWithPack(pack);
            list_view.InitializeWithPack(pack, id2path);
            
            initialize_views[selection_view](all_file_paths);
            eventPack.ApplyModifiedProperties();
        }


        

        SerializedObject eventPack;

        void UpdateListParametersToReflectPack (SerializedProperty ao_list, AssetObjectPack pack) {
            int c = ao_list.arraySize;
            AssetObjectParamDef[] defs = pack.defaultParams;
            for (int i = 0; i < c; i++) SerializedPropertyUtils.Parameters.UpdateParametersToReflectDefaults(ao_list.GetArrayElementAtIndex(i).FindPropertyRelative(AssetObject.params_field), defs);
        }
     

        public bool HasPreviewGUI() { return current_pack_explorer_valid; }

        public void OnInteractivePreviewGUI(Rect r, GUIStyle background) { 
            if (!current_pack_explorer_valid) return;
            on_interactive_previews[selection_view](r, background); 
        }
        public void OnPreviewSettings() { 
            if (!current_pack_explorer_valid) return;
            on_preview_settings[selection_view] (); 
        }

        bool ids_prompt_dismissed;
        string ids_help_msg, sure_msg;

        void InitializeIDsPrompt (AssetObjectPack pack) {
            int l = no_ids.Length;
            if (l == 0) {
                ids_help_msg = sure_msg = "";
                return;
            }
            ids_help_msg = "\n"+ l + " [" + pack.fileExtensions + "] file(s) without proper IDs in the pack directory.\n\n\n" + pack.objectsDirectory + "\n";
            sure_msg = "Generating IDs will rename the following assets:\n";
            for (int i = 0; i < l; i++) sure_msg += "\n" + no_ids[i] + "\n";
        }




        const string sGenerateIDs = "Generate IDs", sCancel = "Cancel";
        GUIContent generate_ids_gui = new GUIContent(sGenerateIDs);
        GUIContent dismiss_gui = new GUIContent("Dismiss");
            

        void DrawObjectsWithoutIDsPrompt (int buffer_space) {
            if (ids_prompt_dismissed) return;
            
            GUIUtils.Space(buffer_space);
            EditorGUILayout.HelpBox(ids_help_msg, MessageType.Warning);
            
            EditorGUILayout.BeginHorizontal();
            if (GUIUtils.Button(generate_ids_gui, false, EditorColors.green_color, EditorColors.black_color)) {
                if (EditorUtility.DisplayDialog(sGenerateIDs, sure_msg, sGenerateIDs, sCancel)) {

                    AssetObjectsEditor.GenerateNewIDs(AssetObjectsEditor.GetAllAssetObjectPaths (pack.objectsDirectory, pack.fileExtensions, false), no_ids);
                    Dictionary<int, string> id2path;
                    all_file_paths = GetAllFilePaths(out id2path);
                    //explorer_view.ReinitializeAssetObjectReferences(all_file_paths);
                    list_view.id2path = id2path;
                    initialize_views[selection_view](all_file_paths);
                }
            }
            if (GUIUtils.Button(dismiss_gui, false)) ids_prompt_dismissed = true;
            EditorGUILayout.EndHorizontal();
            
            GUIUtils.Space(buffer_space);       
        }
                
        string[] GetAllFilePaths (out Dictionary<int, string> id2path){
            return AssetObjectsEditor.GetAllAssetObjectPaths (pack.objectsDirectory, pack.fileExtensions, false, out no_ids, out id2path);
        }

        bool current_pack_explorer_valid;
        string pack_help_string = "";

        
        
        
        public bool Draw (){
            if (!current_pack_explorer_valid) {
                EditorGUILayout.HelpBox(pack_help_string, MessageType.Error);
                return false;
            }
                    
            if (no_ids.Length != 0) DrawObjectsWithoutIDsPrompt(2);

            bool changed;
            selection_view = GUIUtils.Tabs(tab_guis, selection_view, out changed);
            if (changed) initialize_views[selection_view](all_file_paths);

            return draw_methods[selection_view](all_file_paths);
        }   
    }
}