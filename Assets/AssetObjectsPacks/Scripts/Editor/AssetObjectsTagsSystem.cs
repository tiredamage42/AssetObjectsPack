using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace AssetObjectsPacks {
    public class TagObjectSystem {
        public class TagsListTracker {
            public SerializedProperty tags_prop;
            public int object_id;
            public TagsListTracker(SerializedProperty tags_prop, int object_id) {
                this.tags_prop = tags_prop;
                this.object_id = object_id;
            }
        }
        List<TagsListTracker> list_trackers;
        public bool[] keywords_filter;
        SearchKeywordsGUI search_keywords = new SearchKeywordsGUI();
        AssetObjectTagsGUI tags_gui = new AssetObjectTagsGUI();
        public delegate List<TagsListTracker> RBLISTTRACKERS();
        public RBLISTTRACKERS rebuild_list_trackers;
        List<SerializedProperty> selected_tags_props = new List<SerializedProperty>();
        List<string> all_tags;
        SerializedObject serializedObject;

        string pack_name;
        public void OnEnable (string pack_name, RBLISTTRACKERS rebuild_list_trackers, SerializedObject serializedObject) {
            this.pack_name = pack_name;
            this.serializedObject = serializedObject;
            this.rebuild_list_trackers = rebuild_list_trackers;
            all_tags = AssetObjectsEditor.LoadAllTags(pack_name);
            search_keywords.OnEnable(OnSearchKeywordsChange, all_tags);
            tags_gui.OnEnable(OnTagsChanged);
        }
        public void DrawTagsSearch() {
            search_keywords.DrawTagSearch();
        }       
        public void RebuildListTrackers (List<int> selected_ids) {
            list_trackers = rebuild_list_trackers();
            keywords_filter = new bool[list_trackers.Count];
            UpdateSearchTagsFilter();
            OnSelectionChanged(selected_ids);
        }
        public void OnSelectionChanged(List<int> selected_ids) {
            tags_gui.selection_changed = true;
            RebuildSelectedTagsProps(selected_ids);
        }
        bool HasSearchTags (SerializedProperty tags_prop, int keywords_count) {
            if (keywords_count == 0) return true;
            for (int i = 0; i < keywords_count; i++) {
                if (tags_prop.Contains(search_keywords.keywords[i])) {
                    return true;
                }
            }
            return false;
        }
        void UpdateSearchTagsFilter() {
            int c = list_trackers.Count;
            int keywords_count = search_keywords.keywords.Count;
            for (int i = 0; i < c; i++) {
                keywords_filter[i] = HasSearchTags(list_trackers[i].tags_prop, keywords_count);
            }
        }
        void OnTagsChanged (string changed_tag) {
            if (!all_tags.Contains(changed_tag)) {
                all_tags.Add(changed_tag);
                AssetObjectsEditor.SaveAllTags(pack_name, all_tags);
            }
            search_keywords.RepopulatePopupList(all_tags);
            UpdateSearchTagsFilter();
            serializedObject.ApplyModifiedProperties();
        }
        public void OnInteractivePreviewGUI(Rect r, GUIStyle background) {
            if (selected_tags_props.Count != 0) {       
                tags_gui.OnInteractivePreviewGUI(selected_tags_props, all_tags);
            }
        }
        void RebuildSelectedTagsProps (List<int> selected_ids) {
            selected_tags_props.Clear();
            if (selected_ids.Count != 0) {
                int c = list_trackers.Count;
                for (int i = 0; i < c; i++) {
                    if (selected_ids.Contains(list_trackers[i].object_id)) {
                        selected_tags_props.Add(list_trackers[i].tags_prop);
                    }
                }
            }
        }
       
        void OnSearchKeywordsChange () {
            UpdateSearchTagsFilter();
            search_keywords.RepopulatePopupList(all_tags);
        }
    }
}




