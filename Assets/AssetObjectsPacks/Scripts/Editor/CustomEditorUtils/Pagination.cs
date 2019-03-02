using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
namespace AssetObjectsPacks {
    public enum PaginationAttempt { None = 0, Fwd = 1, Back = 2, }
    public class Pagination {
        public void ResetPage() {
            page = 0;
        }
        int page, max_pages;

        public IList<T> Paginate<T> (IList<T> unpaginated, int elementsPerPage = 50) {
            int l = unpaginated.Count;
            max_pages = (l / elementsPerPage) + Mathf.Min(1, l % elementsPerPage);
            if (page >= max_pages) page = Mathf.Max(0, max_pages - 1);
            int min = page * elementsPerPage;
            int max = Mathf.Min(min + elementsPerPage, l - 1);
            return unpaginated.Slice(min, max);
        }

        bool SwitchPage(int offset) {
            int newVal = page + offset;
            if (newVal < 0 || newVal >= max_pages) return false;
            page += offset;
            return true;
        }

        public PaginationAttempt DrawPages (KeyboardListener k, out bool wasSuccess){
            PaginationAttempt r = PaginationAttempt.None;
            
            EditorGUILayout.BeginHorizontal();

            //if (GUIUtils.Button(new GUIContent(" << "), GUIStyles.toolbarButton, true) || k[KeyCode.LeftArrow]) r = PaginationAttempt.Back;
            if (GUIUtils.Button(new GUIContent(" << "), GUIStyles.toolbarButton, true)) r = PaginationAttempt.Back;
            
            GUIStyle s = GUIStyles.label;
            TextAnchor ol = s.alignment;
            s.alignment = TextAnchor.LowerCenter;
            GUIUtils.Label(new GUIContent("Page: " + (page + 1) + " / " + max_pages));
            s.alignment = ol;

            //if (GUIUtils.Button(new GUIContent(" >> "), GUIStyles.toolbarButton, true) || k[KeyCode.RightArrow]) r = PaginationAttempt.Fwd;
            if (GUIUtils.Button(new GUIContent(" >> "), GUIStyles.toolbarButton, true)) r = PaginationAttempt.Fwd;
            
            EditorGUILayout.EndHorizontal();
            
            wasSuccess = r != PaginationAttempt.None && SwitchPage(r == PaginationAttempt.Fwd ? 1 : -1);
            return r;      
        }

    }
}
