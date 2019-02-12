using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;

namespace AssetObjectsPacks {
    public class AssetObjectListGUI {
        int selection_view;
        string[] no_ids;
        GUIContent[] tab_guis = new GUIContent[] { new GUIContent("List"), new GUIContent("Explorer") };
        AssetObjectListView list_view = new AssetObjectListView();
        AssetObjectExplorerView explorer_view = new AssetObjectExplorerView();
        AssetObjectPack pack;

        delegate bool DrawMethod();
        DrawMethod[] draw_methods;
        Action<Rect, GUIStyle>[] on_interactive_previews;
        Action[] on_preview_settings, initialize_views;

        //public void OnDisable() {
        //    explorer_view.OnDisable();
        //}

        public void RealOnEnable (SerializedObject eventPack) {
            this.eventPack = eventPack;
            draw_methods = new DrawMethod[] { list_view.Draw, explorer_view.Draw };
            on_interactive_previews = new Action<Rect, GUIStyle>[] { list_view.OnInteractivePreviewGUI, explorer_view.OnInteractivePreviewGUI };
            on_preview_settings = new Action[] { list_view.OnPreviewSettings, explorer_view.OnPreviewSettings };
            initialize_views = new Action[] { list_view.InitializeView, explorer_view.InitializeView };

            explorer_view.RealOnEnable(eventPack);
            list_view.RealOnEnable(eventPack);
        }



        void InitializeWithPack (AssetObjectPack pack) {
            this.pack = pack;

            UpdateListParametersToReflectPack(eventPack.FindProperty(AssetObjectEventPack.asset_objs_field), pack);
            Dictionary<int, string> id2path;
            string[] all_file_paths = GetAllFilePaths(out id2path);
            InitializeIDsPrompt(pack);

            explorer_view.InitializeWithPack(pack, all_file_paths);
            list_view.InitializeWithPack(pack, id2path);

        }


        

        SerializedObject eventPack;


        public void OnEnable(SerializedObject eventPack, AssetObjectPack pack){
            RealOnEnable(eventPack);

            InitializeWithPack(pack);
            initialize_views[selection_view]();
        }

        void UpdateListParametersToReflectPack (SerializedProperty ao_list, AssetObjectPack pack) {
            int c = ao_list.arraySize;
            AssetObjectParamDef[] defs = pack.defaultParams;
            for (int i = 0; i < c; i++) SerializedPropertyUtils.Parameters.UpdateParametersToReflectDefaults(ao_list.GetArrayElementAtIndex(i).FindPropertyRelative(AssetObject.params_field), defs);
        }


        public void OnInteractivePreviewGUI(Rect r, GUIStyle background) { on_interactive_previews[selection_view](r, background); }
        public void OnPreviewSettings() { on_preview_settings[selection_view] (); }

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

                    AssetObjectsEditor.GenerateNewIDs(explorer_view.all_file_paths, no_ids);
                    Dictionary<int, string> id2path;
                    explorer_view.ReinitializeAssetObjectReferences(GetAllFilePaths(out id2path));
                    list_view.id2path = id2path;
                    initialize_views[selection_view]();
                }
            }
            if (GUIUtils.Button(dismiss_gui, false)) ids_prompt_dismissed = true;
            EditorGUILayout.EndHorizontal();
            
            GUIUtils.Space(buffer_space);       
        }
                
        string[] GetAllFilePaths (out Dictionary<int, string> id2path){
            return AssetObjectsEditor.GetAllAssetObjectPaths (pack.objectsDirectory, pack.fileExtensions, false, out no_ids, out id2path);
        }
        public bool Draw (){
            if (no_ids.Length != 0) DrawObjectsWithoutIDsPrompt(2);

            bool changed;
            selection_view = GUIUtils.Tabs(tab_guis, selection_view, out changed);
            if (changed) initialize_views[selection_view]();

            return draw_methods[selection_view]();
        }   
    }
}