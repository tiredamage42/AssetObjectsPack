
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEditor;
using UnityEditorInternal;
using UnityEditor.Animations;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Object = UnityEngine.Object;

namespace AssetObjectsPacks {

    public class PopupList : PopupWindowContent {
        public delegate void OnSelectCallback(ListElement element);
        public enum Gravity { Top, Bottom }
        public class ListElement {
            public GUIContent m_Content;
            bool m_Selected, m_WasSelected, m_PartiallySelected, m_Enabled;
            public ListElement(string text, bool selected) {
                m_Content = new GUIContent(text);
                if (!string.IsNullOrEmpty(m_Content.text)) {
                    char[] a = m_Content.text.ToCharArray();
                    a[0] = char.ToUpper(a[0]);
                    m_Content.text = new string(a);
                }
                m_Selected = selected;
                m_PartiallySelected = false;
                m_Enabled = true;
            }

            public bool selected{
                get{
                    return m_Selected;
                }
                set{
                    m_Selected = value;
                    if (m_Selected)
                        m_WasSelected = true;
                }
            }
            public bool enabled{
                get{
                    return m_Enabled;
                }
                set{
                    m_Enabled = value;
                }
            }
            public bool partiallySelected{
                get{
                    return m_PartiallySelected;
                }
                set{
                    m_PartiallySelected = value;
                    if (m_PartiallySelected)
                        m_WasSelected = true;
                }
            }
            public string text{
                get{
                    return m_Content.text;
                }
                set{
                    m_Content.text = value;
                }
            }
            public void ResetScore(){
                m_WasSelected = m_Selected || m_PartiallySelected;
            }
        }
        public class InputData{
            public List<ListElement> m_ListElements;
            public bool m_EnableAutoCompletion = true;
            public OnSelectCallback m_OnSelectCallback;
            public int m_MaxCount;
            public InputData(){
                m_ListElements = new List<ListElement>();
            }
            public void DeselectAll(){
                foreach (ListElement element in m_ListElements){
                    element.selected = false;
                    element.partiallySelected = false;
                }
            }
            public void ResetScores(){
                foreach (var element in m_ListElements)
                    element.ResetScore();
            }
            public virtual IEnumerable<ListElement> BuildQuery(string prefix){
                if (prefix == "")
                    return m_ListElements;
                else
                    return m_ListElements.Where(
                        element => element.m_Content.text.StartsWith(prefix, System.StringComparison.OrdinalIgnoreCase)
                    );
            }
            public IEnumerable<ListElement> GetFilteredList(string prefix){
                IEnumerable<ListElement> res = BuildQuery(prefix);
                if (m_MaxCount > 0)
                    res = res.Take(m_MaxCount);
                return res;
            }
            public int GetFilteredCount(string prefix) {
                IEnumerable<ListElement> res = BuildQuery(prefix);
                if (m_MaxCount > 0)
                    res = res.Take(m_MaxCount);
                return res.Count();
            }
            public ListElement NewOrMatchingElement(string label) {
                foreach (var element in m_ListElements) {
                    if (element.text.Equals(label, StringComparison.OrdinalIgnoreCase))
                        return element;
                }
                var res = new ListElement(label, false);//, -1);
                m_ListElements.Add(res);
                return res;
            }
        }
        private class Styles{
            public GUIStyle menuItem = "MenuItem";
            public GUIStyle menuItemMixed = "MenuItemMixed";
            public GUIStyle background = "grey_border";
            public GUIStyle customTextField;
            public GUIStyle customTextFieldCancelButton;
            public GUIStyle customTextFieldCancelButtonEmpty;
            public Styles(){
                customTextField = new GUIStyle(GUIUtils.GetStyle("ToolbarSeachTextField"));// EditorStyles.toolbarSearchField);
                customTextFieldCancelButton = new GUIStyle(GUIUtils.GetStyle("ToolbarSeachCancelButton"));//EditorStyles.toolbarSearchFieldCancelButton);
                customTextFieldCancelButtonEmpty = new GUIStyle(GUIUtils.GetStyle("ToolbarSeachCancelButtonEmpty"));//EditorStyles.toolbarSearchFieldCancelButtonEmpty);
            }
        }

        // Static
        static Styles s_Styles;
        // State
        private InputData m_Data;
        // Layout
        const float k_LineHeight = 16;
        const float k_TextFieldHeight = 16;
        const float k_Margin = 10;
        Gravity m_Gravity;
        string m_EnteredTextCompletion = "";
        string m_EnteredText = "";
        int m_SelectedCompletionIndex = 0;
        public PopupList(InputData inputData) : this(inputData, null) {}
        public PopupList(InputData inputData, string initialSelectionLabel){
            m_Data = inputData;
            m_Data.ResetScores();
            SelectNoCompletion();
            m_Gravity = Gravity.Top;
            if (initialSelectionLabel != null){
                m_EnteredTextCompletion = initialSelectionLabel;
                UpdateCompletion();
            }
        }
        public override void OnClose(){
            if (m_Data != null)
                m_Data.ResetScores();
        }
        public virtual float GetWindowHeight(){
            int count = (m_Data.m_MaxCount == 0) ? m_Data.GetFilteredCount(m_EnteredText) : m_Data.m_MaxCount;
            return count * k_LineHeight + 2 * k_Margin + (k_TextFieldHeight);
        }
        public virtual float GetWindowWidth(){
            return 150f;
        }
        public override Vector2 GetWindowSize(){
            return new Vector2(GetWindowWidth(), GetWindowHeight());
        }
        public override void OnGUI(Rect windowRect){
            Event evt = Event.current;
            // We do not use the layout event
            if (evt.type == EventType.Layout)
                return;
            if (s_Styles == null)
                s_Styles = new Styles();
            if (evt.type == EventType.KeyDown && evt.keyCode == KeyCode.Escape){
                editorWindow.Close();
                GUIUtility.ExitGUI();
            }
            if (m_Gravity == Gravity.Bottom){
                DrawList(editorWindow, windowRect);
                DrawCustomTextField(editorWindow, windowRect);
            }
            else{
                DrawCustomTextField(editorWindow, windowRect);
                DrawList(editorWindow, windowRect);
            }
            // Background with 1 pixel border (rendered above content)
            if (evt.type == EventType.Repaint)
                s_Styles.background.Draw(new Rect(windowRect.x, windowRect.y, windowRect.width, windowRect.height), false, false, false, false);
        }
        private void DrawCustomTextField(EditorWindow editorWindow, Rect windowRect){
            Event evt = Event.current;
            bool enableAutoCompletion = m_Data.m_EnableAutoCompletion;
            bool closeWindow = false;
            bool useEventBeforeTextField = false;
            bool clearText = false;
            string textBeforeEdit = CurrentDisplayedText();
            // Handle "special" keyboard input
            if (evt.type == EventType.KeyDown){
                switch (evt.keyCode){
                    case KeyCode.Comma:
                    case KeyCode.Space:
                    case KeyCode.Tab:
                    case KeyCode.Return:
                        if (textBeforeEdit != ""){
                            // Toggle state
                            if (m_Data.m_OnSelectCallback != null)
                                m_Data.m_OnSelectCallback(m_Data.NewOrMatchingElement(textBeforeEdit));
                            if (evt.keyCode == KeyCode.Tab || evt.keyCode == KeyCode.Comma)
                                clearText = true;  // to ease multiple entries (it is unlikely that the same filter is used more than once)
                            // Auto close
                            closeWindow = true;
                        }
                        useEventBeforeTextField = true;
                        break;
                    case KeyCode.Delete:
                    case KeyCode.Backspace:
                        enableAutoCompletion = false;
                        // Don't use the event yet, so the textfield below can get it and delete the selection
                        break;
                    case KeyCode.DownArrow:
                        ChangeSelectedCompletion(1);
                        useEventBeforeTextField = true;
                        break;
                    case KeyCode.UpArrow:
                        ChangeSelectedCompletion(-1);
                        useEventBeforeTextField = true;
                        break;
                    case KeyCode.None:
                        if (evt.character == ' ' || evt.character == ',')
                            useEventBeforeTextField = true;
                        break;
                }
            }
            string textFieldText;
            // Draw textfield
            {
                Rect pos = new Rect(windowRect.x + k_Margin / 2, windowRect.y + (m_Gravity == Gravity.Top ? (k_Margin / 2) : (windowRect.height - k_TextFieldHeight - k_Margin / 2)), windowRect.width - k_Margin - 14, k_TextFieldHeight);   
                if (useEventBeforeTextField)
                    evt.Use();  // We have to delay this until after we get the control id, otherwise the id we get is just -1

                textFieldText = EditorGUI.TextField(pos, textBeforeEdit, s_Styles.customTextField);

                Rect buttonRect = pos;
                buttonRect.x += pos.width;
                buttonRect.width = 14;
                // Draw "clear textfield" button (X)
                if ((GUI.Button(buttonRect, GUIContent.none, textFieldText != "" ? s_Styles.customTextFieldCancelButton : s_Styles.customTextFieldCancelButtonEmpty) && textFieldText != "") || clearText){
                    textFieldText = "";
                    enableAutoCompletion = false;
                }
            }

            // Handle autocompletion
            if (textBeforeEdit != textFieldText){
                m_EnteredText = textFieldText;
                if (enableAutoCompletion)
                    UpdateCompletion();
                else
                    SelectNoCompletion();
            }
            if (closeWindow)
                editorWindow.Close();
        }
        private string CurrentDisplayedText(){
            return m_EnteredTextCompletion != "" ? m_EnteredTextCompletion : m_EnteredText;
        }
        private void UpdateCompletion(){
            if (!m_Data.m_EnableAutoCompletion)
                return;
            IEnumerable<string> query = m_Data.GetFilteredList(m_EnteredText).Select(element => element.text);
            if (m_EnteredTextCompletion != "" && m_EnteredTextCompletion.StartsWith(m_EnteredText, System.StringComparison.OrdinalIgnoreCase)){
                m_SelectedCompletionIndex = query.TakeWhile(element => element != m_EnteredTextCompletion).Count();
                // m_EnteredTextCompletion is already correct
            }
            else{
                // Clamp m_SelectedCompletionIndex to 0..query.Count () - 1
                if (m_SelectedCompletionIndex < 0)
                    m_SelectedCompletionIndex = 0;
                else if (m_SelectedCompletionIndex >= query.Count())
                    m_SelectedCompletionIndex = query.Count() - 1;
                m_EnteredTextCompletion = query.Skip(m_SelectedCompletionIndex).DefaultIfEmpty("").FirstOrDefault();
            }
        }
        private void ChangeSelectedCompletion(int change){
            int count = m_Data.GetFilteredCount(m_EnteredText);
            if (m_SelectedCompletionIndex == -1 && change < 0)  // specal case for initial selection
                m_SelectedCompletionIndex = count;
            int index = count > 0 ? (m_SelectedCompletionIndex + change + count) % count : 0;
            SelectCompletionWithIndex(index);
        }
        private void SelectCompletionWithIndex(int index){
            m_SelectedCompletionIndex = index;
            m_EnteredTextCompletion = "";
            UpdateCompletion();
        }
        private void SelectNoCompletion(){
            m_SelectedCompletionIndex = -1;
            m_EnteredTextCompletion = "";
        }
        private void DrawList(EditorWindow editorWindow, Rect windowRect){
            Event evt = Event.current;
            int i = -1;
            foreach (var element in m_Data.GetFilteredList(m_EnteredText)){
                i++;
                Rect rect = new Rect(windowRect.x, windowRect.y + k_Margin + i * k_LineHeight + (m_Gravity == Gravity.Top ? k_TextFieldHeight : 0), windowRect.width, k_LineHeight);
                switch (evt.type){
                    case EventType.Repaint:{
                        GUIStyle style = element.partiallySelected ? s_Styles.menuItemMixed : s_Styles.menuItem;
                        bool selected = element.selected || element.partiallySelected;
                        bool focused = false;
                        bool isHover = i == m_SelectedCompletionIndex;
                        bool isActive = selected;
                        using (new EditorGUI.DisabledScope(!element.enabled)){
                            GUIContent content = element.m_Content;
                            style.Draw(rect, content, isHover, isActive, selected, focused);
                        }
                    }
                    break;
                    case EventType.MouseDown:{
                        if (Event.current.button == 0 && rect.Contains(Event.current.mousePosition) && element.enabled) {
                            // Toggle state
                            if (m_Data.m_OnSelectCallback != null)
                                m_Data.m_OnSelectCallback(element);
                            evt.Use();
                            editorWindow.Close();
                        }
                    }
                    break;
                    case EventType.MouseMove:{
                        if (rect.Contains(Event.current.mousePosition)){
                            SelectCompletionWithIndex(i);
                            evt.Use();
                        }
                    }
                    break;
                }
            }
        }
    }
    


    public class AssetObjectTagsGUI {
        const int max_shown_tags = 10;
        Action<string> on_tags_change;
        HashSet<SerializedProperty> current_tags_props_set;
        PopupList.InputData popup_list_data;
        string changed_tag;
        bool is_changed, change_was_add;
        
        public void OnEnable (Action<string> on_tags_change) {
            this.on_tags_change = on_tags_change;
            //EditorApplication.projectChanged += InvalidateLabels;
        }
        //public void OnDisable() {
            //EditorApplication.projectChanged -= InvalidateLabels;
        //    SaveTags();
        //}
        //public void InvalidateLabels() {
        //    popup_list_data = null;
        //    current_tags_props_set = null;
        //}
        public void SaveTags() {
            if (is_changed && popup_list_data != null && current_tags_props_set != null) {
                foreach (SerializedProperty tags_prop in current_tags_props_set) {
                    int i;
                    if (change_was_add) {
                        if (!tags_prop.Contains(changed_tag, out i)) {
                            i = tags_prop.arraySize;
                            tags_prop.InsertArrayElementAtIndex(i);
                            tags_prop.GetArrayElementAtIndex(i).stringValue = changed_tag;
                        }
                    }
                    else {
                        tags_prop.Remove(changed_tag);
                    }
                }
                on_tags_change(changed_tag);
                is_changed = false;
            }
        }
        public void PopupListCallback(PopupList.ListElement element) {
            changed_tag = element.text;
            element.selected = !element.selected;
            change_was_add = element.selected;
            element.partiallySelected = false;
            is_changed = true;
            SaveTags();
        }

        public bool selection_changed;

        void InitTagCaches(List<SerializedProperty> tags_lists, List<string> all_tags) {
            if (current_tags_props_set == null || selection_changed)
            {
                List<string> all;
                List<string> partial;
                GetTagsInLists(tags_lists, out all, out partial);
                popup_list_data = new PopupList.InputData {
                    m_OnSelectCallback = PopupListCallback,
                    m_MaxCount = 15
                };
                foreach (var tag in all_tags) {
                    PopupList.ListElement element = popup_list_data.NewOrMatchingElement(tag);
                    element.selected = all.Any(label => string.Equals(label, tag, StringComparison.OrdinalIgnoreCase));
                    element.partiallySelected = partial.Any(label => string.Equals(label, tag, StringComparison.OrdinalIgnoreCase));
                }
                selection_changed = false;
            }
            current_tags_props_set = new HashSet<SerializedProperty>(tags_lists);
            is_changed = false;
        }

        public void OnInteractivePreviewGUI(List<SerializedProperty> tags_lists, List<string> all_tags)
        {
            InitTagCaches(tags_lists, all_tags);
            // For the label list as a whole
            // The previous layouting means we've already lost a pixel to the left and couple at the top, so it is an attempt at horizontal padding: 3, verical padding: 5
            // (the rounded sides of labels makes this look like the horizontal and vertical padding is the same)
            float leftPadding = 1.0f;
            float rightPadding = 2.0f;
            float topPadding = 3.0f;
            float bottomPadding = 5.0f;
            GUIStyle labelButton = GUIUtils.GetStyle("AssetLabel Icon");
            float buttonWidth = labelButton.margin.left + labelButton.fixedWidth + rightPadding;
            // Assumes we are already in a vertical layout
            GUILayout.Space(topPadding);
            // Create a rect to test how wide the label list can be
            Rect widthProbeRect = GUILayoutUtility.GetRect(0, 10240, 0, 0);
            widthProbeRect.width -= buttonWidth; // reserve some width for the button
            EditorGUILayout.BeginHorizontal();
            // Left padding
            GUILayoutUtility.GetRect(leftPadding, leftPadding, 0, 0);
            // Draw labels (fully selected)
            DrawSelectedTagsList(false, widthProbeRect.xMax);
            // Draw labels (partially selected)
            DrawSelectedTagsList(true, widthProbeRect.xMax);
            GUILayout.FlexibleSpace();
            Rect r = GUILayoutUtility.GetRect(labelButton.fixedWidth, labelButton.fixedWidth, labelButton.fixedHeight + bottomPadding, labelButton.fixedHeight + bottomPadding);
            r.x = widthProbeRect.xMax + labelButton.margin.left;
            if (EditorGUI.DropdownButton(r, GUIContent.none, FocusType.Passive, labelButton)) {
                PopupWindow.Show(r, new PopupList(popup_list_data));
            }
            EditorGUILayout.EndHorizontal();
        }

        void DrawSelectedTagsList(bool partiallySelected, float xMax)
        {
            GUIStyle labelStyle = partiallySelected ? GUIUtils.GetStyle("AssetLabel Partial") : GUIUtils.GetStyle("AssetLabel");
            Event evt = Event.current;
            foreach (GUIContent content in (from i in popup_list_data.m_ListElements where (partiallySelected ? i.partiallySelected : i.selected) orderby i.text.ToLower() select i.m_Content).Take(max_shown_tags))
            {
                Rect rt = GUILayoutUtility.GetRect(content, labelStyle);
                if (Event.current.type == EventType.Repaint && rt.xMax >= xMax)
                    break;
                GUI.Label(rt, content, labelStyle);
                if (rt.xMax <= xMax && evt.type == EventType.MouseDown && rt.Contains(evt.mousePosition) && evt.button == 0 && GUI.enabled)
                {
                    evt.Use();
                    rt.x = xMax;
                    PopupWindow.Show(rt, new PopupList(popup_list_data, content.text));
                }
            }
        }

        void GetTagsInLists(List<SerializedProperty> tags_lists, out List<string> all, out List<string> partial) {
            all = new List<string>();
            partial = new List<string>();
            Dictionary<string, int> labelAssetCount = new Dictionary<string, int>();
            foreach (SerializedProperty tags in tags_lists) {
                for (int i = 0; i < tags.arraySize; i++) {
                    string label = tags.GetArrayElementAtIndex(i).stringValue;
                    labelAssetCount[label] = labelAssetCount.ContainsKey(label) ? labelAssetCount[label] + 1 : 1;   
                }
            }
            foreach (KeyValuePair<string, int> entry in labelAssetCount) {
                var list = (entry.Value == tags_lists.Count) ? all : partial;
                list.Add(entry.Key);
            }
        }
    }

    
    public class SearchKeywordsGUI {
        const int max_shown_keywords = 10;
        public List<string> keywords = new List<string>();
        PopupList.InputData popup_list_data;
        string changed_keyword;
        bool changed_any_keyword, m_ChangeWasAdd, gui_initialized;
        Action on_keywords_change;
        GUIStyle tag_style;
        GUIContent gui_SearchTags;
        void InitializeGUIStuff () {
            if (gui_initialized)
                return;
            tag_style = GUIUtils.GetStyle("AssetLabel");
            gui_SearchTags = new GUIContent("Search Tags:");
            gui_initialized = true;
        }
        public void OnEnable (System.Action on_keywords_change, List<string> all_tags) {
            this.on_keywords_change = on_keywords_change;
            RepopulatePopupList(all_tags);
        }
        void SaveLabels() {
            if (changed_any_keyword && popup_list_data != null){
                if (m_ChangeWasAdd) {
                    if (!keywords.Contains(changed_keyword)) {
                        keywords.Add(changed_keyword);
                    }
                }
                else {
                    keywords.Remove(changed_keyword);
                }
                on_keywords_change();
                changed_any_keyword = false;
            }
        }
        public void AssetLabelListCallback(PopupList.ListElement element) {
            changed_keyword = element.text;
            element.selected = !element.selected;
            m_ChangeWasAdd = element.selected;
            element.partiallySelected = false;
            changed_any_keyword = true;
            SaveLabels();
        }
        public void RepopulatePopupList (List<string> all_tags) {
            popup_list_data = new PopupList.InputData {
                m_OnSelectCallback = AssetLabelListCallback,
                m_MaxCount = 15,
            };
            foreach (var tag in all_tags) {
                PopupList.ListElement element = popup_list_data.NewOrMatchingElement(tag);
                element.selected = keywords.Contains(tag);
                element.partiallySelected = false;
            }
            foreach (string keyword in keywords) {
                if (!all_tags.Contains(keyword)) {
                    PopupList.ListElement element = popup_list_data.NewOrMatchingElement(keyword);
                    element.selected = true;
                    element.partiallySelected = false;
                }
            }
        }
        public void DrawTagSearch() {
            InitializeGUIStuff();
            EditorGUILayout.BeginHorizontal();            
            if (GUILayout.Button(gui_SearchTags)) {
                PopupWindow.Show(GUILayoutUtility.GetRect(gui_SearchTags, tag_style), new PopupList(popup_list_data));
            }
            GUILayout.FlexibleSpace();
            DrawKeywordsList( );
            EditorGUILayout.EndHorizontal();
        }
        void DrawKeywordsList() {
            Event evt = Event.current;
            foreach (string keyword in keywords.Take(max_shown_keywords)){
                GUIContent content = new GUIContent(keyword);
                Rect rt = GUILayoutUtility.GetRect(content, tag_style);
                GUI.Label(rt, content, tag_style);
                if (evt.type == EventType.MouseDown && rt.Contains(evt.mousePosition) && evt.button == 0 && GUI.enabled) {
                    evt.Use();
                    PopupWindow.Show(rt, new PopupList(popup_list_data, content.text));
                }
            }
        }
    }    
}




