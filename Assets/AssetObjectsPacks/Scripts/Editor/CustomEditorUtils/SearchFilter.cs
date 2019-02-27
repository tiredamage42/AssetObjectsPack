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
            //EditorGUILayout.BeginHorizontal();
            
            //GUIUtils.Label(new GUIContent("Search:"), true);
            
            string lastSearch = searchFilter;
            
            GUIUtils.NextControlOverridesKeyboard();

            string defaultSearch = searchFilter.IsEmpty() ? "Search" : searchFilter;
            


            string searchResult = EditorGUILayout.DelayedTextField(GUIUtils.blank_content, defaultSearch, GUILayout.MinWidth(32));
            
            if (searchResult != defaultSearch) {
                searchFilter = searchResult;
            }
            //if clicked outside, lose focus
            GUIUtils.CheckLoseFocusLastRect();
            
            
            //EditorGUILayout.EndHorizontal();
            return searchFilter != lastSearch;
        }
    }
}
