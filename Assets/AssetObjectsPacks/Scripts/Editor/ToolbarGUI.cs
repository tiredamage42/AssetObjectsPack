using UnityEditor;
using UnityEngine;
namespace AssetObjectsPacks {
    public static class ToolbarGUI {
        static readonly GUIContent[] tab_guis = new GUIContent[] { 
            new GUIContent("Event Pack"), 
            new GUIContent("Project") 
        };
        //static readonly GUIContent helpGUI = new GUIContent("Help", "Show Help");
        static readonly GUIContent foldersViewGUI = new GUIContent("Folders", "Enable/Disable Folders View");
        static readonly GUIContent showHiddenGUI = new GUIContent("Show Hidden", "Show 'Hidden' Elements");
        static readonly GUIContent resetHiddenGUI = new GUIContent("Reset Hidden", "Unhide All");
        static readonly GUIContent hideUnhideSelectedGUI = new GUIContent("Hide/Unhide", "Toggle the hidden status of the selection (if any)");
        static readonly GUIContent addRemoveSelectedGUI = new GUIContent("Add/Remove", "Add Selected (When in project view)\nRemove Selected (When in event view)");
        static readonly GUIContent importSettingsGUI = new GUIContent("Import Settings", "Open import settings on selection (if any)");
        static readonly GUIContent searchGUI = new GUIContent("Search:");
        static readonly GUIContent folderBackGUI = new GUIContent("   <<   ", "Back");
        
        const string curDirString = "<b>Current Directory: </b><i>{0}</i>";
        
        public static bool showHidden, folderedView;
        public static int listProjectView;
        static string searchFilter = "";
        static bool showHelp;
        
        public static bool PathPassesSearchFilter(string path) {
            if (searchFilter.IsEmpty()) return true;
            return path.ToLower().Contains(searchFilter.ToLower());
        }

        public static void DrawToolbar (
            bool validSelection, 
            string curDirectory,
            int foldersHiearchyOffset,
            out bool importSettings, 
            out bool removeOrAdd, 
            out bool searchChanged,
            out bool resetHidden, 
            out bool toggleHidden, 
            out bool showHiddenToggled, 
            out bool changedTabView, 
            out bool folderBack, 
            out bool changedFolderedView
        ) {

            GUIUtils.StartBox(0);
            changedTabView = GUIUtils.Tabs(tab_guis, ref listProjectView);
            GUIUtils.Space();

            EditorGUILayout.BeginHorizontal();

            GUI.enabled = validSelection;
            importSettings = ImportSettingsButton(GUIStyles.miniButtonLeft);
            removeOrAdd = AddRemoveButton(GUIStyles.miniButtonMid);
            toggleHidden = ToggleHiddenButton(GUIStyles.miniButtonMid);
            GUI.enabled = true;

            showHiddenToggled = ShowHiddenButton(GUIStyles.miniButtonMid);
            resetHidden = ResetHiddenButton(GUIStyles.miniButtonMid);            
            changedFolderedView = FolderedViewButton(GUIStyles.miniButtonRight);
            //bool showHelp = HelpButton(GUIStyles.miniButton);

            EditorGUILayout.EndHorizontal();

            //if (showHelp) {
            //    HelpWindow.Init();

                //HelpGUI.DrawToolBarHelp();
            //}

            searchChanged = SearchBar();
            
            folderBack = folderedView && DirectoryButtons (GUIStyles.miniButtonLeft, curDirectory, foldersHiearchyOffset);

            GUIUtils.EndBox(1);
        }

        static bool DirectoryButtons (GUIStyle s, string curDirectory, int foldersHiearchyOffset) {   
            EditorGUILayout.BeginHorizontal();
            GUI.enabled = foldersHiearchyOffset > 0;
            bool folderBack = GUIUtils.Button(folderBackGUI, true, s);
            GUI.enabled = true;
            GUIUtils.Label(new GUIContent(string.Format(curDirString, curDirectory)), true);
            EditorGUILayout.EndHorizontal();

            return folderBack;
        }
        public static bool SearchBar () {
            EditorGUILayout.BeginHorizontal();
            GUIUtils.Label(searchGUI, true);
            string lastSearch = searchFilter;
            GUIUtils.NextControlOverridesKeyboard();
            searchFilter = EditorGUILayout.DelayedTextField(GUIContent.none, searchFilter);
            //if clicked outside, lose focus
            GUIUtils.CheckLoseFocusLastRect();
            EditorGUILayout.EndHorizontal();
            return searchFilter != lastSearch;
        }
        static bool ImportSettingsButton (GUIStyle s) {
            return GUIUtils.Button(importSettingsGUI, false, s);
        }
        //static bool HelpButton (GUIStyle s) {
        //    return GUIUtils.Button(helpGUI, false, s);
        //}
        static bool ResetHiddenButton (GUIStyle s) {
            return GUIUtils.Button(resetHiddenGUI, false, s);
        }
        static bool ToggleHiddenButton (GUIStyle s) {
            return GUIUtils.Button(hideUnhideSelectedGUI, false, s);
        }
        static bool AddRemoveButton (GUIStyle s) {
            return GUIUtils.Button(addRemoveSelectedGUI, false, s);
        }
        static bool ShowHiddenButton (GUIStyle s) {
            bool toggled;
            showHidden = GUIUtils.ToggleButton(showHiddenGUI, false, showHidden, s, out toggled);
            return toggled;
        }
        static bool FolderedViewButton (GUIStyle s) {
            bool toggled;
            folderedView = GUIUtils.ToggleButton(foldersViewGUI, false, folderedView, s, out toggled);
            return toggled;
        }   
    }   
}