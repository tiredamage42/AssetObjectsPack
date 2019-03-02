using UnityEngine;
using UnityEditor;
namespace AssetObjectsPacks {
    public class SearchFilter {
        string searchFilter;
        
        public bool PassesSearchFilter(string text) {
            if (searchFilter.IsEmpty()) return true;
            return text.ToLower().Contains(searchFilter.ToLower());
        }
        public bool SearchBarGUI () {
            string lastSearch = searchFilter;
            string defaultSearch = searchFilter.IsEmpty() ? "Search" : searchFilter;
            
            string searchResult = GUIUtils.DrawTextField(defaultSearch, GUIUtils.TextFieldType.Delayed, true, out _, GUILayout.MaxWidth(128));
            
            if (searchResult != defaultSearch) searchFilter = searchResult;
            
            return searchFilter != lastSearch;
        }
    }
}
