﻿using UnityEngine;
using System.Collections.Generic;
using UnityEditor;

namespace AssetObjectsPacks {
    public class FoldersView {
        string currentDirectory = "";
        int foldersHiearchyOffset;
        GUIContent backGUI;
        void UpdateBackGUI () {
            backGUI = new GUIContent( " <<  '" + currentDirectory + "'");
        }
        public void MoveForward (string directory) {
            foldersHiearchyOffset++;
            currentDirectory += directory + "/";
            UpdateBackGUI();
        }
        public bool MoveBackward () {
            if (foldersHiearchyOffset <= 0) return false;
            foldersHiearchyOffset--;
            string without_last_slash = currentDirectory.Substring(0, currentDirectory.Length-1);
            int new_cutoff = without_last_slash.LastIndexOf("/") + 1;
            currentDirectory = currentDirectory.Substring(0, new_cutoff);
            UpdateBackGUI();
            return true;
        }
        public bool DrawBackButton () {
            if (foldersHiearchyOffset > 0 && GUIUtils.Button(backGUI, true)) return MoveBackward();
            return false;
        }
        public bool DisplaysPath(string path){
            return currentDirectory.IsEmpty() || path.StartsWith(currentDirectory);
        }
        public string DisplayNameFromPath (string path) {
            if (path.Contains("/")) return path.Split('/')[foldersHiearchyOffset];    
            return path;
        }       
    }

    public class PaginateView {
        int page, max_pages;
        GUIStyle _ls = null;
        GUIStyle label_s {
            get {
                if (_ls == null) {
                    _ls = new GUIStyle(EditorStyles.label);
                    _ls.alignment = TextAnchor.LowerCenter;
                    _ls.normal.textColor = EditorColors.light_gray;   
                }
                return _ls;
            }
        }

        GUIContent back_gui = new GUIContent(" << "), fwd_gui = new GUIContent(" >> ");
        GUIContent page_gui;

        public void ResetPage() {
            page = 0;
        }

        public bool NextPage() {
            if (page+1 >= max_pages) return false;
            page++;
            return true;
        }
        public bool PreviousPage () {
            if (page-1 < 0) return false;
            page--;
            return true;
        }
        public void Paginate(int count, int perPage, out int min, out int max) {
            max_pages = (count / perPage) + Mathf.Min(1, count % perPage);

            min = page * perPage;
            max = Mathf.Min(min + perPage, count - 1);

            page_gui = new GUIContent("Page: " + (page + 1) + " / " + max_pages);
        }
        
        public bool ChangePageGUI () {
            bool changed_page = false;

            GUIUtils.StartBox(EditorColors.med_gray);

            EditorGUILayout.BeginHorizontal();
            if (GUIUtils.Button(back_gui, true)) changed_page = PreviousPage();

            EditorGUILayout.LabelField(page_gui, label_s);
            
            if (GUIUtils.Button(fwd_gui, true)) changed_page = NextPage();
            EditorGUILayout.EndHorizontal();

            GUIUtils.EndBox();
            
            return changed_page;
        }
    }

    
    public class HiddenView {
        public bool showHidden;
        HashSet<int> hiddenIds = new HashSet<int>();
        GUIContent show_hidden_gui = new GUIContent("Show Hidden");
        GUIContent reset_hidden_gui = new GUIContent("Reset Hidden");
        GUIContent hideSelectedGUI = new GUIContent("Hide Selected");
        GUIContent unhideSelectedGUI = new GUIContent("Unhide Selected");
            

        public bool IsHidden (int id) {
            return hiddenIds.Contains(id);
        }
        public void HideIDs(HashSet<int> ids) {
            foreach (var e in ids) hiddenIds.Add(e);
        }
        public void UnhideIDs(HashSet<int> ids) {    
            foreach (var e in ids) hiddenIds.Remove(e);
        }
        public bool IDsAreAlltheSameHiddenStatus(HashSet<int> ids, out bool hidden_status) {
            hidden_status = false;
            if (ids.Count == 0 || hiddenIds.Count == 0){
                return true;
            }
            bool was_checked = false;
            foreach (var id in ids) {
                bool is_hidden = hiddenIds.Contains(id);
                if (!was_checked) {
                    hidden_status = is_hidden;
                    was_checked = true;
                }
                else {
                    if (is_hidden != hidden_status) return false;
                }
            }
            return true;
        }
        public bool ToggleShowHiddenButton () {
            GUI.enabled = hiddenIds.Count != 0;
            bool last_show_hidden = showHidden;
            showHidden = GUIUtils.ToggleButton(show_hidden_gui, true, showHidden);
            GUI.enabled = true;
            return last_show_hidden != showHidden;
        }
        public bool ResetHiddenButton () {
            bool pressed = false;
            GUI.enabled = hiddenIds.Count != 0;
            if (GUIUtils.Button(reset_hidden_gui, true)) {
                hiddenIds.Clear();
                pressed=true;
            }
            GUI.enabled = true;
            return pressed;
        }

        public bool ToggleHiddenSelected (HashSet<int> idsFromElements, bool hidden_status, bool check_hidden_status = true) {
            if (check_hidden_status && !IDsAreAlltheSameHiddenStatus( idsFromElements, out hidden_status )) return false;
            
            if (hidden_status) UnhideIDs( idsFromElements );
            else HideIDs( idsFromElements );

            return !showHidden;        
        }

        public bool ToggleHiddenSelectedButton (HashSet<int> selected_ids) {
            bool pressed = false;
            
            bool hidden_status = false;
            if (GUI.enabled) GUI.enabled = IDsAreAlltheSameHiddenStatus( selected_ids, out hidden_status );
            
            if (GUIUtils.Button(hidden_status ? unhideSelectedGUI : hideSelectedGUI, true)) pressed = ToggleHiddenSelected(selected_ids, hidden_status, false);

            GUI.enabled = true;
            
            return pressed;
            
        }
    }

}
