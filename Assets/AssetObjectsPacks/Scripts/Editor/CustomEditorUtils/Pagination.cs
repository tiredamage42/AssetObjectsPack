using System.Collections.Generic;
using UnityEngine;
//using UnityEditor;
namespace AssetObjectsPacks {
    public class Pagination {
        public void ResetPage() {
            page = 0;
        }

        public GUIContent pagesGUI;
        int page, max_pages;

        public IList<T> Paginate<T> (IList<T> unpaginated, int elementsPerPage = 50) {
            int l = unpaginated.Count;
            max_pages = (l / elementsPerPage) + Mathf.Min(1, l % elementsPerPage);
            if (page >= max_pages) page = Mathf.Max(0, max_pages - 1);
            int min = page * elementsPerPage;
            int max = Mathf.Min(min + elementsPerPage, l - 1);

            pagesGUI = new GUIContent("Page: " + (page + 1) + " / " + max_pages);
            return unpaginated.Slice(min, max);
        }


        public bool SwitchPage(int offset) {
            int newVal = page + offset;
            if (newVal < 0 || newVal >= max_pages) return false;
            page += offset;
            //pagesGUI = new GUIContent("Page: " + (page + 1) + " / " + max_pages);
            return true;
        }
    }
}
