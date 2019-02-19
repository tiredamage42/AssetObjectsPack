using UnityEngine;
using UnityEditor;
namespace AssetObjectsPacks {
    public static class ViewToolbarGUI {
        public static bool folderedView;
        public static int listProjectView;
        static readonly GUIContent foldersViewGUI = new GUIContent("Folders", "Enable/Disable Folders View");
        
        static readonly GUIContent[] tab_guis = new GUIContent[] { 
            new GUIContent("Event Pack"), 
            new GUIContent("Project") 
        };

        public static void DrawToolbar (out bool changedTabView, out bool folderBack, out bool changedFolderedView, GUIContent folderBackGUI, GUIStyle buttonStyle, int foldersHiearchyOffset) {
            GUIUtils.StartBox(0);

            changedTabView = GUIUtils.Tabs(tab_guis, ref listProjectView);

            EditorGUILayout.BeginHorizontal();

            //folder back
            folderBack = folderedView && foldersHiearchyOffset > 0 && GUIUtils.Button(folderBackGUI, true, buttonStyle);
            
            //folder / list view
            folderedView = GUIUtils.ToggleButton(foldersViewGUI, true, folderedView, buttonStyle, out changedFolderedView);
            
            EditorGUILayout.EndHorizontal();
            
            GUIUtils.EndBox(0);
        }
    }
}
