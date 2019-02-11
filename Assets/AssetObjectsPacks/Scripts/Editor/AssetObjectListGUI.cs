using UnityEngine;
using UnityEditor;
using System;

namespace AssetObjectsPacks {
    public class AssetObjectListGUI {
        int selection_view;
        string[] no_ids;
        GUIContent[] tab_guis;
        AssetObjectListView list_view = new AssetObjectListView();
        AssetObjectExplorerView explorer_view = new AssetObjectExplorerView();
        AssetObjectPack pack;

        delegate bool DrawMethod();
        DrawMethod[] draw_methods;
        Action<Rect, GUIStyle>[] on_interactive_previews;
        Action[] on_preview_settings, initialize_views;

        public void OnDisable() {
            explorer_view.OnDisable();
        }
        public void OnEnable(SerializedObject eventPack, AssetObjectPack pack){
            this.pack = pack;
            list_view.OnEnable(eventPack, pack);
            explorer_view.OnEnable(eventPack, pack, GetAllFilePaths());

            tab_guis = new GUIContent[] { new GUIContent("List: " + pack.name), new GUIContent("Explorer") };

            draw_methods = new DrawMethod[] { list_view.Draw, explorer_view.Draw };
            on_interactive_previews = new Action<Rect, GUIStyle>[] { list_view.OnInteractivePreviewGUI, explorer_view.OnInteractivePreviewGUI };
            on_preview_settings = new Action[] { list_view.OnPreviewSettings, explorer_view.OnPreviewSettings };
            initialize_views = new Action[] { list_view.InitializeView, explorer_view.InitializeView };
            
            initialize_views[selection_view]();
        }
        public void OnInteractivePreviewGUI(Rect r, GUIStyle background) {
            on_interactive_previews[selection_view](r, background);
        }
        public void OnPreviewSettings() {
            on_preview_settings[selection_view] ();
        }
        void DrawObjectsWithoutIDsPrompt () {
            int l = no_ids.Length;
            if (l == 0) return;
            EditorGUILayout.HelpBox("There are " + l + " [" + string.Join(",", pack.fileExtensions) + "] files without proper IDs.", MessageType.Warning);
            if (GUILayout.Button("Generate IDs")) {
                AssetObjectsEditor.GenerateNewIDs(explorer_view.all_file_paths, no_ids);
                explorer_view.ReinitializePaths(GetAllFilePaths());
            }
        }
        string[] GetAllFilePaths (){
            return AssetObjectsEditor.GetAllAssetObjectPaths (pack.objectsDirectory, pack.fileExtensions, false, out no_ids);
        }
        public bool Draw (){
            DrawObjectsWithoutIDsPrompt();

            int last_selection_view = selection_view;
            selection_view = GUIUtils.Tabs(tab_guis, selection_view);
            if (selection_view != last_selection_view) initialize_views[selection_view]();

            return draw_methods[selection_view]();
        }   
    }
}