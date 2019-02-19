using UnityEditor;
using UnityEngine;
namespace AssetObjectsPacks {
    public static class ToolbarGUI {
        static readonly GUIContent addSelectedGUI = new GUIContent("   Add   ", "Add Selected");
        static readonly GUIContent removeSelectedGUI = new GUIContent("Remove", "Remove Selected");
        static readonly GUIContent showHiddenGUI = new GUIContent("Show Hidden", "Show 'Hidden' Elements");
        static readonly GUIContent resetHiddenGUI = new GUIContent("Reset Hidden", "Unhide All");
        static readonly GUIContent hideUnhideSelectedGUI = new GUIContent("Hide/Unhide", "Toggle the hidden status of the selection");
        static readonly GUIContent importSettingsGUI = new GUIContent("Import Settings", "Open import settings");
        static readonly GUIContent helpGUI = new GUIContent("Help", "Show Help");
        static readonly GUIContent searchGUI = new GUIContent("Search:");
        static readonly GUILayoutOption searchWidth = GUILayout.MaxWidth(345);
        static string searchFilter = "";
        public static bool showHidden;
        static bool showHelp;

        public static bool PathPassesSearchFilter(string path) {
            if (searchFilter.IsEmpty()) return true;
            return path.ToLower().Contains(searchFilter.ToLower());
        }

        public static void DrawToolbar (bool drawingListView, GUIStyle buttonStyle, bool validSelection, out bool importSettings, out bool toggleHidden, out bool showHiddenToggled, out bool resetHidden, out bool removeOrAdd, out bool searchChanged) {

            GUIUtils.StartBox(0);

            if (showHelp) HelpGUI.DrawToolBarHelp(2);

            GUIUtils.Space();

            EditorGUILayout.BeginHorizontal();

            //help
            showHelp = GUIUtils.ToggleButton(helpGUI, true, showHelp, buttonStyle, out _);
            
            GUI.enabled = validSelection;

            //import settings
            importSettings = GUIUtils.Button(importSettingsGUI, true, buttonStyle);
            
            //hide / unhide
            toggleHidden = GUIUtils.Button(hideUnhideSelectedGUI, true, buttonStyle);
            
            GUI.enabled = true;
            
            //show hidden
            showHidden = GUIUtils.ToggleButton(showHiddenGUI, true, showHidden, buttonStyle, out showHiddenToggled);
            
            //reset hidden
            resetHidden = GUIUtils.Button(resetHiddenGUI, true, buttonStyle);

            GUI.enabled = validSelection;
            //remove or add (list)
            removeOrAdd  = GUIUtils.Button(drawingListView ? removeSelectedGUI : addSelectedGUI, true, buttonStyle);
            GUI.enabled = true;
            
            EditorGUILayout.EndHorizontal();
            
            GUIUtils.Space();

            //search bar
            EditorGUILayout.BeginHorizontal();
            
            GUIUtils.Label(searchGUI, true);

            string lastSearch = searchFilter;
            GUIUtils.NextControlOverridesKeyboard();
            searchFilter = EditorGUILayout.DelayedTextField(GUIContent.none, searchFilter, searchWidth);
            
            //if clicked outside, lose focus
            GUIUtils.CheckLoseFocusLastRect();
            
            EditorGUILayout.EndHorizontal();
            searchChanged = searchFilter != lastSearch;

            GUIUtils.EndBox(1);
        }
    }
}