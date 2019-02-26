using UnityEngine;
using UnityEditor;
namespace AssetObjectsPacks {
    public class SearchFilter 
    {
        string searchFilter;
        public bool PassesSearchFilter(string text) {
            if (searchFilter.IsEmpty()) return true;
            return text.ToLower().Contains(searchFilter.ToLower());
        }
        public bool SearchBarGUI () {
            EditorGUILayout.BeginHorizontal();
            GUIUtils.Label(new GUIContent("Search:"), true);
            string lastSearch = searchFilter;
            GUIUtils.NextControlOverridesKeyboard();
            searchFilter = EditorGUILayout.DelayedTextField(GUIUtils.blank_content, searchFilter, GUILayout.MinWidth(32));
            //if clicked outside, lose focus
            GUIUtils.CheckLoseFocusLastRect();
            EditorGUILayout.EndHorizontal();
            return searchFilter != lastSearch;
        }
    }
}
