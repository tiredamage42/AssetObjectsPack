using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
namespace AssetObjectsPacks {
    public enum PaginationAttempt { None = 0, Fwd = 1, Back = 2, }
    public class Pagination {
        public void ResetPage() {
            page = 0;
        }

        int elementsPerPage = 30;
        int page, max_pages;

        public IList<T> Paginate<T> (IList<T> unpaginatedElements) {
            int l = unpaginatedElements.Count;

            max_pages = (l / elementsPerPage) + Mathf.Min(1, l % elementsPerPage);
            if (page >= max_pages) page = Mathf.Max(0, max_pages - 1);
            
            int min = page * elementsPerPage;
            int max = Mathf.Min(min + elementsPerPage, l - 1);
            return unpaginatedElements.Slice(min, max);
        }

        bool SwitchPage(int offset) {
            int newVal = page + offset;
            if (newVal < 0 || newVal >= max_pages) return false;
            page += offset;
            return true;
        }

        public PaginationAttempt DrawPages (KeyboardListener kbListener, out bool wasSuccess){
            PaginationAttempt r = PaginationAttempt.None;
            
            GUIUtils.StartBox(0);
            EditorGUILayout.BeginHorizontal();

            GUI.enabled = page > 0;
            if (GUIUtils.Button(new GUIContent(" << "), true, GUIStyles.toolbarButton) || kbListener[KeyCode.LeftArrow]) r = PaginationAttempt.Back;
            GUI.enabled = true;

            GUIStyle s = GUIStyles.label;
            TextAnchor ol = s.alignment;
            s.alignment = TextAnchor.LowerCenter;
            GUIUtils.Label(new GUIContent("Page: " + (page + 1) + " / " + max_pages), false);
            s.alignment = ol;

            GUI.enabled = page < max_pages;
            if (GUIUtils.Button(new GUIContent(" >> "), true, GUIStyles.toolbarButton) || kbListener[KeyCode.RightArrow]) r = PaginationAttempt.Fwd;
            GUI.enabled = true;
            
            EditorGUILayout.EndHorizontal();
            GUIUtils.EndBox(0);     

            wasSuccess = r != PaginationAttempt.None && SwitchPage(r == PaginationAttempt.Fwd ? 1 : -1);
            return r;      
        }

    }
}
